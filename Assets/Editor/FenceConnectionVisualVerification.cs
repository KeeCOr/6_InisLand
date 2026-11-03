using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class FenceConnectionVisualVerification
    {
        public static void Verify()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var camera=new GameObject("Fence QA camera").AddComponent<Camera>();camera.tag="MainCamera";
            camera.orthographic=true;camera.orthographicSize=4;camera.transform.position=new Vector3(0,.5f,-10);
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.86f,.9f,.94f);
            for(int x=-3;x<=3;x++) {
                if(x==0)VillageStarter.SpawnGate(new Vector3(x,-2,0));
                else if(Mathf.Abs(x)<3)VillageStarter.SpawnFence(new Vector3(x,-2,0),0,FencePieceType.Center);
                if(Mathf.Abs(x)<3)VillageStarter.SpawnFence(new Vector3(x,2,0),0,FencePieceType.Center);
            }
            for(int y=-1;y<=1;y++)foreach(int x in new[]{-3,3})VillageStarter.SpawnVerticalFence(new Vector3(x,y,0),VerticalFencePieceType.Center);
            VillageStarter.SpawnFenceCorner(new Vector3(-3,-2,0),FenceCornerPieceType.BottomLeft);
            VillageStarter.SpawnFenceCorner(new Vector3(3,-2,0),FenceCornerPieceType.BottomRight);
            VillageStarter.SpawnFenceCorner(new Vector3(-3,2,0),FenceCornerPieceType.TopLeft);
            VillageStarter.SpawnFenceCorner(new Vector3(3,2,0),FenceCornerPieceType.TopRight);
            var hero=new GameObject("Hero scale reference").AddComponent<SpriteRenderer>();hero.sprite=DirectionalCharacterVisual.HeroSideIdle("hero_axe");hero.transform.position=Vector3.zero;
            var target=new RenderTexture(960,640,24);camera.targetTexture=target;var image=new Texture2D(960,640,TextureFormat.RGBA32,false);
            camera.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,960,640),0,0);image.Apply();
            File.WriteAllBytes("D:/codex_temp/InisLand-20261103/fence-connected-gpu-temp.png",image.EncodeToPNG());
            camera.targetTexture=null;RenderTexture.active=null;Object.DestroyImmediate(target);Object.DestroyImmediate(image);
            Debug.Log("FENCE CONNECTION GPU RENDERED: horizontal, vertical, four corners, gate and hero scale.");
        }
    }
}
