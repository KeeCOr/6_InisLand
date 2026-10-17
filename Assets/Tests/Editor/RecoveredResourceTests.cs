using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace IL6.Tests
{
    public class RecoveredResourceTests
    {
        [TestCase("Animals/deer")]
        [TestCase("Animals/wolf")]
        [TestCase("Enemies/zombie")]
        [TestCase("Enemies/boss_frost_zombie")]
        [TestCase("Companions/uncle")]
        [TestCase("Companions/aunt")]
        [TestCase("Companions/child")]
        [TestCase("Effects/41_warm_glow")]
        public void RuntimeSprite_LoadsThroughSpriteBank(string path)
        {
            Assert.That(SpriteBank.Load(path), Is.Not.Null, path);
        }

        [Test]
        public void PlayerAndCampfire_SubSpritesLoad()
        {
            Assert.That(SpriteBank.PlayerIdle(), Is.Not.Null);
            Assert.That(SpriteBank.Campfire(), Is.Not.Null);
        }

        [TestCase(FencePieceType.Left, "wooden_fence_Left")]
        [TestCase(FencePieceType.Center, "wooden_fence_Center")]
        [TestCase(FencePieceType.Right, "wooden_fence_Right")]
        public void FenceFactory_UsesRequestedPiece(FencePieceType piece, string expectedName)
        {
            GameObject fence = null;
            try
            {
                fence = VillageStarter.SpawnFence(Vector3.zero, 0f, piece);
                var sprite = fence.GetComponent<SpriteRenderer>().sprite;
                Assert.That(sprite, Is.Not.Null);
                Assert.That(sprite.name, Is.EqualTo(expectedName));
            }
            finally
            {
                if (fence != null) Object.DestroyImmediate(fence);
            }
        }

        [Test]
        public void GeneratedPrefabs_HaveNoMissingScripts()
        {
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });
            Assert.That(guids.Length, Is.EqualTo(23));
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Assert.That(prefab.GetComponentsInChildren<MonoBehaviour>(true).Length,
                    Is.GreaterThan(0), path);
                foreach (var script in prefab.GetComponentsInChildren<MonoBehaviour>(true))
                    Assert.That(script, Is.Not.Null, path);
            }
        }

        [Test]
        public void YSort_SetOffsetBeforeAwake_UpdatesRenderer()
        {
            var go = new GameObject("EditorGeneratedObject");
            try
            {
                var renderer = go.AddComponent<SpriteRenderer>();
                var sort = go.AddComponent<YSort>();
                sort.SetOrderOffset(5);
                Assert.That(renderer.sortingOrder, Is.EqualTo(5));
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void ObjectiveAdvisor_HeatFailureAndNightOutrankFoodShortage()
        {
            Assert.That(DailyObjectiveAdvisor.Build(0, 2, false, true).Goal,
                Is.EqualTo("Restore village heat"));
            Assert.That(DailyObjectiveAdvisor.Build(0, 2, true, true).Goal,
                Is.EqualTo("Survive the night"));
        }
    }
}
