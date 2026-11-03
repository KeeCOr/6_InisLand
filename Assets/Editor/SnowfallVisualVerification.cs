using System.IO;
using System.Reflection;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace IL6.EditorBuild
{
    // Development-only GPU check at the same ~32 pixels/world-unit as the 720p game.
    public static class SnowfallVisualVerification
    {
        public static void Verify()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            Time.timeScale=1;InputReader.HudBlocked=false;
            var camera=new GameObject("Verification camera").AddComponent<Camera>();
            camera.orthographic=true;camera.orthographicSize=3.375f;camera.transform.position=new Vector3(0,0,-10);
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.93f,.95f,.98f);
            var ground=new GameObject("Snow ground").AddComponent<SpriteRenderer>();
            ground.sprite=Resources.Load<Sprite>("Sprites/Environment/Snowfield/snow_ground");
            ground.drawMode=SpriteDrawMode.Tiled;ground.size=Vector2.one*20;ground.sortingOrder=-10000;
            var overlay=new GameObject("Snow patch overlay").AddComponent<SpriteRenderer>();
            overlay.sprite=ground.sprite;overlay.drawMode=SpriteDrawMode.Tiled;overlay.size=Vector2.one*20;
            overlay.color=new Color(1,1,1,.3f);overlay.sortingOrder=-9500;
            var field=new GameObject("Footprint render check").AddComponent<SnowFootprintField>();
            // This command runs in Edit Mode, where normal MonoBehaviour lifecycle
            // callbacks have not run. Initialize the same runtime path explicitly.
            var flags=BindingFlags.Instance|BindingFlags.NonPublic;
            if(typeof(SnowFootprintField).GetField("_material",flags).GetValue(field)==null)
                typeof(SnowFootprintField).GetMethod("Awake",flags).Invoke(field,null);
            if(typeof(SnowFootprintField).GetField("_unsubDay",flags).GetValue(field)==null)
                typeof(SnowFootprintField).GetMethod("OnEnable",flags).Invoke(field,null);
            EventBus.Instance.Emit(new DayStartedPayload(1));
            for(int i=0;i<9;i++)field.Stamp(new Vector2(i%2==0?-.095f:.095f,-1.7f+i*.42f),Vector2.up,i%2==0);
            typeof(SnowFootprintField).GetMethod("LateUpdate",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(field,null);
            var target=new RenderTexture(384,216,24);camera.targetTexture=target;
            var pixels=new Texture2D(384,216,TextureFormat.RGBA32,false);
            camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,384,216),0,0);pixels.Apply();var marked=pixels.GetPixels32();
            foreach(var renderer in field.GetComponentsInChildren<MeshRenderer>())renderer.enabled=false;
            camera.Render();pixels.ReadPixels(new Rect(0,0,384,216),0,0);pixels.Apply();var plain=pixels.GetPixels32();int changed=0;
            for(int i=0;i<marked.Length;i++)if(Mathf.Abs(marked[i].r-plain[i].r)+Mathf.Abs(marked[i].g-plain[i].g)+Mathf.Abs(marked[i].b-plain[i].b)>12)changed++;
            if(changed<70)throw new System.InvalidOperationException("Bootprints not visibly rendered over snow: "+changed);
            foreach(var renderer in field.GetComponentsInChildren<MeshRenderer>())renderer.enabled=true;
            camera.Render();pixels.ReadPixels(new Rect(0,0,384,216),0,0);pixels.Apply();
            File.WriteAllBytes("D:/codex_temp/InisLand-20261103/snow-visual-temp.png",pixels.EncodeToPNG());
            Debug.Log("FOOTPRINT GPU VERIFIED: "+changed+" visibly changed pixels, nine marks, 32 pixels/world unit.");
            camera.targetTexture=null;RenderTexture.active=null;Object.DestroyImmediate(target);Object.DestroyImmediate(pixels);
        }
    }
}
