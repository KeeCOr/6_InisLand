using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace IL6.Tests
{
    public class WorldReadabilityTests
    {
        private static IEnumerator Ready()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(2);
            var hud=Object.FindFirstObjectByType<SimpleHud>();
            typeof(SimpleHud).GetMethod("PrepareHudPreview",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,null);
        }
        [UnityTest] public IEnumerator SceneBalancesForestAndWildlifeAndKeepsVillageArrivals()
        {
            yield return Ready();
            GameSession.Instance.Cycle.Restore(new DayNightSnapshot{day=1,phase=Phase.Day,elapsedInPhase=0});
            var spawner=Object.FindFirstObjectByType<ProceduralSpawner>();spawner.enabled=false;
            Assert.That(spawner.TreeChance,Is.EqualTo(.52f).Within(.001f));
            Assert.That(spawner.DeerChance,Is.EqualTo(.035f).Within(.001f));
            Assert.That(spawner.MaxProceduralAnimals,Is.EqualTo(16));
            Assert.That(spawner.NpcChance,Is.EqualTo(.05f).Within(.001f));
            Assert.That(spawner.ScarceNpcIntervalSec,Is.EqualTo(12f));
            Assert.That(spawner.LowNpcIntervalSec,Is.EqualTo(20f));
            Assert.That(spawner.DailyNpcIntervalSec,Is.EqualTo(30f));
            Assert.That(spawner.MaxAliveNpcs,Is.EqualTo(5));
            foreach(var companion in Object.FindObjectsByType<Companion>(FindObjectsSortMode.None))
                Assert.That(companion.SeparationRadius,Is.GreaterThanOrEqualTo(1.1f),"Authored followers must match the new spacing");
            Vector2 center=new Vector2(GameConstants.VillageCenterX,GameConstants.VillageCenterY);
            foreach(var npc in Object.FindObjectsByType<RecruitableNpc>(FindObjectsSortMode.None)) Object.Destroy(npc.gameObject);
            yield return null;
            for(int i=0;i<spawner.ScarceMaxAliveNpcs;i++)
                ProceduralSpawner.CreateNpcFromArchetype(ProceduralSpawner._npcArchetypes[0],"Remote candidate "+i,new Vector3(2000+i*3,2000));
            Physics2D.SyncTransforms();spawner.SpawnDailyVillageNpc();
            RecruitableNpc local=null;int count=0;
            foreach(var npc in Object.FindObjectsByType<RecruitableNpc>(FindObjectsSortMode.None))
                if(Vector2.Distance(npc.transform.position,center)<=12f){local=npc;count++;}
            Assert.That(count,Is.EqualTo(1),"Remote NPC cap must not prevent a visible village candidate");
            Assert.That(ProceduralSpawner.IsOutsideVillageForNpc(local.transform.position),Is.True);
            Physics2D.SyncTransforms();
            foreach(var hit in Physics2D.OverlapCircleAll(local.transform.position,.65f))
                Assert.That(hit.GetComponent<Building>(),Is.Null,"Candidate must not arrive inside a fence or building");
            for(int i=0;i<10;i++)spawner.SpawnDailyVillageNpc();
            count=0;
            foreach(var npc in Object.FindObjectsByType<RecruitableNpc>(FindObjectsSortMode.None))
                if(npc.enabled && npc.CanRecruit() && Vector2.Distance(npc.transform.position,center)<=12f)count++;
            Assert.That(count,Is.EqualTo(5),"Repeated arrivals must stop at five nearby candidates without limiting recruitment");
            foreach(var npc in Object.FindObjectsByType<RecruitableNpc>(FindObjectsSortMode.None))
                Assert.That(ProceduralSpawner.IsOutsideVillageForNpc(npc.transform.position),Is.True,"Candidates must remain outside the village");
            yield return SceneManager.LoadSceneAsync("BootScene");
        }
        [UnityTest] public IEnumerator CanopyFadesForCandidatesAndRecruitedCompanionsThenRestores()
        {
            yield return Ready();
            var method=typeof(ProceduralSpawner).GetMethod("CreateTree",BindingFlags.Static|BindingFlags.NonPublic);
            var tree=(GameObject)method.Invoke(null,new object[]{5000f,5000f});
            var npc=ProceduralSpawner.CreateNpcFromArchetype(ProceduralSpawner._npcArchetypes[0],"Visible candidate",new Vector3(5000,5000));
            yield return new WaitForSeconds(.5f);
            var sprite=tree.GetComponent<SpriteRenderer>();
            Assert.That(tree.GetComponent<TreeVisibility>(),Is.Not.Null);
            Assert.That(sprite.color.a,Is.LessThan(.4f));
            npc.GetComponent<RecruitableNpc>().Recruit();npc.GetComponent<Companion>().enabled=false;
            yield return new WaitForSeconds(.3f);
            Assert.That(sprite.color.a,Is.LessThan(.4f),"Recruitment must preserve canopy readability");
            npc.transform.position+=Vector3.right*30;
            yield return new WaitForSeconds(.5f);
            Assert.That(sprite.color.a,Is.EqualTo(1f).Within(.01f));
            Object.Destroy(npc);Object.Destroy(tree);yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
