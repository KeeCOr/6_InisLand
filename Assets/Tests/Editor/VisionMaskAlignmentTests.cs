using System.Reflection;
using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public sealed class VisionMaskAlignmentTests
    {
        private const BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
        [Test] public void OffCenterVisionHoleMatchesWorldProjectionInsteadOfMirroredPosition()
        {
            var cameraGo=new GameObject("Vision alignment camera");var camera=cameraGo.AddComponent<Camera>();
            camera.orthographic=true;camera.orthographicSize=5;camera.transform.position=new Vector3(0,0,-10);
            var go=new GameObject("Vision alignment test");var mask=go.AddComponent<VisionMask>();mask.Cam=camera;mask.MaskWidth=256;
            Texture2D texture=null;
            try {
                Vector3 point=new Vector3(1,2.5f,0);
                typeof(VisionMask).GetMethod("AddVisionHole",Flags).Invoke(mask,new object[]{point,.6f});
                typeof(VisionMask).GetMethod("RebuildMaskTexture",Flags).Invoke(mask,null);
                texture=(Texture2D)typeof(VisionMask).GetField("_maskTex",Flags).GetValue(mask);
                var screen=camera.WorldToScreenPoint(point);float x=screen.x/Screen.width,y=screen.y/Screen.height;
                Assert.That(texture.GetPixelBilinear(x,y).a,Is.LessThan(.05f),"Light must be centered on the actual off-center actor");
                Assert.That(texture.GetPixelBilinear(x,1-y).a,Is.GreaterThan(.8f),"An empty mirrored position must remain dark");
            } finally {Object.DestroyImmediate(go);Object.DestroyImmediate(cameraGo);if(texture!=null)Object.DestroyImmediate(texture);}
        }
        [Test] public void ResizingAndLifecycleCleanupReleaseItsTextures()
        {
            var go=new GameObject("Vision texture ownership");var mask=go.AddComponent<VisionMask>();
            var rebuild=typeof(VisionMask).GetMethod("RebuildMaskTexture",Flags);var field=typeof(VisionMask).GetField("_maskTex",Flags);
            Texture2D first=null,second=null;
            try {
                mask.MaskWidth=64;rebuild.Invoke(mask,null);first=(Texture2D)field.GetValue(mask);
                mask.MaskWidth=128;rebuild.Invoke(mask,null);second=(Texture2D)field.GetValue(mask);
                Assert.That(first==null,Is.True,"Resolution changes must release the previous runtime texture");
                // EditMode-created MonoBehaviours do not receive the normal play lifecycle.
                typeof(VisionMask).GetMethod("OnDestroy",Flags).Invoke(mask,null);
                Assert.That(second==null,Is.True,"Lifecycle cleanup must release the mask texture");
            } finally {if(go!=null)Object.DestroyImmediate(go);if(first!=null)Object.DestroyImmediate(first);if(second!=null)Object.DestroyImmediate(second);}
        }
    }
}
