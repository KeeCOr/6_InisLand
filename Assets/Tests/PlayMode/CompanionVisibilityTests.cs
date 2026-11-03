using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public class CompanionVisibilityTests
    {
        private const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        private IEnumerator Ready()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(2);
            foreach(var spawner in UnityEngine.Object.FindObjectsByType<ProceduralSpawner>(FindObjectsSortMode.None)) spawner.enabled=false;
            var hud=UnityEngine.Object.FindFirstObjectByType<SimpleHud>();typeof(SimpleHud).GetMethod("PrepareHudPreview",Flags).Invoke(hud,null);
            hud.Player.transform.position=new Vector3(1000,1000,0);
        }
        private void EndCutscene()
        {
            var hud=UnityEngine.Object.FindFirstObjectByType<SimpleHud>();hud.DeclineRecruitConversation();
            Time.timeScale=1;InputReader.HudBlocked=false;
        }
        private RecruitableNpc Npc(string role,Vector3 position)
        {
            var go=new GameObject("Visible recruited "+role);go.transform.position=position;
            go.AddComponent<SpriteRenderer>().sprite=SpriteBank.CompanionSpriteForRole(role);
            var npc=go.AddComponent<RecruitableNpc>();npc.Role=role;
            npc.Player=UnityEngine.Object.FindFirstObjectByType<PlayerController>().transform;
            return npc;
        }
        [UnityTest] public IEnumerator RecruitingBeforeFirstStartRetainsVisibleRoleScale()
        {
            yield return Ready();var npc=Npc("Child",new Vector3(1001,1000,0));var go=npc.gameObject;
            npc.Recruit();EndCutscene();
            Assert.That(go.transform.localScale,Is.EqualTo(SpriteBank.CompanionScaleForRole("Child")),"Recruiting in the spawn frame must not assign a zero scale");
            Assert.That(go.GetComponent<SpriteRenderer>().enabled,Is.True);
            npc.Recruit();Assert.That(go.GetComponents<Companion>().Length,Is.EqualTo(1));
            yield return null;
            Assert.That(go.transform.localScale,Is.EqualTo(SpriteBank.CompanionScaleForRole("Child")),"Role size must not be applied twice");
            UnityEngine.Object.Destroy(go);Time.timeScale=1;yield return SceneManager.LoadSceneAsync("BootScene");
        }
        [UnityTest] public IEnumerator ChunkUnloadingPreservesRecruitedCompanions()
        {
            yield return Ready();
            foreach(var type in new[]{typeof(ProceduralSpawner),typeof(ChunkManager)}) {
                var owner=new GameObject("Companion streaming test");var manager=owner.AddComponent(type) as MonoBehaviour;manager.enabled=false;
                var npc=Npc("Hunter",new Vector3(1001,1000,0));var recruited=npc.gameObject;npc.Recruit();EndCutscene();
                var prop=new GameObject("Disposable chunk prop");
                var dataType=type.GetNestedType("ChunkData",BindingFlags.NonPublic);var data=Activator.CreateInstance(dataType,true);
                var spawned=(List<GameObject>)dataType.GetField("Spawned").GetValue(data);spawned.Add(recruited);spawned.Add(prop);
                var loaded=(IDictionary)type.GetField("_loaded",Flags).GetValue(manager);loaded.Add((1,1),data);
                type.GetField("UnloadRadius").SetValue(manager,0);
                if(type==typeof(ProceduralSpawner)) type.GetMethod("UnloadFar",Flags).Invoke(manager,new object[]{100,100});
                else {type.GetField("Player").SetValue(manager,npc.Player);type.GetMethod("UnloadFar",Flags).Invoke(manager,null);}
                yield return null;
                Assert.That(recruited!=null,Is.True,type.Name+" must preserve recruited followers");
                Assert.That(prop==null,Is.True,type.Name+" ordinary world objects must still unload");
                Assert.That(loaded.Count,Is.Zero);
                UnityEngine.Object.Destroy(recruited);UnityEngine.Object.Destroy(owner);
            }
            Time.timeScale=1;yield return SceneManager.LoadSceneAsync("BootScene");
        }
        [UnityTest] public IEnumerator PassiveAnimalsDoNotHideCiviliansAndSafeDaylightReleasesShelter()
        {
            yield return Ready();var npc=Npc("Farmer",new Vector3(1001,1000,0));npc.IsCombat=false;
            var go=npc.gameObject;yield return null;npc.Recruit();EndCutscene();var companion=go.GetComponent<Companion>();companion.enabled=false;
            var house=BuildingFactory.SpawnHouse(new Vector3(1003,1000,0)).GetComponent<Building>();
            var deer=new GameObject("Passive deer");deer.transform.position=new Vector3(1001,1001,0);var ai=deer.AddComponent<DeerAi>();ai.InitHp(10);ai.enabled=false;
            var check=typeof(Companion).GetMethod("TryAutoHide",Flags);check.Invoke(companion,null);
            Assert.That(companion.CurrentMode,Is.EqualTo(Companion.Mode.Follow),"Passive animals must not teleport civilians into shelter");
            Assert.That(go.GetComponent<SpriteRenderer>().enabled,Is.True);
            var wolf=new GameObject("Threatening wolf");wolf.transform.position=deer.transform.position;var predator=wolf.AddComponent<WolfAi>();predator.InitHp(10);predator.enabled=false;
            check.Invoke(companion,null);Assert.That(companion.CurrentMode,Is.EqualTo(Companion.Mode.Hiding));
            UnityEngine.Object.Destroy(wolf);yield return null;check.Invoke(companion,null);
            Assert.That(companion.CurrentMode,Is.EqualTo(Companion.Mode.Follow),"Safe daylight must bring sheltered followers back");
            Assert.That(go.GetComponent<SpriteRenderer>().enabled,Is.True);Assert.That(house.HostedCompanions.Contains(companion),Is.False);
            var night=UnityEngine.Object.FindFirstObjectByType<NightController>();var phase=typeof(NightController).GetField("<CurrentPhase>k__BackingField",Flags);
            phase.SetValue(night,Phase.Night);companion.HideInBuilding(house);check.Invoke(companion,null);
            Assert.That(companion.CurrentMode,Is.EqualTo(Companion.Mode.Hiding),"Noncombat night shelter must remain supported");
            phase.SetValue(night,Phase.Day);check.Invoke(companion,null);Assert.That(companion.CurrentMode,Is.EqualTo(Companion.Mode.Follow));
            UnityEngine.Object.Destroy(go);UnityEngine.Object.Destroy(deer);UnityEngine.Object.Destroy(house.gameObject);
            Time.timeScale=1;yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
