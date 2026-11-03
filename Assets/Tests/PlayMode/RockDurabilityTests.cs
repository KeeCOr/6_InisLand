using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public class RockDurabilityTests
    {
        [UnityTest] public IEnumerator CancelAndResumeKeepsRockDamageThroughGatherController()
        {
            foreach(var hud in Object.FindObjectsByType<SimpleHud>(FindObjectsSortMode.None))hud.enabled=false;
            Time.timeScale=1;InputReader.HudBlocked=false;
            var actor=new GameObject("Mining controller player");actor.transform.position=new Vector3(8000,8000);
            var player=actor.AddComponent<PlayerController>();player.enabled=false;
            var input=actor.GetComponent<InputReader>();input.enabled=false;
            var gather=actor.AddComponent<GatherController>();gather.Player=player;gather.Input=input;gather.Store=new ResourceStore();
            var rock=new GameObject("Mining controller rock");rock.transform.position=actor.transform.position+Vector3.right;
            rock.AddComponent<SpriteRenderer>().sprite=SpriteBank.SnowRock();
            var node=rock.AddComponent<Gatherable>();node.YieldKind=ResourceKind.Stone;node.DurationSec=.8f;node.DestroyOnGather=false;
            yield return null;
            Assert.That(gather.StartGathering(node),Is.True);yield return new WaitForSeconds(.3f);
            var hp=node.EnsureRockHealth();Assert.That(hp.CurrentHp,Is.InRange(150,175));
            gather.Cancel();int remaining=hp.CurrentHp;yield return new WaitForSeconds(.6f);
            Assert.That(hp.CurrentHp,Is.EqualTo(remaining));Assert.That(gather.StartGathering(node),Is.True);
            Assert.That(gather.Progress,Is.EqualTo(1f-(float)remaining/hp.MaxHp));
            yield return new WaitForSeconds(1.8f);
            Assert.That(hp.CurrentHp,Is.Zero);Assert.That(gather.IsActive,Is.False);
            Object.Destroy(rock);Object.Destroy(actor);yield return null;
        }
        [UnityTest] public IEnumerator MiningRequiresTwoHundredHpAndPaysOnlyOnce()
        {
            var go=new GameObject("Durable rock test");go.transform.position=Vector3.one*5000;
            var node=go.AddComponent<Gatherable>();node.YieldKind=ResourceKind.Stone;node.YieldAmount=2;node.DurationSec=6;node.DestroyOnGather=false;
            var ensure=typeof(Gatherable).GetMethod("EnsureRockHealth");Assert.That(ensure,Is.Not.Null);
            var hp=ensure.Invoke(node,null);var type=hp.GetType();var mine=type.GetMethod("Mine");
            int Current()=>(int)type.GetProperty("CurrentHp").GetValue(hp);
            Assert.That((int)type.GetProperty("MaxHp").GetValue(hp),Is.EqualTo(200));Assert.That(Current(),Is.EqualTo(200));
            float oreChance=BalanceConfig.Instance.RockOreDropChance;BalanceConfig.Instance.RockOreDropChance=0;
            var store=new ResourceStore();int stone=store.Get(ResourceKind.Stone);
            node.OnGathered(store);Assert.That(store.Get(ResourceKind.Stone),Is.EqualTo(stone),"Helpers cannot bypass rock HP");
            mine.Invoke(hp,new object[]{6f,store});Assert.That(Current(),Is.EqualTo(100));
            Assert.That(store.Get(ResourceKind.Stone),Is.EqualTo(stone));
            yield return null;yield return null;
            var bar=go.GetComponent<HpBarUi>();Assert.That(bar,Is.Not.Null);Assert.That(go.transform.Find("HpBar").gameObject.activeSelf,Is.True);
            // Waiting/interruption does not restore the remaining HP.
            yield return null;Assert.That(Current(),Is.EqualTo(100));
            mine.Invoke(hp,new object[]{6f,store});Assert.That(Current(),Is.Zero);
            Assert.That(store.Get(ResourceKind.Stone),Is.EqualTo(stone+2));
            mine.Invoke(hp,new object[]{60f,store});node.OnGathered(store);
            Assert.That(store.Get(ResourceKind.Stone),Is.EqualTo(stone+2));
            BalanceConfig.Instance.RockOreDropChance=oreChance;Object.Destroy(go);yield return null;
        }
    }
}
