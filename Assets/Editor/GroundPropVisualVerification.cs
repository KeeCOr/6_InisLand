using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class GroundPropVisualVerification
    {
        public static void Verify()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);Time.timeScale=1;InputReader.HudBlocked=false;
            var camera=new GameObject("Ground prop verification camera").AddComponent<Camera>();camera.tag="MainCamera";
            camera.orthographic=true;camera.orthographicSize=5.625f;camera.transform.position=new Vector3(0,0,-10);
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.93f,.95f,.98f);
            var ground=new GameObject("Snow ground").AddComponent<SpriteRenderer>();ground.sprite=Resources.Load<Sprite>("Sprites/Environment/Snowfield/snow_ground");
            ground.drawMode=SpriteDrawMode.Tiled;ground.size=Vector2.one*30;ground.sortingOrder=-10000;
            var hero=new GameObject("Hero proportion reference").AddComponent<SpriteRenderer>();hero.sprite=DirectionalCharacterVisual.HeroSideIdle("hero_axe");
            hero.transform.position=new Vector3(-9,0,0);hero.transform.localScale=new Vector3(.9f,1,1);
            var props=new List<SpriteRenderer>();var store=new ResourceStore();int i=0;
            foreach(var kind in new[]{ResourceKind.Wood,ResourceKind.Stone,ResourceKind.Ore,ResourceKind.Meat,ResourceKind.Food,ResourceKind.Frostbloom})
            {
                float x=-7.5f+i++*3;
                var site=new GameObject("World discovery "+kind);site.transform.position=new Vector3(x,1.5f,0);site.transform.localScale=Vector3.one*1.15f;
                var renderer=site.AddComponent<SpriteRenderer>();var discovery=site.AddComponent<DiscoverySite>();discovery.RewardKind=kind;
                typeof(DiscoverySite).GetMethod("Start",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(discovery,null);props.Add(renderer);
                var drop=WoodPickup.Spawn(new Vector3(x-.25f,-1.5f+.15f,0),2,store,kind);
                typeof(WoodPickup).GetField("_age",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(drop,1f);
                typeof(WoodPickup).GetMethod("Update",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(drop,null);
                props.Add(drop.transform.Find("WoodArt").GetComponent<SpriteRenderer>());
            }
            var target=new RenderTexture(640,360,24);camera.targetTexture=target;var pixels=new Texture2D(640,360,TextureFormat.RGBA32,false);
            camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,640,360),0,0);pixels.Apply();var shown=pixels.GetPixels32();
            foreach(var prop in props)prop.enabled=false;
            camera.Render();pixels.ReadPixels(new Rect(0,0,640,360),0,0);pixels.Apply();var plain=pixels.GetPixels32();int changed=0;
            for(int p=0;p<shown.Length;p++)if(Mathf.Abs(shown[p].r-plain[p].r)+Mathf.Abs(shown[p].g-plain[p].g)+Mathf.Abs(shown[p].b-plain[p].b)>24)changed++;
            if(changed<600)throw new System.InvalidOperationException("Ground props failed GPU visibility: "+changed);
            foreach(var prop in props)prop.enabled=true;
            camera.Render();pixels.ReadPixels(new Rect(0,0,640,360),0,0);pixels.Apply();File.WriteAllBytes("D:/codex_temp/InisLand-20261103/ground-props-gpu-temp.png",pixels.EncodeToPNG());
            Debug.Log("GROUND GPU VERIFIED: 12 props, "+changed+" visible pixels, 32 pixels/world unit.");
            camera.targetTexture=null;RenderTexture.active=null;Object.DestroyImmediate(target);Object.DestroyImmediate(pixels);
        }
    }
}
