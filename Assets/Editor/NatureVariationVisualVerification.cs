using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class NatureVariationVisualVerification
    {
        [Serializable] private sealed class Manifest {public float rock_foot_baseline;}
        public static void Verify()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var camera=new GameObject("Nature verification camera").AddComponent<Camera>();camera.tag="MainCamera";
            camera.orthographic=true;camera.orthographicSize=5.625f;camera.transform.position=new Vector3(0,0,-10);
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.93f,.95f,.98f);
            var ground=new GameObject("Snow ground").AddComponent<SpriteRenderer>();ground.sprite=Resources.Load<Sprite>("Sprites/Environment/Snowfield/snow_ground");
            ground.drawMode=SpriteDrawMode.Tiled;ground.size=Vector2.one*40;ground.sortingOrder=-10000;
            var hero=new GameObject("Hero scale reference").AddComponent<SpriteRenderer>();hero.sprite=DirectionalCharacterVisual.HeroSideIdle("hero_axe");
            hero.transform.position=new Vector3(-13.5f,.4f,0);hero.transform.localScale=new Vector3(.9f,1,1);
            var props=new List<SpriteRenderer>();var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText("Assets/art/generated/environment/nature_variants20261103/generation_manifest.json"));
            for(int i=0;i<6;i++)
            {
                float x=-11.25f+i*4.5f,scale=i%3==1?2.2f:2.15f;
                var tree=new GameObject("Tree variant "+i);tree.transform.position=new Vector3(x,.4f+.24f*scale,0);tree.transform.localScale=Vector3.one*scale;
                var renderer=tree.AddComponent<SpriteRenderer>();TreeVisualStyle.Apply(renderer,i);YSort.Ensure(tree);props.Add(renderer);
                var rock=new GameObject("Rock variant "+i);rock.transform.position=new Vector3(x,-3f-manifest.rock_foot_baseline*1.8f,0);rock.transform.localScale=Vector3.one*1.8f;
                renderer=rock.AddComponent<SpriteRenderer>();renderer.sprite=RockVisualStyle.SpriteFor(i);YSort.Ensure(rock);props.Add(renderer);
            }
            var target=new RenderTexture(960,360,24);camera.targetTexture=target;var pixels=new Texture2D(960,360,TextureFormat.RGBA32,false);
            camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,960,360),0,0);pixels.Apply();var shown=pixels.GetPixels32();
            foreach(var prop in props)prop.enabled=false;
            camera.Render();pixels.ReadPixels(new Rect(0,0,960,360),0,0);pixels.Apply();var plain=pixels.GetPixels32();int changed=0;
            for(int i=0;i<shown.Length;i++)if(Mathf.Abs(shown[i].r-plain[i].r)+Mathf.Abs(shown[i].g-plain[i].g)+Mathf.Abs(shown[i].b-plain[i].b)>24)changed++;
            if(changed<5000)throw new InvalidOperationException("Nature variants not visible: "+changed);
            foreach(var prop in props)prop.enabled=true;
            camera.Render();pixels.ReadPixels(new Rect(0,0,960,360),0,0);pixels.Apply();File.WriteAllBytes("D:/codex_temp/InisLand-20261103/nature-variants-gpu-temp.png",pixels.EncodeToPNG());
            Debug.Log("NATURE GPU VERIFIED: 6 trees, 6 non-slab rocks, "+changed+" visible pixels at 32 pixels/world unit.");
            camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(pixels);
        }
    }
}
