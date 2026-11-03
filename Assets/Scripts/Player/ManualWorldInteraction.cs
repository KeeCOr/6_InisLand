using UnityEngine;
namespace IL6 {
public enum ManualInteractionKind { None, Dialogue, Repair, Gather, Attack, Work }
public readonly struct ManualInteractionTarget {
public readonly ManualInteractionKind Kind;
public readonly MonoBehaviour Target;
public readonly string Label;
public readonly System.Action Callback;
public ManualInteractionTarget(ManualInteractionKind kind,MonoBehaviour target,string label,System.Action callback=null) {Kind=kind;Target=target;Label=label;Callback=callback;}
}
public static class ManualWorldInteraction {
public const float RepairRange=2.2f;
private static RecruitableNpc NearbyNpc(Vector3 position) {
RecruitableNpc best=null;float distance=float.MaxValue;
foreach(var npc in Object.FindObjectsByType<RecruitableNpc>(FindObjectsSortMode.None)) {
if(!npc.isActiveAndEnabled) continue;float d=Vector2.Distance(position,npc.transform.position);
if(d<=npc.RecruitRange && d<distance) {best=npc;distance=d;}
}return best;
}
private static Building NearbyRepair(Vector3 position) {
Building best=null;float distance=RepairRange;
foreach(var b in Object.FindObjectsByType<Building>(FindObjectsSortMode.None)) {
if(!b.isActiveAndEnabled || b.CurrentHp<=0 || b.CurrentHp>=b.MaxHp) continue;
float d=Vector2.Distance(position,b.transform.position);if(d<=distance) {best=b;distance=d;}
}return best;
}
public static bool CanRepair(PlayerController player) => player!=null && player.GetComponent<PlayerAttackController>() is PlayerAttackController attacker && attacker.IsConstructionHammer;
public static bool HasRepairTarget(Vector3 position) => NearbyRepair(position)!=null;
public static string Hint(Vector3 position) => NearbyNpc(position)!=null?"대화":"공격";
public static string Hint(PlayerController player) => Resolve(player).Label ?? "공격";
// Both the action button and the world preview consume this one priority decision.
public static ManualInteractionTarget Resolve(PlayerController player) {
if(player==null || player.IsDead || InputReader.HudBlocked || Time.timeScale<=0f) return default;
var gather=player.GetComponent<GatherController>();
if(gather!=null && gather.IsActive) return GatherTarget(gather.ActiveTarget);
if(player.Actions!=null && player.Actions.IsBusy) return default;
var npc=NearbyNpc(player.transform.position);
if(npc!=null) return new ManualInteractionTarget(ManualInteractionKind.Dialogue,npc,"대화");
var attack=player.GetComponent<PlayerAttackController>();
if(attack==null || attack.Weapon==null) return default;
if(attack.IsConstructionHammer) return new ManualInteractionTarget(ManualInteractionKind.Repair,NearbyRepair(player.transform.position),"수리");
var combat=attack.FindManualCombatTarget();
if(combat!=null) return new ManualInteractionTarget(ManualInteractionKind.Attack,combat,"공격");
var resource=gather!=null?gather.FindNearbyTarget(attack.ManualTreeInteractRadius):null;
if(resource!=null) return GatherTarget(resource);
var hud=Object.FindFirstObjectByType<SimpleHud>();
if(hud!=null && hud.Player==player && hud.TryGetContextAction(out var callback,out var label))
return new ManualInteractionTarget(ManualInteractionKind.Work,null,label,callback);
return new ManualInteractionTarget(ManualInteractionKind.Attack,null,"공격");
}
private static ManualInteractionTarget GatherTarget(Gatherable target) => new ManualInteractionTarget(ManualInteractionKind.Gather,target,
target.GetComponent<DiscoverySite>()!=null?"조사":target.IsTree?"벌목":target.YieldKind==ResourceKind.Stone?"채굴":"채집");
public static bool TryRepair(PlayerController player,ResourceStore store) {
if(!CanRepair(player) || player.IsDead || InputReader.HudBlocked || Time.timeScale<=0 || (player.Actions!=null && player.Actions.IsBusy)) return false;
return Repair(player,Resolve(player),store);
}
private static bool Repair(PlayerController player,ManualInteractionTarget resolved,ResourceStore store) {
var building=resolved.Target as Building;if(resolved.Kind!=ManualInteractionKind.Repair || building==null || store==null || !store.Spend(ResourceKind.Wood,1)) return false;
building.RepairHp(Mathf.Max(1,building.MaxHp/5));Sfx.Build();player.GetComponent<CharacterActionVisual>()?.PlayAttack(building.transform.position-player.transform.position);return true;
}
public static bool TryUse(PlayerController player,ResourceStore store,out bool handled) {
return TryUse(player,store,Resolve(player),out handled);
}
public static bool TryUse(PlayerController player,ResourceStore store,ManualInteractionTarget resolved,out bool handled) {
handled=resolved.Kind==ManualInteractionKind.Dialogue || resolved.Kind==ManualInteractionKind.Repair;
if(player==null || player.IsDead || InputReader.HudBlocked || Time.timeScale<=0 || (player.Actions!=null && player.Actions.IsBusy)) return false;
if(resolved.Kind==ManualInteractionKind.Dialogue) {
var npc=resolved.Target as RecruitableNpc;if(npc==null || !npc.CanRecruit()) return false;
var hud=Object.FindFirstObjectByType<SimpleHud>();return hud!=null && hud.Player==player && hud.BeginRecruitConversation(npc);
}
return CanRepair(player) && Repair(player,resolved,store);
}
}}
