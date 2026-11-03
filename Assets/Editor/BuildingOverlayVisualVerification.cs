using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class BuildingOverlayVisualVerification
    {
        public static void Verify()
        {
            foreach(string guid in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/Prefabs/Buildings"}))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);
                var prefab=PrefabUtility.LoadPrefabContents(path);
                try {
                    var marker=prefab.transform.Find("RoleMarker");
                    if(marker!=null){Object.DestroyImmediate(marker.gameObject);PrefabUtility.SaveAsPrefabAsset(prefab,path);}
                } finally {PrefabUtility.UnloadPrefabContents(prefab);}
            }
            AssetDatabase.SaveAssets();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var camera=new GameObject("Building overlay QA camera").AddComponent<Camera>();camera.tag="MainCamera";
            camera.orthographic=true;camera.orthographicSize=3.3f;camera.transform.position=new Vector3(0,2.1f,-10);
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.88f,.92f,.96f);
            var house=BuildingFactory.SpawnHouse(Vector3.zero);
            if(house.transform.Find("RoleMarker")!=null)throw new System.InvalidOperationException("House role marker still overlays art");
            var target=new RenderTexture(640,640,24);camera.targetTexture=target;
            var image=new Texture2D(640,640,TextureFormat.RGBA32,false);
            camera.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,640,640),0,0);image.Apply();
            File.WriteAllBytes("D:/codex_temp/InisLand-20261103/building-overlay-gpu-temp.png",image.EncodeToPNG());
            camera.targetTexture=null;RenderTexture.active=null;Object.DestroyImmediate(target);Object.DestroyImmediate(image);
            Debug.Log("BUILDING OVERLAY GPU RENDERED: house without miniature HUD icon.");
        }
    }
}
