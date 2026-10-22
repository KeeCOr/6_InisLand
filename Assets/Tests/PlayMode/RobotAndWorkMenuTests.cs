using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace IL6.Tests
{
    public class RobotAndWorkMenuTests
    {
        private const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        private IEnumerator Ready()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(2);
            var hud=Object.FindFirstObjectByType<SimpleHud>();typeof(SimpleHud).GetMethod("PrepareHudPreview",Flags).Invoke(hud,null);
            hud.Player.transform.position=new Vector3(1000,1000,0);
        }
        [UnityTest] public IEnumerator FirstWaveRobotSurvivesTwoBasicMeleeHits()
        {
            yield return Ready();var night=Object.FindFirstObjectByType<NightController>();
            GameSession.Instance.Cycle.Restore(new DayNightSnapshot { day=1,phase=Phase.Day,elapsedInPhase=0 });
            typeof(NightController).GetMethod("SpawnOne",Flags).Invoke(night,null);
            Zombie newest=null;foreach(var z in Object.FindObjectsByType<Zombie>(FindObjectsSortMode.None)) if(z.name=="Zombie_wave") newest=z;
            Assert.That(newest,Is.Not.Null);Assert.That(newest.MaxHp,Is.EqualTo(60));
            newest.TakeDamage(28);newest.TakeDamage(28);Assert.That(newest.IsDead,Is.False);
            newest.TakeDamage(28);Assert.That(newest.IsDead,Is.True);
            yield return SceneManager.LoadSceneAsync("BootScene");
        }
        [UnityTest] public IEnumerator WorkMenuBlocksOtherUiAndRestoresTimeBeforeExecutingUpgrade()
        {
            yield return Ready();var hud=Object.FindFirstObjectByType<SimpleHud>();
            var house=BuildingFactory.SpawnHouse(hud.Player.transform.position+Vector3.right).GetComponent<Building>();
            GameSession.Instance.Resources.Add(ResourceKind.Wood,100);GameSession.Instance.Resources.Add(ResourceKind.Stone,100);int before=house.Level;
            var open=typeof(SimpleHud).GetMethod("OpenNearbyWorkMenu");Assert.That(open,Is.Not.Null,"Nearby work must use a single exclusive list");
            Assert.That((bool)open.Invoke(hud,null),Is.True);Assert.That(Time.timeScale,Is.Zero);Assert.That(InputReader.HudBlocked,Is.True);
            Assert.That((bool)typeof(SimpleHud).GetMethod("IsHudPoint",Flags).Invoke(hud,new object[]{Vector2.zero}),Is.True);
            var use=typeof(SimpleHud).GetMethod("TryUseWorkMenuAction");Assert.That((bool)use.Invoke(hud,new object[]{-1}),Is.False);
            var actions=(IList)typeof(SimpleHud).GetField("_workMenuActions",Flags).GetValue(hud);int upgrade=-1;
            for(int i=0;i<actions.Count;i++) if(((string)actions[i].GetType().GetField("Label").GetValue(actions[i])).StartsWith("시설 강화")) upgrade=i;
            Assert.That(upgrade,Is.GreaterThanOrEqualTo(0));
            Assert.That((bool)use.Invoke(hud,new object[]{upgrade}),Is.True);Assert.That(Time.timeScale,Is.EqualTo(1));Assert.That(house.Level,Is.GreaterThan(before));
            Assert.That((bool)use.Invoke(hud,new object[]{upgrade}),Is.False,"A row can only execute once after closing");
            Object.Destroy(house.gameObject);yield return SceneManager.LoadSceneAsync("BootScene");
        }
        [UnityTest] public IEnumerator ResourceDetailsDoNotLeaveCompanionButtonsActiveBehindThem()
        {
            yield return Ready();var hud=Object.FindFirstObjectByType<SimpleHud>();
            typeof(SimpleHud).GetField("_resourceDetailsOpen",Flags).SetValue(hud,true);
            var hiddenEdge=new Vector2(MobileHudLayout.CompanionSummary(false).x+2,MobileHudLayout.CompanionSummary(false).y+8);
            Assert.That((bool)typeof(SimpleHud).GetMethod("IsHudPoint",Flags).Invoke(hud,new object[]{hiddenEdge}),Is.False,"Hidden companion controls must not intercept input");
            yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
