using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace IL6.Tests {
public class UnifiedInteractionTests {
private const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
private IEnumerator Ready() {Time.timeScale=1;InputReader.HudBlocked=false;yield return SceneManager.LoadSceneAsync("BootScene");yield return null;yield return null;yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(2);}
[UnityTest] public IEnumerator BuildSelectionEquipsHammerPausesCatalogAndRestoresPreviousWeapon() {
yield return Ready();var hud=Object.FindFirstObjectByType<SimpleHud>();typeof(SimpleHud).GetMethod("PrepareHudPreview",Flags).Invoke(hud,null);
var mode=typeof(SimpleHud).GetMethod("SetHudMode",Flags);mode.Invoke(hud,new object[]{SimpleHud.HudMode.Ranged});
Assert.That(hud.Attacker.WeaponIndex,Is.EqualTo(1));
mode.Invoke(hud,new object[]{SimpleHud.HudMode.Build});
Assert.That(hud.Attacker.WeaponIndex,Is.EqualTo(2),"The third weapon must be the construction hammer");
Assert.That(hud.Attacker.Weapon.Id,Is.EqualTo("construction_hammer"));
Assert.That(Time.timeScale,Is.Zero);Assert.That((bool)typeof(SimpleHud).GetField("_buildCatalogOpen",Flags).GetValue(hud),Is.True);
typeof(SimpleHud).GetMethod("CloseBuildCatalog",Flags).Invoke(hud,null);
Assert.That(Time.timeScale,Is.EqualTo(1));Assert.That(hud.Attacker.WeaponIndex,Is.EqualTo(1),"Closing construction restores the previous combat weapon");
hud.Attacker.SwitchToWeapon(2);Assert.That(hud.Attacker.TryAttack(),Is.False);Assert.That(hud.Attacker.ManualActionLabel,Is.EqualTo("수리"));
hud.Attacker.SwitchToWeapon(0);yield return SceneManager.LoadSceneAsync("BootScene");
}
[UnityTest] public IEnumerator CampfireLosesHpEachSecondDuringDayButNotWhilePaused() {
yield return Ready();var hud=Object.FindFirstObjectByType<SimpleHud>();typeof(SimpleHud).GetMethod("PrepareHudPreview",Flags).Invoke(hud,null);
var go=VillageStarter.SpawnCampfire(new Vector3(1000,1000,0));var building=go.GetComponent<Building>();
yield return null;int initial=building.CurrentHp;
yield return new WaitForSeconds(1.1f);
Assert.That(building.CurrentHp,Is.EqualTo(initial-1),"Daytime must also consume one campfire HP each second");
Time.timeScale=0;int paused=building.CurrentHp;yield return new WaitForSecondsRealtime(.3f);
Assert.That(building.CurrentHp,Is.EqualTo(paused),"Pause must not consume campfire HP");
Time.timeScale=1;yield return new WaitForSeconds(1.1f);Assert.That(building.CurrentHp,Is.EqualTo(initial-2));
Object.Destroy(go);yield return SceneManager.LoadSceneAsync("BootScene");
}
[UnityTest] public IEnumerator CampfireHpBarHidesAtFullHealthAndTracksRepair() {
yield return Ready();var go=VillageStarter.SpawnCampfire(new Vector3(1000,1000,0));var building=go.GetComponent<Building>();
yield return null;yield return null;
var bar=go.transform.Find("HpBar");Assert.That(bar,Is.Not.Null);Assert.That(go.GetComponents<HpBarUi>().Length,Is.EqualTo(1));
Assert.That(bar.gameObject.activeSelf,Is.False,"Full-health campfires must hide their HP bar");
Assert.That(bar.localPosition.y,Is.EqualTo(go.GetComponent<HpBarUi>().Offset.y).Within(.001f),"Runtime offset must be applied above the fire");
building.TakeDamage(10);yield return null;yield return null;
Assert.That(bar.gameObject.activeSelf,Is.True,"Damaged campfires must show their HP bar");
Assert.That(bar.Find("Fill").localScale.x,Is.EqualTo((float)building.CurrentHp/building.MaxHp).Within(.001f));
building.RepairHp(building.MaxHp);yield return null;yield return null;
Assert.That(bar.gameObject.activeSelf,Is.False,"Repairing to full health must hide the HP bar again");
Object.Destroy(go);Time.timeScale=1;yield return SceneManager.LoadSceneAsync("BootScene");
}
[UnityTest] public IEnumerator AttackButtonRepairsAndRecruitsNearbyTargets() {
yield return Ready();var hud=Object.FindFirstObjectByType<SimpleHud>();typeof(SimpleHud).GetMethod("PrepareHudPreview",Flags).Invoke(hud,null);
var player=hud.Player;player.transform.position=new Vector3(1000,1000,0);
var house=BuildingFactory.SpawnHouse(player.transform.position+Vector3.right).GetComponent<Building>();house.TakeDamage(30);
var store=GameSession.Instance.Resources;store.Add(ResourceKind.Wood,10);int wood=store.Get(ResourceKind.Wood),hp=house.CurrentHp;
typeof(SimpleHud).GetMethod("SetHudMode",Flags).Invoke(hud,new object[]{SimpleHud.HudMode.Build});
Assert.That(hud.EnterBuildRepairMode(),Is.True);
Assert.That(hud.Attacker.TryManualAction(),Is.True);Assert.That(house.CurrentHp,Is.GreaterThan(hp));Assert.That(store.Get(ResourceKind.Wood),Is.EqualTo(wood-1));
Assert.That(hud.Attacker.TryManualAction(),Is.False,"Repeated action must respect interaction cooldown");
Object.Destroy(house.gameObject);yield return new WaitForSeconds(.4f);
typeof(SimpleHud).GetMethod("CloseBuildCatalog",Flags).Invoke(hud,null);hud.Attacker.SwitchToWeapon(0);
var group=new GameObject("Unlimited recruitment crowd test");
for(int i=0;i<32;i++) {var member=new GameObject("Existing companion");member.transform.SetParent(group.transform);member.transform.position=new Vector3(1100,1100,0);member.AddComponent<Companion>().SetMaxHp(100,true);}
Assert.That(RecruitableNpc.CurrentCompanionCount(),Is.GreaterThanOrEqualTo(32));
var go=new GameObject("Attack recruit test");go.transform.position=player.transform.position+Vector3.right;var npc=go.AddComponent<RecruitableNpc>();npc.Player=player.transform;
Time.timeScale=0;Assert.That(hud.Attacker.TryManualAction(),Is.False);Assert.That(go.GetComponent<Companion>(),Is.Null);Time.timeScale=1;
Assert.That(hud.Attacker.TryManualAction(),Is.True);Assert.That(go.GetComponent<Companion>(),Is.Not.Null);
npc.Recruit();Assert.That(go.GetComponents<Companion>().Length,Is.EqualTo(1),"Repeated recruitment must not create duplicate companions");
Object.Destroy(group);Object.Destroy(go);Time.timeScale=1;InputReader.HudBlocked=false;yield return SceneManager.LoadSceneAsync("BootScene");
}
[UnityTest] public IEnumerator HiddenBuildHudDoesNotBlockPlacementOnOldHudAreas() {
yield return Ready();var hud=Object.FindFirstObjectByType<SimpleHud>();typeof(SimpleHud).GetMethod("PrepareHudPreview",Flags).Invoke(hud,null);
GameSession.Instance.Resources.Add(ResourceKind.Wood,100);GameSession.Instance.Resources.Add(ResourceKind.Stone,100);
typeof(SimpleHud).GetMethod("SetHudMode",Flags).Invoke(hud,new object[]{SimpleHud.HudMode.Build});Assert.That(hud.TrySelectBuilding(BuildingKind.House),Is.True);
var hit=typeof(SimpleHud).GetMethod("IsHudPoint",Flags);
Assert.That((bool)hit.Invoke(hud,new object[]{MobileHudLayout.Status.center}),Is.False);
Assert.That((bool)hit.Invoke(hud,new object[]{MobileHudLayout.Actions.center}),Is.False);
Assert.That((bool)hit.Invoke(hud,new object[]{MobileHudLayout.Movement.center}),Is.False);
typeof(SimpleHud).GetMethod("CloseBuildCatalog",Flags).Invoke(hud,null);Assert.That(Time.timeScale,Is.EqualTo(1));yield return SceneManager.LoadSceneAsync("BootScene");
}
}}
