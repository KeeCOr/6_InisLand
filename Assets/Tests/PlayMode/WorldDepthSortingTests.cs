using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace IL6.Tests
{
    public class WorldDepthSortingTests
    {
        [UnityTest] public IEnumerator PeopleAndBuildingsChangeFrontOrderWithoutOverlayingRoleIcons()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(2);
            var hud=Object.FindFirstObjectByType<SimpleHud>();
            typeof(SimpleHud).GetMethod("PrepareHudPreview",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,null);
            var player=hud.Player;player.transform.position=new Vector3(3000,3000,0);
            // This test teleports through a solid building to verify rendering order, not collision response.
            player.enabled=false;player.GetComponent<Rigidbody2D>().simulated=false;
            Camera.main.transform.position=new Vector3(3000,3000,-10);
            var house=BuildingFactory.SpawnHouse(new Vector3(3000,3002,0));
            var npc=ProceduralSpawner.CreateNpcFromArchetype(ProceduralSpawner._npcArchetypes[0],"Depth candidate",new Vector3(3000,3004,0));
            var tree=(GameObject)typeof(ProceduralSpawner).GetMethod("CreateTree",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{3003f,3006f});
            yield return null;yield return null;
            var p=player.GetComponent<SortingGroup>();var h=house.GetComponent<SortingGroup>();
            var n=npc.GetComponent<SortingGroup>();var t=tree.GetComponent<SortingGroup>();
            Assert.That(p,Is.Not.Null);Assert.That(h,Is.Not.Null);Assert.That(n,Is.Not.Null);Assert.That(t,Is.Not.Null);
            Assert.That(p.sortingOrder,Is.GreaterThan(h.sortingOrder));
            Assert.That(h.sortingOrder,Is.GreaterThan(n.sortingOrder));
            Assert.That(n.sortingOrder,Is.GreaterThan(t.sortingOrder));
            Assert.That(h.sortingOrder,Is.GreaterThan(-9500),"World geometry must remain above snow overlays and the ground far from origin");
            Assert.That(house.transform.Find("RoleMarker"),Is.Null,"HUD role icons must not appear as miniature buildings over the house");
            player.transform.position=new Vector3(3000,3003,0);
            yield return null;yield return null;
            Assert.That(p.sortingOrder,Is.LessThan(h.sortingOrder),"Moving above a building must put the player behind it");
            Assert.That(house.transform.localScale,Is.EqualTo(ConstructionPlacement.Scale(BuildingKind.House)));
            Object.Destroy(house);Object.Destroy(npc);Object.Destroy(tree);
            yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
