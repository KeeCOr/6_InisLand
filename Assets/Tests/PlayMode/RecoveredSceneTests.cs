using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace IL6.Tests
{
    public class RecoveredSceneTests
    {
        [UnityTest]
        public IEnumerator BootToSnowfield_InitializesWorldAndRuntimeSprites()
        {
            yield return SceneManager.LoadSceneAsync("BootScene");
            yield return null;
            yield return null;
            Assert.That(GameSession.Instance, Is.Not.Null);
            Assert.That(GameSession.Instance.Resources, Is.Not.Null);
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");
            yield return new WaitForSeconds(2f);
            var player = Object.FindFirstObjectByType<PlayerController>();
            Assert.That(player, Is.Not.Null);
            Assert.That(player.GetComponent<SpriteRenderer>().sprite, Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<SimpleHud>(), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<CampfireAura>(), Is.Not.Null);
            Assert.That(Object.FindObjectsByType<Building>(FindObjectsSortMode.None).Length,
                Is.GreaterThan(0));
            // Confirm the selected front-view art survives runtime Start.
            var frontBuildings = new[] {
                BuildingFactory.SpawnBarricade(new Vector3(30, 0, 0)),
                BuildingFactory.SpawnHouse(new Vector3(32, 0, 0)),
                BuildingFactory.SpawnStorage(new Vector3(34, 0, 0)),
                BuildingFactory.SpawnWatchtower(new Vector3(36, 0, 0))
            };
            yield return null;
            var frontNames = new[] { "wood_barricade", "cabin", "storage", "watchtower" };
            for (int i = 0; i < frontBuildings.Length; i++)
            {
                var building = frontBuildings[i];
                var expected = Resources.Load<Sprite>("Sprites/GeneratedStatic20261014/Buildings/" + frontNames[i]);
                Assert.That(building.GetComponent<SpriteRenderer>().sprite, Is.SameAs(expected), building.name);
                Object.Destroy(building);
            }
#if UNITY_EDITOR
            // Some prefabs (notably the campfire) assemble their renderers in
            // Awake, so validate visuals in play mode after initialization.
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var instance = Object.Instantiate(prefab, new Vector3(40, 0, 0), Quaternion.identity);
                yield return null;
                yield return null;
                bool hasVisibleSprite = false;
                foreach (var renderer in instance.GetComponentsInChildren<SpriteRenderer>(true))
                    if (renderer.sprite != null) hasVisibleSprite = true;
                Assert.That(hasVisibleSprite, Is.True, path);
                Object.Destroy(instance);
            }
#endif
            var session = GameSession.Instance;
            yield return SceneManager.LoadSceneAsync("BootScene");
            yield return null;
            Assert.That(GameSession.Instance, Is.SameAs(session));
        }
    }
}
