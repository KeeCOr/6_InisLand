using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using IL6.Events;

namespace IL6.Tests
{
    public class FencePersistenceTests
    {
        [UnityTest] public IEnumerator NighttimeConstructionKeepsBrokenAndDamagedFenceState()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(2);
            var hud=Object.FindFirstObjectByType<SimpleHud>();
            typeof(SimpleHud).GetMethod("PrepareHudPreview",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,null);
            var root=GameObject.Find("VillageOuterFence");Assert.That(root,Is.Not.Null);
            Building damaged=null,missing=null;
            foreach(var b in root.GetComponentsInChildren<Building>())
            {
                if(b.GetComponent<Door>()!=null) continue;
                if(damaged==null) damaged=b;else {missing=b;break;}
            }
            var gate=Object.FindFirstObjectByType<Door>();float halfSize=VillageStarter.CurrentHalfSize;
            damaged.TakeDamage(7);int hp=damaged.CurrentHp;Vector3 gap=missing.transform.position;
            missing.TakeDamage(missing.MaxHp);yield return null;
            var night=Object.FindFirstObjectByType<NightController>();night.enabled=false;
            GameSession.Instance.Cycle.Restore(new DayNightSnapshot {day=1,phase=Phase.Night,elapsedInPhase=0});
            EventBus.Instance.Emit(new NightStartedPayload(1));
            var center=new Vector3(GameConstants.VillageCenterX,GameConstants.VillageCenterY,0);
            typeof(SimpleHud).GetMethod("SpawnConstructionSite",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,new object[]{BuildingKind.House,center+Vector3.right*3});
            var site=Object.FindFirstObjectByType<ConstructionSite>();site.TotalTime=.05f;
            yield return new WaitForSeconds(.25f);
            Assert.That(GameObject.Find("VillageOuterFence"),Is.SameAs(root),"Completing a building must not replace the perimeter");
            Assert.That(damaged!=null,Is.True);Assert.That(damaged.CurrentHp,Is.EqualTo(hp));
            Assert.That(Object.FindFirstObjectByType<Door>(),Is.SameAs(gate));Assert.That(VillageStarter.CurrentHalfSize,Is.EqualTo(halfSize));
            foreach(var b in Object.FindObjectsByType<Building>(FindObjectsSortMode.None))
                if(b.Kind==BuildingKind.Fence) Assert.That(Vector2.Distance(b.transform.position,gap),Is.GreaterThan(.1f),"Destroyed pieces must remain missing");
            Time.timeScale=1;yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
