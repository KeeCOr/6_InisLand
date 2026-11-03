using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace IL6.Tests
{
    public class VillageReferenceFeedbackTests
    {
        [UnityTest]
        public IEnumerator HearthLightStaysBelowPeopleAndRespectsFuelAndPause()
        {
            Time.timeScale=1;
            var fire=VillageStarter.SpawnCampfire(new Vector3(4000,4000));
            var aura=fire.GetComponent<CampfireAura>();
            yield return null; yield return null;
            var light=aura.GlowRenderer;
            Assert.That(light.sprite,Is.Not.Null);
            Assert.That(light.transform.parent,Is.Null,"Ground light cannot inherit a building SortingGroup");
            Assert.That(light.sortingOrder,Is.InRange(-9499,-9401));
            Assert.That(light.bounds.size.x,Is.EqualTo(5.4f).Within(.02f));
            Assert.That(light.bounds.size.y,Is.EqualTo(3.8f).Within(.02f));
            Assert.That(light.color.a,Is.GreaterThan(0));
            aura.Fuel=0; yield return null;
            Assert.That(light.color.a,Is.Zero);
            aura.AddFuel(20); yield return null;
            Assert.That(light.color.a,Is.GreaterThan(0));
            Time.timeScale=0; float fuel=aura.Fuel; var color=light.color;
            yield return null; yield return null;
            Assert.That(aura.Fuel,Is.EqualTo(fuel)); Assert.That(light.color,Is.EqualTo(color));
            Time.timeScale=1;Object.Destroy(fire);yield return null;
        }

        [Test]
        public void MaterialAudioHasDistinctBoundedWaveforms()
        {
            AudioClip previous=null;
            foreach(ImpactMaterial material in System.Enum.GetValues(typeof(ImpactMaterial)))
            {
                var clip=Sfx.ImpactClip(material);
                Assert.That(clip,Is.SameAs(Sfx.ImpactClip(material)));
                Assert.That(clip,Is.Not.SameAs(previous));
                Assert.That(clip.length,Is.InRange(.1f,.18f));
                var data=new float[clip.samples];Assert.That(clip.GetData(data,0),Is.True);
                float peak=0;foreach(float value in data) peak=Mathf.Max(peak,Mathf.Abs(value));
                Assert.That(peak,Is.InRange(.1f,.66f));
                Assert.That(Mathf.Abs(data[0]),Is.LessThan(.001f));
                Assert.That(Mathf.Abs(data[data.Length-1]),Is.LessThan(.001f));
                previous=clip;
            }
        }

        [UnityTest]
        public IEnumerator ImpactFragmentsExpireWithoutStoppingTime()
        {
            Time.timeScale=1;
            var center=new Vector3(4200,4200);
            GameFeel.MaterialImpact(center,ImpactMaterial.Metal);
            int count=0;
            foreach(var sr in Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
                if(sr.name=="__materialImpact" && Vector3.Distance(sr.transform.position,center)<1) count++;
            Assert.That(count,Is.EqualTo(4));Assert.That(Time.timeScale,Is.EqualTo(1));
            yield return new WaitForSeconds(.25f);
            foreach(var sr in Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
                if(sr.name=="__materialImpact") Assert.That(Vector3.Distance(sr.transform.position,center),Is.GreaterThan(1));
        }
    }
}
