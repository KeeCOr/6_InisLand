using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class GatherCompletionVisualVerification
    {
        public static void Verify()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var camera=new GameObject("FX verification camera").AddComponent<Camera>();
            camera.orthographic=true;camera.orthographicSize=5;camera.transform.position=new Vector3(0,0,-10);
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.93f,.95f,.98f);
            var ground=new GameObject("Snow ground").AddComponent<SpriteRenderer>();ground.sprite=Resources.Load<Sprite>("Sprites/Environment/Snowfield/snow_ground");
            ground.drawMode=SpriteDrawMode.Tiled;ground.size=Vector2.one*40;ground.sortingOrder=-10000;
            var hero=new GameObject("Hero scale reference").AddComponent<SpriteRenderer>();hero.sprite=DirectionalCharacterVisual.HeroSideIdle("hero_axe");
            hero.transform.position=new Vector3(-4.5f,0,0);hero.transform.localScale=new Vector3(.9f,1,1);
            var props=new List<SpriteRenderer>();string[] materials={"wood","stone","plant"},phases={"start","loop","end"};
            for(int row=0;row<3;row++)for(int col=0;col<3;col++)
            {
                var renderer=new GameObject(materials[row]+" "+phases[col]).AddComponent<SpriteRenderer>();
                renderer.sprite=Resources.Load<Sprite>("Sprites/FX/Gather20261103/"+materials[row]+"_"+phases[col]);
                renderer.transform.position=new Vector3(-2+col*3,2-row*2,0);props.Add(renderer);
            }
            var target=new RenderTexture(640,320,24);camera.targetTexture=target;var pixels=new Texture2D(640,320,TextureFormat.RGBA32,false);
            camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,640,320),0,0);pixels.Apply();var shown=pixels.GetPixels32();
            foreach(var prop in props)prop.enabled=false;
            camera.Render();pixels.ReadPixels(new Rect(0,0,640,320),0,0);pixels.Apply();var plain=pixels.GetPixels32();int changed=0;
            for(int i=0;i<shown.Length;i++)if(Mathf.Abs(shown[i].r-plain[i].r)+Mathf.Abs(shown[i].g-plain[i].g)+Mathf.Abs(shown[i].b-plain[i].b)>24)changed++;
            if(changed<25)throw new InvalidOperationException("Gather effects invisible: "+changed);
            foreach(var prop in props)prop.enabled=true;
            camera.Render();pixels.ReadPixels(new Rect(0,0,640,320),0,0);pixels.Apply();File.WriteAllBytes("D:/codex_temp/InisLand-20261103/gather-fx-gpu-temp.png",pixels.EncodeToPNG());
            Debug.Log("GATHER GPU VERIFIED: "+changed+" visible pixels at 32 pixels/world unit.");
            camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(pixels);
        }
    }
}
