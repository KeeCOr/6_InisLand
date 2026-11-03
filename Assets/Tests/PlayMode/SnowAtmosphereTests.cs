using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public class SnowAtmosphereTests
    {
        [UnityTest]
        public IEnumerator SnowStartsVisibleCoversZoomAndStopsDuringPause()
        {
            Time.timeScale=1;
            var go=new GameObject("Snow camera test");go.transform.position=new Vector3(5000,5000,-10);
            var cam=go.AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=8;
            var snow=go.AddComponent<SnowEmitter>();yield return null;
            foreach(var ps in new[]{snow.DistantSnow,snow.NearbySnow})
            {
                Assert.That(ps.particleCount,Is.GreaterThan(0));
                Assert.That(ps.transform.localPosition.z,Is.GreaterThan(cam.nearClipPlane));
                Assert.That(ps.transform.localPosition.z,Is.LessThan(cam.farClipPlane));
                Assert.That(ps.main.simulationSpace,Is.EqualTo(ParticleSystemSimulationSpace.Local));
                Assert.That(ps.GetComponent<ParticleSystemRenderer>().sortingOrder,Is.GreaterThan(9000));
                Assert.That(ps.main.maxParticles,Is.LessThanOrEqualTo(1300));
            }
            cam.orthographicSize=14;yield return null;
            Assert.That(snow.DistantSnow.shape.scale.y,Is.GreaterThan(28));
            Time.timeScale=0;yield return null;float t=snow.DistantSnow.time;yield return null;yield return null;
            Assert.That(snow.DistantSnow.time,Is.EqualTo(t).Within(.001f));
            Time.timeScale=1;Object.Destroy(go);yield return null;
        }
    }
}
