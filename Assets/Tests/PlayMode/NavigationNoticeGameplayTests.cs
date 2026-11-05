using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public class NavigationNoticeGameplayTests
    {
        [UnityTest] public IEnumerator FuelConsumptionDoesNotRaiseAttackAlertAndRecentOffscreenHitsAreGrouped()
        {
            var camera=new GameObject("Notice test camera").AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=5;camera.transform.position=new Vector3(7000,7000,-10);
            var a=BuildingFactory.SpawnHouse(new Vector3(7100,7000));var b=BuildingFactory.SpawnHouse(new Vector3(7102,7000));
            var first=a.GetComponent<Building>();var second=b.GetComponent<Building>();
            first.ConsumeFuelHp(1);
            var all=new[]{first,second};var allies=new Companion[0];
            Assert.That(ThreatNavigation.Collect(camera,camera.transform.position,all,allies,Time.time,false).Visible,Is.False);
            first.TakeDamage(1);second.TakeDamage(1);
            var notice=ThreatNavigation.Collect(camera,camera.transform.position,all,allies,Time.time,false);
            Assert.That(notice.Count,Is.EqualTo(2));Assert.That(notice.Label,Is.EqualTo("시설 피격 · 2"));
            Assert.That(notice.Position,Is.EqualTo(first.transform.position),"Equal-time damage picks the nearer target");
            Assert.That(ThreatNavigation.Collect(camera,camera.transform.position,all,allies,Time.time+5,false).Visible,Is.False);
            camera.transform.position=new Vector3(7101,7000,-10);
            Assert.That(ThreatNavigation.Collect(camera,camera.transform.position,all,allies,Time.time,false).Visible,Is.False,"Onscreen HP bars are enough");
            Object.Destroy(a);Object.Destroy(b);Object.Destroy(camera.gameObject);yield return null;
        }
    }
}
