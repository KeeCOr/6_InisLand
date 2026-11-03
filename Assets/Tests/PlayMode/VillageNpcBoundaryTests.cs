using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public class VillageNpcBoundaryTests
    {
        [UnityTest] public IEnumerator EveryCandidateSpawnPathStaysBeyondVillageFence()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(2);
            var hud=Object.FindFirstObjectByType<SimpleHud>();
            typeof(SimpleHud).GetMethod("PrepareHudPreview",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,null);
            var spawner=Object.FindFirstObjectByType<ProceduralSpawner>();spawner.enabled=false;
            GameSession.Instance.Cycle.Restore(new DayNightSnapshot{day=1,phase=Phase.Day,elapsedInPhase=0});
            foreach(var npc in Object.FindObjectsByType<RecruitableNpc>(FindObjectsSortMode.None)) Object.Destroy(npc.gameObject);
            yield return null;
            var center=new Vector3(GameConstants.VillageCenterX,GameConstants.VillageCenterY,0);
            for(int i=0;i<8;i++)
            {
                spawner.SpawnDailyVillageNpc();
                foreach(var npc in Object.FindObjectsByType<RecruitableNpc>(FindObjectsSortMode.None))
                {
                    AssertOutside(npc.transform.position);Object.Destroy(npc.gameObject);
                }
                yield return null;
            }
            var explicitNpc=ProceduralSpawner.CreateNpcFromArchetype(ProceduralSpawner._npcArchetypes[0],"Explicit candidate",center);
            AssertOutside(explicitNpc.transform.position);
            var random=typeof(ProceduralSpawner).GetMethod("CreateNpc",BindingFlags.Static|BindingFlags.NonPublic);
            var inside=(GameObject)random.Invoke(null,new object[]{center.x,center.y,new SeededRng(42)});
            Assert.That(inside,Is.Null,"Procedural candidates must skip village interior");
            Object.Destroy(explicitNpc);
            var sceneNpc=new GameObject("Scene candidate");sceneNpc.transform.position=center;
            sceneNpc.AddComponent<RecruitableNpc>();yield return null;AssertOutside(sceneNpc.transform.position);
            Object.Destroy(sceneNpc);yield return SceneManager.LoadSceneAsync("BootScene");
        }
        private static void AssertOutside(Vector3 position)
        {
            float dx=Mathf.Abs(position.x-GameConstants.VillageCenterX),dy=Mathf.Abs(position.y-GameConstants.VillageCenterY);
            Assert.That(Mathf.Max(dx,dy),Is.GreaterThanOrEqualTo(VillageStarter.CurrentHalfSize+1.25f));
        }
    }
}
