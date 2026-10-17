using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace IL6.Tests {
public class MaterialLootGameplayTests {
[UnityTest] public IEnumerator HuntingAndMiningDropItemsBeforeInventoryCredit() {
Time.timeScale=1;yield return SceneManager.LoadSceneAsync("BootScene");yield return null;yield return null;
yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(2);
var hud=Object.FindFirstObjectByType<SimpleHud>();typeof(SimpleHud).GetMethod("PrepareHudPreview",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,null);
hud.Attacker.AutoAttackEnabled=false;var player=hud.Player;player.transform.position=new Vector3(1000,1000,0);
foreach(var s in Object.FindObjectsByType<ProceduralSpawner>(FindObjectsSortMode.None)) s.enabled=false;
var store=GameSession.Instance.Resources;var snapshot=store.Snapshot();var researchSnapshot=GameSession.Instance.Research.Snapshot();GameSession.Instance.Research.Restore(null);float chance=BalanceConfig.Instance.RockOreDropChance;
var rock=new GameObject("Ore rock test");var deer=new GameObject("Meat prey test");var noOre=new GameObject("Ordinary rock test");
try {
store.Spend(ResourceKind.Ore,store.Get(ResourceKind.Ore));store.Spend(ResourceKind.Meat,store.Get(ResourceKind.Meat));store.Spend(ResourceKind.Stone,store.Get(ResourceKind.Stone));
BalanceConfig.Instance.RockOreDropChance=1;
rock.transform.position=player.transform.position+Vector3.right*4;
var node=rock.AddComponent<Gatherable>();node.YieldKind=ResourceKind.Stone;node.YieldAmount=2;node.DestroyOnGather=false;
node.OnGathered(store);node.OnGathered(store);
var ore=Object.FindObjectsByType<WoodPickup>(FindObjectsSortMode.None).Where(x=>x.Kind==ResourceKind.Ore).ToArray();
Assert.That(ore.Length,Is.EqualTo(1));Assert.That(store.Get(ResourceKind.Stone),Is.EqualTo(2));Assert.That(store.Get(ResourceKind.Ore),Is.Zero);
yield return new WaitForSeconds(.5f);player.transform.position=ore[0].transform.position;
store.Add(ResourceKind.Ore,store.GetCap(ResourceKind.Ore));Assert.That(ore[0].TryCollect(player),Is.False);
store.Spend(ResourceKind.Ore,store.Get(ResourceKind.Ore));Assert.That(ore[0].TryCollect(player),Is.True);Assert.That(store.Get(ResourceKind.Ore),Is.Zero);
yield return new WaitForSeconds(.8f);Assert.That(store.Get(ResourceKind.Ore),Is.EqualTo(1));Assert.That(WoodPickup.LastPickupKind,Is.EqualTo(ResourceKind.Ore));
BalanceConfig.Instance.RockOreDropChance=0;
var ordinary=noOre.AddComponent<Gatherable>();ordinary.YieldKind=ResourceKind.Stone;ordinary.DestroyOnGather=false;ordinary.OnGathered(store);
Assert.That(Object.FindObjectsByType<WoodPickup>(FindObjectsSortMode.None).Count(x=>x.Kind==ResourceKind.Ore),Is.Zero);
deer.transform.position=player.transform.position+Vector3.right*4;
var meat=deer.AddComponent<Gatherable>();meat.YieldKind=ResourceKind.Meat;meat.YieldAmount=0;
var animal=deer.AddComponent<DeerAi>();animal.InitHp(2);animal.enabled=false;
Assert.That(hud.Gather.StartGathering(meat),Is.False,"Living animals must be hunted, not harvested");
meat.OnGathered(store);Assert.That(Object.FindObjectsByType<WoodPickup>(FindObjectsSortMode.None).Count(x=>x.Kind==ResourceKind.Meat),Is.Zero);
animal.TakeDamage(1);Assert.That(store.Get(ResourceKind.Meat),Is.Zero);
animal.TakeDamage(1);animal.TakeDamage(1);
var dropped=Object.FindObjectsByType<WoodPickup>(FindObjectsSortMode.None).Where(x=>x.Kind==ResourceKind.Meat).ToArray();
Assert.That(dropped.Length,Is.EqualTo(1));Assert.That(dropped[0].Amount,Is.EqualTo(1));Assert.That(store.Get(ResourceKind.Meat),Is.Zero);
yield return new WaitForSeconds(.5f);player.transform.position=dropped[0].transform.position;Assert.That(dropped[0].TryCollect(player),Is.True);
yield return new WaitForSeconds(.8f);Assert.That(store.Get(ResourceKind.Meat),Is.EqualTo(1));
} finally {
BalanceConfig.Instance.RockOreDropChance=chance;store.Restore(snapshot);GameSession.Instance.Research.Restore(researchSnapshot);
if(rock!=null)Object.Destroy(rock);if(deer!=null)Object.Destroy(deer);if(noOre!=null)Object.Destroy(noOre);
foreach(var item in Object.FindObjectsByType<WoodPickup>(FindObjectsSortMode.None)) Object.Destroy(item.gameObject);
Time.timeScale=1;InputReader.HudBlocked=false;
}
yield return null;
}
}}
