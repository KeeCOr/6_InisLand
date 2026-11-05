using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace IL6.Tests
{
    public sealed class CompanionNightVisibilityTests
    {
        [UnityTest] public IEnumerator NightHidesBodyHpAndShadowAndDawnRestoresPresentationOnly()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(1);
            var hud=Object.FindFirstObjectByType<SimpleHud>();
            typeof(SimpleHud).GetMethod("PrepareHudPreview",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,null);
            Time.timeScale=1;
            var cycle=GameSession.Instance.Cycle;
            cycle.Restore(new DayNightSnapshot{day=1,phase=Phase.Day});
            var go=new GameObject("Night companion");go.transform.position=new Vector3(9500,9500,0);
            var body=go.AddComponent<SpriteRenderer>();body.sprite=SpriteBank.CompanionSpriteForRole("Hunter");
            var snow=ActorSnowPresentation.Attach(body);var companion=go.AddComponent<Companion>();companion.enabled=false;
            yield return null;
            try
            {
                Assert.That(body.forceRenderingOff,Is.False);
                var mode=companion.CurrentMode;var collider=go.GetComponent<Collider2D>();bool collision=collider.enabled;
                cycle.Restore(new DayNightSnapshot{day=1,phase=Phase.Night});EventBus.Instance.Emit(new NightStartedPayload(1));
                yield return null;
                foreach(var renderer in go.GetComponentsInChildren<Renderer>(true))Assert.That(renderer.forceRenderingOff,Is.True);
                Assert.That(snow.Shadow.forceRenderingOff,Is.True);
                Assert.That(companion.CurrentMode,Is.EqualTo(mode));Assert.That(collider.enabled,Is.EqualTo(collision));
                var late=new GameObject("Late nighttime companion");late.transform.position=go.transform.position+Vector3.right*10;
                var lateBody=late.AddComponent<SpriteRenderer>();lateBody.sprite=body.sprite;late.AddComponent<Companion>().enabled=false;
                yield return null;Assert.That(lateBody.forceRenderingOff,Is.True,"Companions spawned or recruited during night also hide");
                cycle.Restore(new DayNightSnapshot{day=1,phase=Phase.Dawn});EventBus.Instance.Emit(new DawnStartedPayload(1));
                yield return null;
                Assert.That(body.forceRenderingOff,Is.False);Assert.That(lateBody.forceRenderingOff,Is.False);Assert.That(snow.Shadow.forceRenderingOff,Is.False);
                Assert.That(body.enabled,Is.True);Object.Destroy(late);
            }
            finally {Object.Destroy(go);}
            yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
