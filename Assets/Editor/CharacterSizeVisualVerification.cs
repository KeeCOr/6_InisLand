using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class CharacterSizeVisualVerification
    {
        public static void Before()=>Render("before");
        public static void After()=>Render("after");
        private static void Render(string phase)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var camera=new GameObject("Character scale QA camera").AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=4.5f;
            camera.transform.position=new Vector3(0,0,-10);camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.84f,.9f,.94f);
            string[] keys={"hero_axe","hero_bow","hero_hammer","hunter","warrior","farmer","cook","healer","aunt","uncle","child"};
            for(int row=0;row<3;row++)for(int i=0;i<keys.Length;i++)
            {
                string key=keys[i];var renderer=new GameObject(key).AddComponent<SpriteRenderer>();
                renderer.sprite=row==0?DirectionalCharacterVisual.DirectionSprite(key,CharacterFacing.Front):row==1?DirectionalCharacterVisual.WalkSprite(key,CharacterFacing.Right,0):CharacterActionVisual.Frame(key,CharacterFacing.Front,ActionPhase.Loop);
                renderer.transform.position=new Vector3(-6.25f+i*1.25f,2.1f-row*2.7f,0);
                renderer.transform.localScale=key.StartsWith("hero")?new Vector3(.9f,1,1):SpriteBank.CompanionScaleForRole(key);
            }
            var target=new RenderTexture(1500,900,24);camera.targetTexture=target;camera.Render();RenderTexture.active=target;
            var image=new Texture2D(1500,900,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,1500,900),0,0);image.Apply();
            File.WriteAllBytes("D:/codex_temp/InisLand-20261103/character-size-"+phase+"-temp.png",image.EncodeToPNG());
            camera.targetTexture=null;RenderTexture.active=null;Object.DestroyImmediate(image);Object.DestroyImmediate(target);
            Debug.Log("CHARACTER SIZE GPU: "+phase+" 33 actual imported sprites at gameplay actor scales");
        }
    }
}
