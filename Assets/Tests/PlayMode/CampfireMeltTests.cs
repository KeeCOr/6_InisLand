using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace IL6.Tests
{
    public sealed class CampfireMeltTests
    {
        [UnityTest] public IEnumerator WarmCampfireExposesGroundAndCoolingRefreezesWithoutScalingOrLeaking()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return null;
            var go=VillageStarter.SpawnCampfire(new Vector3(9800,9800));yield return null;yield return null;
            Type type=typeof(CampfireAura).Assembly.GetType("IL6.CampfireSnowMelt");Assert.That(type,Is.Not.Null,"Campfire ground melt is missing");
            var visual=go.GetComponent(type);Assert.That(visual,Is.Not.Null);
            var renderer=(SpriteRenderer)type.GetProperty("GroundRenderer").GetValue(visual);
            var step=type.GetMethod("UpdateVisual");float Amount()=>(float)type.GetProperty("Amount").GetValue(visual);
            Assert.That(renderer.sprite,Is.Not.Null);Assert.That(renderer.enabled,Is.True);Assert.That(Amount(),Is.EqualTo(1).Within(.01f));
            Assert.That(renderer.transform.parent,Is.Null);Assert.That(renderer.sortingOrder,Is.GreaterThan(-9500));Assert.That(renderer.sortingOrder,Is.LessThan(-9475));
            float width=renderer.bounds.size.x,height=renderer.bounds.size.y;
            go.transform.localScale=new Vector3(5,3,1);go.transform.position+=Vector3.right*2;step.Invoke(visual,new object[]{0f});
            Assert.That(renderer.bounds.size.x,Is.EqualTo(width).Within(.01f));Assert.That(renderer.bounds.size.y,Is.EqualTo(height).Within(.01f));
            Assert.That(renderer.transform.position.x,Is.EqualTo(go.transform.position.x));
            var aura=go.GetComponent<CampfireAura>();aura.Fuel=0;step.Invoke(visual,new object[]{4f});Assert.That(Amount(),Is.EqualTo(.5f).Within(.01f));
            Time.timeScale=0;float paused=Amount();yield return new WaitForSecondsRealtime(.1f);Assert.That(Amount(),Is.EqualTo(paused).Within(.001f));
            Time.timeScale=1;step.Invoke(visual,new object[]{4f});Assert.That(renderer.enabled,Is.False);
            aura.AddFuel(50);step.Invoke(visual,new object[]{2f});Assert.That(renderer.enabled,Is.True);Assert.That(Amount(),Is.EqualTo(1).Within(.01f));
            aura.Radius=99;step.Invoke(visual,new object[]{0f});Assert.That(renderer.bounds.size.x,Is.LessThanOrEqualTo(6.41f));
            go.SetActive(false);Assert.That(renderer.enabled,Is.False);go.SetActive(true);step.Invoke(visual,new object[]{0f});Assert.That(renderer.enabled,Is.True);
            var decal=renderer.gameObject;Object.Destroy(go);yield return null;yield return null;Assert.That(decal==null,Is.True,"Scene-owned ground must be cleaned up with its fire");
            Time.timeScale=1;InputReader.HudBlocked=false;yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
