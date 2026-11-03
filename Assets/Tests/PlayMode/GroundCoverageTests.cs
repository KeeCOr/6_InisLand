using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace IL6.Tests
{
    public class GroundCoverageTests
    {
        [UnityTest] public IEnumerator GroundCoversViewportAfterTravelAcrossPositiveAndNegativeChunksAndZoom()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(2);
            var camera=Camera.main;camera.GetComponent<CameraFollow>().enabled=false;
            var ground=GameObject.Find("Ground").GetComponent<SpriteRenderer>();
            Assert.That(ground.drawMode,Is.EqualTo(SpriteDrawMode.Tiled));
            Assert.That(ground.sprite,Is.EqualTo(Resources.Load<Sprite>("Sprites/Environment/Snowfield/snow_ground")));
            foreach(float aspect in new[]{16f/9f,20f/9f,4f/3f})
            foreach(Vector2 center in new[]{new Vector2(12,12),new Vector2(80,80),new Vector2(-80,-80),new Vector2(9.99f,-10.01f)})
            {
                camera.aspect=aspect;camera.orthographicSize=16;
                camera.transform.position=new Vector3(center.x,center.y,-10);
                yield return null;yield return null;
                var bounds=ground.bounds;
                Assert.That(ground.transform.localScale,Is.EqualTo(Vector3.one),"Zoom must not stretch the painted snow grain");
                float tileWidth=ground.sprite.bounds.size.x,tileHeight=ground.sprite.bounds.size.y;
                Assert.That(bounds.min.x/tileWidth,Is.EqualTo(Mathf.Round(bounds.min.x/tileWidth)).Within(.001f));
                Assert.That(bounds.min.y/tileHeight,Is.EqualTo(Mathf.Round(bounds.min.y/tileHeight)).Within(.001f),"Texture phase must stay fixed in world space");
                var half=new Vector2(camera.orthographicSize*camera.aspect,camera.orthographicSize);
                Assert.That(bounds.min.x,Is.LessThan(center.x-half.x));
                Assert.That(bounds.max.x,Is.GreaterThan(center.x+half.x));
                Assert.That(bounds.min.y,Is.LessThan(center.y-half.y));
                Assert.That(bounds.max.y,Is.GreaterThan(center.y+half.y));
            }
            Assert.That(ground.color.a,Is.EqualTo(1));
            Assert.That(ground.sprite.texture.GetPixel(0,0).a,Is.EqualTo(1),"Ground corners must be opaque");
            yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
