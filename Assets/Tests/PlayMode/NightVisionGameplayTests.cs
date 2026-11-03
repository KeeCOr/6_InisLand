using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public sealed class NightVisionGameplayTests
    {
        private const BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
        private static float Alpha(Texture2D texture,Camera camera,Vector3 point)
        {
            var p=camera.WorldToScreenPoint(point);return texture.GetPixelBilinear(p.x/Screen.width,p.y/Screen.height).a;
        }
        [UnityTest] public IEnumerator ActualPlayerAndHeatStayLitDuringCameraMotionAndPause()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(2);
            var hud=Object.FindFirstObjectByType<SimpleHud>();typeof(SimpleHud).GetMethod("PrepareHudPreview",Flags).Invoke(hud,null);
            foreach(var spawner in Object.FindObjectsByType<ProceduralSpawner>(FindObjectsSortMode.None))spawner.enabled=false;
            foreach(var night in Object.FindObjectsByType<NightController>(FindObjectsSortMode.None))night.enabled=false;
            var mask=Object.FindFirstObjectByType<VisionMask>();var camera=Camera.main;var follow=camera.GetComponent<CameraFollow>();if(follow!=null)follow.enabled=false;
            camera.orthographic=true;camera.orthographicSize=5;camera.transform.position=new Vector3(5000,5000,-10);
            hud.Player.enabled=false;hud.Player.transform.position=new Vector3(5002,5002.5f,0);mask.Target=hud.Player.transform;mask.Cam=camera;mask.SetRadius(.7f);
            var fire=VillageStarter.SpawnCampfire(new Vector3(4995,4997.5f,0));
            yield return null;
            var update=typeof(VisionMask).GetMethod("UpdateMask",Flags);var field=typeof(VisionMask).GetField("_maskTex",Flags);
            var cycle=GameSession.Instance.Cycle;var saved=cycle.Snapshot();
            try {
                cycle.Restore(new DayNightSnapshot{day=11,phase=Phase.Night});
                Assert.That((bool)typeof(VisionMask).GetProperty("NightVisible",Flags).GetValue(mask),Is.True);
                update.Invoke(mask,null);var texture=(Texture2D)field.GetValue(mask);
                Assert.That(Alpha(texture,camera,hud.Player.transform.position),Is.LessThan(.05f));
                Assert.That(Alpha(texture,camera,fire.transform.position),Is.LessThan(.05f));
                Assert.That(Alpha(texture,camera,new Vector3(5002,4997.5f,0)),Is.GreaterThan(.8f));
                fire.GetComponent<CampfireAura>().enabled=false;update.Invoke(mask,null);
                Assert.That(Alpha(texture,camera,fire.transform.position),Is.GreaterThan(.8f),"Disabled heat must not leave a ghost vision hole");
                Time.timeScale=0;camera.transform.position+=Vector3.up;update.Invoke(mask,null);
                Assert.That(Alpha(texture,camera,hud.Player.transform.position),Is.LessThan(.05f),"Paused redraw must retain projection alignment");
                cycle.Restore(new DayNightSnapshot{day=12,phase=Phase.Dawn});
                Assert.That((bool)typeof(VisionMask).GetProperty("NightVisible",Flags).GetValue(mask),Is.False,"Restoring Dawn without an event must still remove darkness");
                Object.Destroy(mask.gameObject);yield return null;
                Assert.That(texture==null,Is.True,"Runtime destruction must release the owned mask texture");
            } finally {cycle.Restore(saved);Time.timeScale=1;InputReader.HudBlocked=false;Object.Destroy(fire);}
            yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
