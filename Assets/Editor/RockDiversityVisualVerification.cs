using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class RockDiversityVisualVerification
    {
        public static void Verify()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var camera=new GameObject("Rock verification camera").AddComponent<Camera>();
            camera.orthographic=true;camera.orthographicSize=3.6f;camera.transform.position=new Vector3(0,0,-10);
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.93f,.95f,.98f);
            var ground=new GameObject("Snow ground").AddComponent<SpriteRenderer>();ground.sprite=Resources.Load<Sprite>("Sprites/Environment/Snowfield/snow_ground");
            ground.drawMode=SpriteDrawMode.Tiled;ground.size=Vector2.one*40;ground.sortingOrder=-10000;
            for(int i=0;i<RockVisualStyle.VariantCount;i++)
            {
                var sr=new GameObject("Rock "+i).AddComponent<SpriteRenderer>();sr.sprite=RockVisualStyle.SpriteFor(i);
                float scale=1.8f*new[]{.68f,.84f,1f,1.18f,1.38f}[i%5];
                sr.transform.position=new Vector3(-6.5f+(i%5)*3.2f,1.25f-(i/5)*3f+.38175f*scale);
                sr.transform.localScale=Vector3.one*scale;
            }
            var hero=new GameObject("Hero reference").AddComponent<SpriteRenderer>();hero.sprite=DirectionalCharacterVisual.HeroSideIdle("hero_axe");hero.transform.position=new Vector3(8,-1.5f);
            var target=new RenderTexture(1200,480,24);camera.targetTexture=target;
            var pixels=new Texture2D(1200,480,TextureFormat.RGBA32,false);
            camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,1200,480),0,0);pixels.Apply();
            File.WriteAllBytes("D:/codex_temp/InisLand-20261103/rock-diversity-temp.png",pixels.EncodeToPNG());
            camera.targetTexture=null;RenderTexture.active=null;Object.DestroyImmediate(target);Object.DestroyImmediate(pixels);
            Debug.Log("ROCK DIVERSITY RENDERED: ten sprites, five scales, snowy ground and hero reference.");
        }
    }
}
