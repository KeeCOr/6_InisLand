using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using IL6.Events;

namespace IL6.Tests
{
    public class MorningFenceRepairTests
    {
        private IEnumerator Ready()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");
            GameSession.Instance.Cycle.Restore(new DayNightSnapshot{day=1,phase=Phase.Day,elapsedInPhase=0});
            EventBus.Instance.Emit(new DayStartedPayload(1));
            yield return new WaitForSeconds(1);
            var hud=Object.FindFirstObjectByType<SimpleHud>();typeof(SimpleHud).GetMethod("PrepareHudPreview",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(hud,null);
        }
        private static bool Open(SimpleHud hud)=>(bool)typeof(SimpleHud).GetProperty("MorningFenceRepairOpen").GetValue(hud);
        private static bool Confirm(SimpleHud hud)=>(bool)typeof(SimpleHud).GetMethod("ConfirmMorningFenceRepair").Invoke(hud,null);
        private static void Cancel(SimpleHud hud)=>typeof(SimpleHud).GetMethod("DeclineMorningFenceRepair").Invoke(hud,null);
        private static void Morning()=>EventBus.Instance.Emit(new DayStartedPayload(2));

        [UnityTest] public IEnumerator MorningPromptWaitsForExistingMenuAndSkipsUndamagedFences()
        {
            yield return Ready();var hud=Object.FindFirstObjectByType<SimpleHud>();
            Assert.That(hud.OpenResearchCatalog(),Is.True);Building fence=null;
            foreach(var b in GameObject.Find("VillageOuterFence").GetComponentsInChildren<Building>())if(b.GetComponent<Door>()==null){fence=b;break;}
            fence.TakeDamage(10);Morning();yield return null;
            Assert.That(Open(hud),Is.False,"Existing research menu keeps modal ownership");
            hud.CloseResearchCatalog();yield return null;yield return null;
            Assert.That(Open(hud),Is.True);Cancel(hud);
            foreach(var b in Object.FindObjectsByType<Building>(FindObjectsSortMode.None))if(b.Kind==BuildingKind.Fence)b.RepairHp(b.MaxHp);
            EventBus.Instance.Emit(new DayStartedPayload(3));yield return null;yield return null;
            Assert.That(Open(hud),Is.False,"Healthy fences do not need a daily popup");
            Assert.That(Time.timeScale,Is.EqualTo(1));yield return SceneManager.LoadSceneAsync("BootScene");
        }

        [UnityTest] public IEnumerator DawnPreservesDamageAndPaidMorningRepairRestoresGateAndVerticalSectionOnce()
        {
            yield return Ready();var hud=Object.FindFirstObjectByType<SimpleHud>();Building damaged=null,vertical=null,gate=null;
            foreach(var b in GameObject.Find("VillageOuterFence").GetComponentsInChildren<Building>())
            {
                if(b.GetComponent<Door>()!=null){gate=b;continue;}
                string name=b.GetComponent<SpriteRenderer>().sprite.name;
                if(name.Contains("vertical")){if(vertical==null)vertical=b;}
                else if(damaged==null)damaged=b;
            }
            var gatePos=gate.transform.position;var verticalPos=vertical.transform.position;
            string verticalSprite=vertical.GetComponent<SpriteRenderer>().sprite.name;
            damaged.TakeDamage(30);int damagedHp=damaged.CurrentHp;
            gate.TakeDamage(gate.MaxHp);vertical.TakeDamage(vertical.MaxHp);yield return null;
            EventBus.Instance.Emit(new DawnStartedPayload(1));yield return null;
            Assert.That(damaged.CurrentHp,Is.EqualTo(damagedHp),"Dawn must not repair fences for free");
            Morning();yield return null;yield return null;
            Assert.That(Open(hud),Is.True);Assert.That(Time.timeScale,Is.Zero);Assert.That(InputReader.HudBlocked,Is.True);
            Assert.That((int)typeof(SimpleHud).GetProperty("MorningFenceRepairCost").GetValue(hud),Is.EqualTo(3));
            // Other gameplay tests may leave the persistent store empty. Arrange the
            // successful-payment case explicitly; the insufficient-funds test is separate.
            var store=GameSession.Instance.Resources;store.Add(ResourceKind.Wood,20);int wood=store.Get(ResourceKind.Wood);
            Assert.That(Confirm(hud),Is.True);Assert.That(store.Get(ResourceKind.Wood),Is.EqualTo(wood-3));
            Assert.That(damaged.CurrentHp,Is.EqualTo(damaged.MaxHp));Assert.That(Open(hud),Is.False);Assert.That(Time.timeScale,Is.EqualTo(1));
            Assert.That(Confirm(hud),Is.False);Assert.That(store.Get(ResourceKind.Wood),Is.EqualTo(wood-3));
            yield return null;yield return null;
            bool restoredGate=false,restoredVertical=false;
            foreach(var b in Object.FindObjectsByType<Building>(FindObjectsSortMode.None))
            {
                if(b.Kind!=BuildingKind.Fence)continue;
                if(Vector2.Distance(b.transform.position,gatePos)<.1f)restoredGate=b.GetComponent<Door>()!=null && b.CurrentHp==b.MaxHp;
                if(Vector2.Distance(b.transform.position,verticalPos)<.1f)restoredVertical=b.GetComponent<SpriteRenderer>().sprite.name==verticalSprite && b.CurrentHp==b.MaxHp;
            }
            Assert.That(restoredGate,Is.True);Assert.That(restoredVertical,Is.True);
            foreach(var debris in Object.FindObjectsByType<FenceWreckage>(FindObjectsSortMode.None))
                Assert.That(Vector2.Distance(debris.transform.position,gatePos),Is.GreaterThan(.1f));
            yield return SceneManager.LoadSceneAsync("BootScene");
        }

        [UnityTest] public IEnumerator InsufficientWoodAndDeclineKeepDamageAndDoNotRepeatSameMorning()
        {
            yield return Ready();var hud=Object.FindFirstObjectByType<SimpleHud>();Building fence=null;
            foreach(var b in GameObject.Find("VillageOuterFence").GetComponentsInChildren<Building>())if(b.GetComponent<Door>()==null){fence=b;break;}
            fence.TakeDamage(20);int hp=fence.CurrentHp;Morning();yield return null;yield return null;
            Assert.That(Open(hud),Is.True);var store=GameSession.Instance.Resources;store.Spend(ResourceKind.Wood,store.Get(ResourceKind.Wood));
            Assert.That(Confirm(hud),Is.False);Assert.That(fence.CurrentHp,Is.EqualTo(hp));Assert.That(store.Get(ResourceKind.Wood),Is.Zero);
            Assert.That(Open(hud),Is.True);Cancel(hud);Assert.That(Open(hud),Is.False);Assert.That(Time.timeScale,Is.EqualTo(1));
            Morning();yield return null;Assert.That(Open(hud),Is.False);
            EventBus.Instance.Emit(new DayStartedPayload(3));yield return null;yield return null;
            Assert.That(Open(hud),Is.True);Cancel(hud);yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
