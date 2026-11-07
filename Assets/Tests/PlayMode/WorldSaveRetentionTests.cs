using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace IL6.Tests {
public class WorldSaveRetentionTests {
 [Serializable] public class B {public string kind;public float x,y;public int level,hp;}
 [Serializable] public class State {
  public int day;public string phase;public float elapsed;public int[] resources,research;
  public bool storyComplete,storyRuined;public int storyStage,kills,playerLevel,playerXp,rangedStacks;
  public B[] buildings;public string[] companions,companionIds;public int[] companionXp,companionHp;public SavedNight night;public int wreckageCount;
 }
 static string Root=>Environment.GetEnvironmentVariable("IL6_TEST_SAVE_DIRECTORY");
 static State Capture(){var s=GameSession.Instance;var p=GameObject.FindWithTag("Player").GetComponent<PlayerProgression>();
  var c=Object.FindObjectsByType<Companion>(FindObjectsSortMode.None).OrderBy(x=>x.name).ToArray();
  foreach(var companion in c)companion.EnsureSaveId();
  return new State {day=s.Cycle.Day,phase=s.Cycle.Phase.ToString(),elapsed=s.Cycle.ElapsedInPhase,
   resources=((ResourceKind[])Enum.GetValues(typeof(ResourceKind))).Select(k=>s.Resources.Get(k)).ToArray(),research=s.Research.Snapshot(),
   storyComplete=s.StoryTutorialComplete,storyRuined=s.StoryVillageRuined,storyStage=s.StoryTutorialStage,kills=s.TotalKills,
   playerLevel=p.Level,playerXp=p.Xp,rangedStacks=p.GetStacks(RuneKind.RangedDamageUp),
   buildings=Object.FindObjectsByType<Building>(FindObjectsSortMode.None).Where(b=>b.CurrentHp>0).Select(b=>new B{kind=b.Kind.ToString(),x=b.transform.position.x,y=b.transform.position.y,level=b.Level,hp=b.CurrentHp}).OrderBy(b=>b.kind).ThenBy(b=>b.x).ThenBy(b=>b.y).ToArray(),
   companions=c.Select(x=>x.name).ToArray(),companionIds=c.Select(x=>x.SaveId).ToArray(),companionHp=c.Select(x=>x.CurrentHp).ToArray(),companionXp=c.Select(x=>x.Xp).ToArray(),
   night=Object.FindFirstObjectByType<NightController>().CaptureSave(),wreckageCount=Object.FindObjectsByType<FenceWreckage>(FindObjectsSortMode.None).Length};}
 static void Write(string name,State s)=>File.WriteAllText(Path.Combine(Root,name+".json"),JsonUtility.ToJson(s,true));
 static void FreezeLoadedScene(Scene scene,LoadSceneMode mode){if(scene.name=="SnowfieldScene"){Time.timeScale=0;SceneManager.sceneLoaded-=FreezeLoadedScene;}}
 static IEnumerator Continue(){
  SceneManager.sceneLoaded-=FreezeLoadedScene;SceneManager.sceneLoaded+=FreezeLoadedScene;
  BootController.SkipLobbyOnce=false;SaveLoad.SelectSlot(3);Time.timeScale=0;InputReader.HudBlocked=false;
  yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
  var lobby=Object.FindFirstObjectByType<LobbyController>();Assert.That(lobby,Is.Not.Null);Assert.That(lobby.TryContinue(3),Is.True);
  float end=Time.realtimeSinceStartup+12;
  while(Time.realtimeSinceStartup<end && (SceneManager.GetActiveScene().name!="SnowfieldScene" || Object.FindFirstObjectByType<SimpleHud>()==null))yield return null;
  Assert.That(SceneManager.GetActiveScene().name,Is.EqualTo("SnowfieldScene"));yield return null;Time.timeScale=0;
 }
 static void Controls(State expected,State actual){
  Assert.That(actual.day,Is.EqualTo(expected.day));CollectionAssert.AreEqual(expected.resources,actual.resources);
  CollectionAssert.AreEqual(expected.research,actual.research);Assert.That(actual.storyComplete,Is.EqualTo(expected.storyComplete));
  Assert.That(actual.storyRuined,Is.EqualTo(expected.storyRuined));Assert.That(actual.storyStage,Is.EqualTo(expected.storyStage));
  {

   Assert.That(actual.buildings.Length,Is.EqualTo(expected.buildings.Length),"No default village duplication");
   foreach(var b in expected.buildings){var found=actual.buildings.FirstOrDefault(x=>x.kind==b.kind && x.x==b.x && x.y==b.y);Assert.That(found,Is.Not.Null,"Missing "+b.kind+" at "+b.x+","+b.y);if(found!=null){Assert.That(found.level,Is.EqualTo(b.level));Assert.That(found.hp,Is.EqualTo(b.hp));}}
   CollectionAssert.AreEqual(expected.companions,actual.companions,"Recruits survive reload");CollectionAssert.AreEqual(expected.companionXp,actual.companionXp);
   CollectionAssert.AreEqual(expected.companionIds,actual.companionIds);CollectionAssert.AreEqual(expected.companionHp,actual.companionHp);
   Assert.That(actual.wreckageCount,Is.EqualTo(expected.wreckageCount));
   Assert.That(actual.night.spawnTimer,Is.EqualTo(expected.night.spawnTimer).Within(.05f));Assert.That(actual.night.warningRemaining,Is.EqualTo(expected.night.warningRemaining).Within(.05f));
   actual.night.spawnTimer=expected.night.spawnTimer;actual.night.warningRemaining=expected.night.warningRemaining;
   Assert.That(JsonUtility.ToJson(actual.night),Is.EqualTo(JsonUtility.ToJson(expected.night)),"Night enemies, pending wave and boss warning remain identical");
   Assert.That(actual.playerLevel,Is.EqualTo(expected.playerLevel));Assert.That(actual.playerXp,Is.EqualTo(expected.playerXp));Assert.That(actual.rangedStacks,Is.EqualTo(expected.rangedStacks));Assert.That(actual.kills,Is.EqualTo(expected.kills));
   Assert.That(actual.phase,Is.EqualTo(expected.phase));Assert.That(actual.elapsed,Is.EqualTo(expected.elapsed).Within(.05f));
  }
 }
 [UnityTest] public IEnumerator PrepareAndLobbyReload(){
  if(string.IsNullOrEmpty(Root))Assert.Ignore("Requires isolated save directory");SaveLoad.SelectSlot(3);
  SaveLoad.SelectSlot(1);Assert.That(SaveLoad.Save(new SaveFileV1{currentDay=11,resources=new ResourceSnapshot{wood=17}}),Is.True);
  File.Copy(SaveLoad.StoragePath,Path.Combine(Root,"untouched-slot-one.json"),true);SaveLoad.SelectSlot(3);
  Assert.That(SaveLoad.Save(new SaveFileV1{currentDay=4,storyTutorialEnabled=false,storyTutorialComplete=true,storyTutorialStage=99,storyVillageRuined=true}),Is.True);
  yield return Continue();var s=GameSession.Instance;s.Resources.IncreaseCap(5000);s.Resources.Add(ResourceKind.Wood,2000);s.Resources.Add(ResourceKind.Stone,2000);
  s.Research.Restore(new[]{2,1,0,0,0,0,0,0,0,0});
  var house=BuildingFactory.SpawnHouse(new Vector3(24,12,0)).GetComponent<Building>();Assert.That(house.TryUpgrade(s.Resources),Is.True);house.TakeDamage(13);
  var fences=Object.FindObjectsByType<Building>(FindObjectsSortMode.None).Where(b=>b.Kind==BuildingKind.Fence).OrderBy(b=>b.transform.position.x).ThenBy(b=>b.transform.position.y).ToArray();Assert.That(fences.Length,Is.GreaterThan(1));
  fences[0].TakeDamage(17);fences[1].TakeDamage(fences[1].MaxHp+100);
  var npc=new GameObject("AuditCandidate").AddComponent<RecruitableNpc>();npc.DisplayName="AuditRecruit";npc.Role="Hunter";npc.Player=GameObject.FindWithTag("Player").transform;npc.transform.position=new Vector3(20,20,0);npc.Recruit();
  yield return null;var comp=GameObject.Find("AuditRecruit(Recruited)").GetComponent<Companion>();comp.GrantXp(5);
  var p=GameObject.FindWithTag("Player").GetComponent<PlayerProgression>();p.GrantXp(12);p.ApplyRune(RuneKind.RangedDamageUp);p.ApplyRune(RuneKind.ResourceGift);
  for(int i=0;i<3;i++)s.OnZombieKilled();
  s.Cycle.Restore(new DayNightSnapshot{day=4,phase=Phase.Night,elapsedInPhase=7});
  var night=Object.FindFirstObjectByType<NightController>();night.StartNight(4);night.SpawnDebugZombie();
  Object.FindFirstObjectByType<Zombie>().TakeDamage(3);night.BossKillThreshold=1;night.OnNightKill();
  var before=Capture();Write("before",before);
  var candidate=WorldPersistence.Capture(s,GameObject.FindWithTag("Player").GetComponent<PlayerController>());
  File.WriteAllText(Path.Combine(Root,"candidate-world.json"),JsonUtility.ToJson(candidate,true));Assert.That(WorldPersistence.IsValid(candidate),Is.True,"Captured world is valid");
  s.SaveNow();Assert.That(SaveLoad.LoadSlot(3).hasWorld,Is.True,"SaveNow writes a world snapshot");File.Copy(SaveLoad.StoragePath,Path.Combine(Root,"written-save.json"),true);
  s.ReturnToLobby();yield return new WaitForSecondsRealtime(.5f);yield return Continue();var after=Capture();Write("after-lobby",after);Controls(before,after);
  Debug.Log("SAVE_AUDIT lobby capture complete; all retention assertions passed.");
  CollectionAssert.AreEqual(File.ReadAllBytes(Path.Combine(Root,"untouched-slot-one.json")),File.ReadAllBytes(SaveLoad.SlotPath(1)));
  Time.timeScale=1;InputReader.HudBlocked=false;
 }
 [UnityTest] public IEnumerator FreshProcessReload(){
  if(string.IsNullOrEmpty(Root) || !File.Exists(Path.Combine(Root,"before.json")))Assert.Ignore("Requires prepare run in isolated directory");yield return Continue();
  var before=JsonUtility.FromJson<State>(File.ReadAllText(Path.Combine(Root,"before.json")));var after=Capture();Write("after-restart",after);Controls(before,after);
  Debug.Log("SAVE_AUDIT fresh process capture complete; all retention assertions passed.");Time.timeScale=1;InputReader.HudBlocked=false;
  Assert.That(GameSession.Instance.SaveNow(),Is.True);GameSession.Instance.ReturnToLobby();yield return new WaitForSecondsRealtime(.5f);yield return Continue();
  Controls(before,Capture());CollectionAssert.AreEqual(File.ReadAllBytes(Path.Combine(Root,"untouched-slot-one.json")),File.ReadAllBytes(SaveLoad.SlotPath(1)));
  Time.timeScale=1;InputReader.HudBlocked=false;
 }
 [UnityTest] public IEnumerator BossAlreadySpawnedDoesNotDuplicateAfterLoad(){
  if(string.IsNullOrEmpty(Root))Assert.Ignore("Requires isolated save directory");SaveLoad.SelectSlot(3);
  Assert.That(SaveLoad.Save(new SaveFileV1{currentDay=5,storyTutorialEnabled=false,storyTutorialComplete=true,storyTutorialStage=99}),Is.True);
  yield return Continue();var session=GameSession.Instance;session.Cycle.Restore(new DayNightSnapshot{day=5,phase=Phase.Night,elapsedInPhase=9});
  var night=Object.FindFirstObjectByType<NightController>();night.StartNight(5);var warning=night.CaptureSave();warning.warningRemaining=0;night.RestoreSave(warning,Phase.Night);
  Assert.That(night.BossEncounterActive,Is.True);Assert.That(night.CaptureSave().enemies.Count(e=>e.boss),Is.EqualTo(1));
  session.ReturnToLobby();yield return new WaitForSecondsRealtime(.5f);yield return Continue();
  night=Object.FindFirstObjectByType<NightController>();Assert.That(night.BossEncounterActive,Is.True);Assert.That(night.CaptureSave().enemies.Count(e=>e.boss),Is.EqualTo(1));Assert.That(night.CaptureSave().warningPending,Is.False);
  Time.timeScale=1;InputReader.HudBlocked=false;
 }
}}
