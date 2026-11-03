using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class ForestExpansionVisualVerification
    {
        public static void Verify()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var camera=new GameObject("Forest verification camera").AddComponent<Camera>();camera.tag="MainCamera";
            camera.orthographic=true;camera.orthographicSize=6.75f;camera.transform.position=new Vector3(0,0,-10);
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.93f,.95f,.98f);
            var ground=new GameObject("Snow ground").AddComponent<SpriteRenderer>();ground.sprite=Resources.Load<Sprite>("Sprites/Environment/Snowfield/snow_ground");
            ground.drawMode=SpriteDrawMode.Tiled;ground.size=Vector2.one*40;ground.sortingOrder=-10000;
            var hero=new GameObject("Hero scale reference").AddComponent<SpriteRenderer>();hero.sprite=DirectionalCharacterVisual.HeroSideIdle("hero_axe");
            hero.transform.position=new Vector3(-10.5f,-4.6f,0);hero.transform.localScale=new Vector3(.9f,1,1);
            var props=new List<SpriteRenderer>();
            for(int i=0;i<SpriteBank.TreeVariantCount;i++)
            {
                float x=-7+(i%3)*7,foot=2.5f-(i/3)*4.3f,scale=i%3==1?2.2f:2.15f;
                var tree=new GameObject("Tree variant "+i);tree.transform.position=new Vector3(x,foot+.24f*scale,0);tree.transform.localScale=Vector3.one*scale;
                var renderer=tree.AddComponent<SpriteRenderer>();TreeVisualStyle.Apply(renderer,i);YSort.Ensure(tree);props.Add(renderer);
            }
            var target=new RenderTexture(768,432,24);camera.targetTexture=target;var pixels=new Texture2D(768,432,TextureFormat.RGBA32,false);
            camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,768,432),0,0);pixels.Apply();var shown=pixels.GetPixels32();
            foreach(var prop in props)prop.enabled=false;
            camera.Render();pixels.ReadPixels(new Rect(0,0,768,432),0,0);pixels.Apply();var plain=pixels.GetPixels32();int changed=0;
            for(int i=0;i<shown.Length;i++)if(Mathf.Abs(shown[i].r-plain[i].r)+Mathf.Abs(shown[i].g-plain[i].g)+Mathf.Abs(shown[i].b-plain[i].b)>24)changed++;
            if(changed<5000)throw new InvalidOperationException("Forest variants not visible: "+changed);
            foreach(var prop in props)prop.enabled=true;
            camera.Render();pixels.ReadPixels(new Rect(0,0,768,432),0,0);pixels.Apply();File.WriteAllBytes("D:/codex_temp/InisLand-20261103/forest-expansion-gpu-temp.png",pixels.EncodeToPNG());
            Debug.Log("FOREST GPU VERIFIED: 9 trees, "+changed+" visible pixels at32 pixels/world unit.");
            camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(pixels);
        }
    }
}
