using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace IL6.Tests
{
    public sealed class StarterHouseTests
    {
        [UnityTest] public IEnumerator StartsWithFinishedHousingInsideFenceAndDoesNotRegenerateIt()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(2);
            var center=new Vector3(GameConstants.VillageCenterX,GameConstants.VillageCenterY,0);
            Building home=null,fire=null;int count=0;
            foreach(var b in Object.FindObjectsByType<Building>(FindObjectsSortMode.None))
            {
                if(b.Kind==BuildingKind.House){home=b;count++;}
                if(b.Kind==BuildingKind.Campfire)fire=b;
            }
            Assert.That(count,Is.EqualTo(1));Assert.That(home.CurrentHp,Is.EqualTo(home.MaxHp));
            Assert.That(home.transform.position,Is.EqualTo(ConstructionPlacement.Snap(home.transform.position)));
            Assert.That(RecruitableNpc.VillageCapacity(),Is.EqualTo(RecruitableNpc.FreeCapacity+RecruitableNpc.CapacityPerHouse));
            var bounds=home.GetComponent<SpriteRenderer>().bounds;float half=VillageStarter.CurrentHalfSize;
            Assert.That(bounds.min.x,Is.GreaterThan(center.x-half));Assert.That(bounds.max.x,Is.LessThan(center.x+half));
            Assert.That(bounds.min.y,Is.GreaterThan(center.y-half));Assert.That(bounds.max.y,Is.LessThan(center.y+half));
            Physics2D.SyncTransforms();
            Assert.That(home.GetComponent<Collider2D>().Distance(fire.GetComponent<Collider2D>()).isOverlapped,Is.False);
            VillageStarter.SpawnStarterVillage(center);
            int repeated=0;foreach(var b in Object.FindObjectsByType<Building>(FindObjectsSortMode.None))if(b.Kind==BuildingKind.House)repeated++;
            Assert.That(repeated,Is.EqualTo(1));
            home.TakeDamage(home.MaxHp);yield return null;
            VillageStarter.SpawnStarterVillage(center);yield return null;
            foreach(var b in Object.FindObjectsByType<Building>(FindObjectsSortMode.None))Assert.That(b.Kind,Is.Not.EqualTo(BuildingKind.House),"Destroyed starting house must stay destroyed");
            Assert.That(RecruitableNpc.VillageCapacity(),Is.EqualTo(RecruitableNpc.FreeCapacity));
            yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
