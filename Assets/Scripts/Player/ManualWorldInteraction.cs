using UnityEngine;
namespace IL6 {
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
public static string Hint(Vector3 position) => NearbyNpc(position)!=null?"영입":"공격";
public static bool TryRepair(PlayerController player,ResourceStore store) {
if(!CanRepair(player) || player.IsDead || InputReader.HudBlocked || Time.timeScale<=0 || (player.Actions!=null && player.Actions.IsBusy)) return false;
var building=NearbyRepair(player.transform.position);if(building==null || store==null || !store.Spend(ResourceKind.Wood,1)) return false;
building.RepairHp(Mathf.Max(1,building.MaxHp/5));Sfx.Build();return true;
}
public static bool TryUse(PlayerController player,ResourceStore store,out bool handled) {
handled=false;if(player==null || player.IsDead || InputReader.HudBlocked || Time.timeScale<=0) return false;
var npc=NearbyNpc(player.transform.position);
if(npc!=null) {handled=true;if(!npc.CanRecruit()) return false;npc.Player=player.transform;npc.Recruit();return true;}
if(!CanRepair(player) || !HasRepairTarget(player.transform.position)) return false;
handled=true;return TryRepair(player,store);
}
}}
