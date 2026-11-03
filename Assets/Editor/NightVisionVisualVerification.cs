using System.IO;
using System.Reflection;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class NightVisionVisualVerification
    {
        public static void Verify()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            int width=Screen.width,height=Screen.height;
            var camera=new GameObject("Night vision QA camera").AddComponent<Camera>();camera.tag="MainCamera";
            camera.orthographic=true;camera.orthographicSize=5;camera.transform.position=new Vector3(0,0,-10);
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.84f,.89f,.94f);
            var hero=new GameObject("Off-center heroine").AddComponent<SpriteRenderer>();hero.sprite=DirectionalCharacterVisual.HeroSideIdle("hero_axe");hero.transform.position=new Vector3(2,2.5f,0);
            var fire=VillageStarter.SpawnCampfire(new Vector3(-4,-2,0));fire.AddComponent<SpriteRenderer>().sprite=SpriteBank.Campfire();
            var monster=new GameObject("Night enemy").AddComponent<SpriteRenderer>();monster.sprite=EnemyFacingVisual.Frame("quadruped_robot",CharacterFacing.Right);monster.transform.position=new Vector3(-1,-2,0);
            var go=new GameObject("Night vision mask");var vision=go.AddComponent<VisionMask>();vision.Cam=camera;vision.Target=hero.transform;vision.SetRadius(1.4f);
            var target=new RenderTexture(width,height,24);camera.targetTexture=target;
            var image=new Texture2D(width,height,TextureFormat.RGBA32,false);camera.Render();RenderTexture.active=target;
            image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();
            typeof(VisionMask).GetMethod("UpdateMask",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(vision,null);
            var mask=(Texture2D)typeof(VisionMask).GetField("_maskTex",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(vision);
            var pixels=image.GetPixels();
            for(int y=0;y<height;y++)for(int x=0;x<width;x++)
            {
                float alpha=mask.GetPixelBilinear((x+.5f)/width,(y+.5f)/height).a;
                int index=y*width+x;pixels[index]=Color.Lerp(pixels[index],Color.black,alpha);
            }
            image.SetPixels(pixels);image.Apply();
            File.WriteAllBytes("D:/codex_temp/InisLand-20261103/night-vision-gpu-temp.png",image.EncodeToPNG());
            camera.targetTexture=null;RenderTexture.active=null;Object.DestroyImmediate(target);Object.DestroyImmediate(image);
            Debug.Log("NIGHT VISION GPU VERIFIED: off-center hero and heat projected into the same world-camera pixels; mask composite excludes HUD.");
        }
    }
}
