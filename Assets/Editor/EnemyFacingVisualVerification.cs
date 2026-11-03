using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class EnemyFacingVisualVerification
    {
        public static void Verify()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var camera=new GameObject("Enemy direction QA camera").AddComponent<Camera>();camera.tag="MainCamera";
            camera.orthographic=true;camera.orthographicSize=7;camera.transform.position=new Vector3(0,0,-10);
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.88f,.92f,.96f);
            Vector2[] directions={Vector2.left,Vector2.down,Vector2.up,Vector2.right};
            for(int row=0;row<EnemyFacingVisual.Keys.Length;row++)for(int column=0;column<4;column++)
            {
                string key=EnemyFacingVisual.Keys[row];var go=new GameObject(key+" "+column);
                var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=Resources.Load<Sprite>("Sprites/Enemies/"+key);
                EnemyFacingVisual.Apply(renderer,directions[column],CharacterFacing.Right);
                go.transform.position=new Vector3(-3+column*2,5.5f-row*2.2f,0);go.transform.localScale=Vector3.one*1.5f;
            }
            var target=new RenderTexture(768,1024,24);camera.targetTexture=target;
            var image=new Texture2D(768,1024,TextureFormat.RGBA32,false);camera.Render();RenderTexture.active=target;
            image.ReadPixels(new Rect(0,0,768,1024),0,0);image.Apply();
            File.WriteAllBytes("D:/codex_temp/InisLand-20261103/enemy-facing-gpu-temp.png",image.EncodeToPNG());
            camera.targetTexture=null;RenderTexture.active=null;Object.DestroyImmediate(target);Object.DestroyImmediate(image);
            Debug.Log("ENEMY FACING GPU RENDERED: 6 types x 4 facings.");
        }
    }
}
