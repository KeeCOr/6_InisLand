using System;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class AnimationFrameVisualVerification
    {
        public static void Verify()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var camera=new GameObject("Action frame verification camera").AddComponent<Camera>();
            camera.orthographic=true;camera.orthographicSize=11;camera.transform.position=new Vector3(0,0,-10);
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.85f,.9f,.95f);
            string[] keys={"hero_axe","hero_bow","hero_hammer","hunter","warrior","farmer","cook","healer","aunt","uncle","child"};
            CharacterFacing[] facings={CharacterFacing.Right,CharacterFacing.Front,CharacterFacing.Back};
            for(int row=0;row<keys.Length;row++)for(int column=0;column<9;column++)
            {
                var sprite=CharacterActionVisual.Frame(keys[row],facings[column/3],(ActionPhase)(column%3));
                if(sprite==null || sprite.rect.width<=1)throw new InvalidOperationException(keys[row]+" empty frame");
                var renderer=new GameObject(keys[row]+" "+column).AddComponent<SpriteRenderer>();renderer.sprite=sprite;
                renderer.transform.position=new Vector3(-7.2f+column*1.8f,9-row*1.8f,0);
            }
            var target=new RenderTexture(1200,1440,24);camera.targetTexture=target;
            var image=new Texture2D(1200,1440,TextureFormat.RGBA32,false);camera.Render();RenderTexture.active=target;
            image.ReadPixels(new Rect(0,0,1200,1440),0,0);image.Apply();
            File.WriteAllBytes("D:/codex_temp/InisLand-20261103/animation-frames-gpu-temp.png",image.EncodeToPNG());
            camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(image);
            Debug.Log("ANIMATION FRAMES GPU RENDERED: 99 actual imported directional action frames.");
        }
    }
}
