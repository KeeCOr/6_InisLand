using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace IL6.Tests
{
    public class HammerRepairTests
    {
        [UnityTest] public IEnumerator FenceAndCampfireRepairRequireTheHammerAndRepairMode()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(2);
            var hud=Object.FindFirstObjectByType<SimpleHud>();
            typeof(SimpleHud).GetMethod("PrepareHudPreview",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,null);
            foreach(var spawner in Object.FindObjectsByType<ProceduralSpawner>(FindObjectsSortMode.None)) spawner.enabled=false;
            hud.Player.transform.position=new Vector3(1000,1000,0);
            var store=GameSession.Instance.Resources;store.Add(ResourceKind.Wood,20);
            foreach(var kind in new[]{BuildingKind.Fence,BuildingKind.Campfire})
            {
                var go=new GameObject("Hammer repair "+kind);go.transform.position=hud.Player.transform.position+Vector3.right;
                var building=go.AddComponent<Building>();building.Initialize(kind);building.TakeDamage(20);
                for(int weapon=0;weapon<2;weapon++)
                {
                    typeof(SimpleHud).GetMethod("SetHudMode",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,new object[]{weapon==0?SimpleHud.HudMode.Melee:SimpleHud.HudMode.Ranged});
                    int hp=building.CurrentHp,wood=store.Get(ResourceKind.Wood);
                    hud.UseUnifiedAction();
                    Assert.That(building.CurrentHp,Is.EqualTo(hp),kind+" must not repair with a combat weapon");
                    Assert.That(store.Get(ResourceKind.Wood),Is.EqualTo(wood));
                }
                hud.Gather.Cancel();hud.Player.Actions.ResetToIdle();
                typeof(SimpleHud).GetMethod("SetHudMode",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,new object[]{SimpleHud.HudMode.Build});
                var repair=typeof(SimpleHud).GetMethod("EnterBuildRepairMode");Assert.That(repair,Is.Not.Null);
                Assert.That((bool)repair.Invoke(hud,null),Is.True);Assert.That(Time.timeScale,Is.EqualTo(1));
                Assert.That(hud.Attacker.IsConstructionHammer,Is.True);
                int before=building.CurrentHp,resources=store.Get(ResourceKind.Wood);hud.UseUnifiedAction();
                Assert.That(building.CurrentHp,Is.GreaterThan(before));Assert.That(store.Get(ResourceKind.Wood),Is.EqualTo(resources-1));
                before=building.CurrentHp;hud.UseUnifiedAction();Assert.That(building.CurrentHp,Is.EqualTo(before),"Repair cooldown still applies");
                Object.Destroy(go);yield return new WaitForSeconds(.4f);
            }
            typeof(SimpleHud).GetMethod("SetHudMode",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,new object[]{SimpleHud.HudMode.Melee});
            yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
