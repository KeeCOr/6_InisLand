using NUnit.Framework;
using UnityEngine;

namespace IL6.Tests
{
    public sealed class ScreenEnemyPresenceTests
    {
        [Test] public void CameraBoundsIncludePartialEnemiesAndExcludeHiddenDeadOrOffscreenOnes()
        {
            var cameraGo=new GameObject("Presence test camera");var camera=cameraGo.AddComponent<Camera>();
            camera.orthographic=true;camera.orthographicSize=5;camera.aspect=1;camera.transform.position=new Vector3(9000,9000,-10);
            var go=new GameObject("Presence enemy");go.transform.position=new Vector3(9000,9000,0);
            var texture=new Texture2D(32,32);var sprite=Sprite.Create(texture,new Rect(0,0,32,32),new Vector2(.5f,.5f),32);
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=sprite;
            var enemy=go.AddComponent<Zombie>();enemy.InitHp(100);enemy.enabled=false;
            try
            {
                Assert.That(ScreenEnemyPresence.Any(camera),Is.True);
                go.transform.position+=Vector3.right*5.3f;Assert.That(ScreenEnemyPresence.Any(camera),Is.True,"Partly visible sprite at edge counts");
                go.transform.position+=Vector3.right;Assert.That(ScreenEnemyPresence.Any(camera),Is.False);
                go.transform.position=new Vector3(9000,9000,0);
                renderer.enabled=false;Assert.That(ScreenEnemyPresence.Any(camera),Is.False);renderer.enabled=true;
                camera.cullingMask=0;Assert.That(ScreenEnemyPresence.Any(camera),Is.False);camera.cullingMask=-1;
                go.SetActive(false);Assert.That(ScreenEnemyPresence.Any(camera),Is.False);go.SetActive(true);
                enemy.InitHp(0);Assert.That(ScreenEnemyPresence.Any(camera),Is.False,"Death counts immediately before Destroy");
                Assert.That(ScreenEnemyPresence.Any(null),Is.False);
            }
            finally {Object.DestroyImmediate(go);Object.DestroyImmediate(cameraGo);Object.DestroyImmediate(sprite);Object.DestroyImmediate(texture);}
        }
    }
}
