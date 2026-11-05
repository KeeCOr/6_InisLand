using NUnit.Framework;
using UnityEditor;
using System.IO;
using UnityEngine;
namespace IL6.Tests
{
    public sealed class AnimationFrameIntegrityTests
    {
        [Test] public void AnimationSpriteSlicesContainVisiblePixelsAndClipsKeepValidReferences()
        {
            string[] roots={"Assets/Resources/Sprites/Characters","Assets/Resources/Sprites/Animals/Motion20261026","Assets/Resources/Sprites/Player/RedHairGirl","Assets/Resources/Sprites/Enemies/Directions20261103"};
            int checkedFrames=0;
            foreach(string root in roots)foreach(string path in Directory.GetFiles(root,"*.png",SearchOption.AllDirectories))
            {
                var image=new Texture2D(2,2);image.LoadImage(File.ReadAllBytes(path));var pixels=image.GetPixels32();
                try {
                    foreach(var asset in AssetDatabase.LoadAllAssetsAtPath(path.Replace('\\','/')))if(asset is Sprite sprite)
                    {
                        var rect=sprite.rect;Assert.That(rect.width,Is.GreaterThan(1),path+"/"+sprite.name);Assert.That(rect.height,Is.GreaterThan(1),path+"/"+sprite.name);
                        int visible=0;
                        for(int y=Mathf.RoundToInt(rect.yMin);y<Mathf.RoundToInt(rect.yMax);y++)
                            for(int x=Mathf.RoundToInt(rect.xMin);x<Mathf.RoundToInt(rect.xMax);x++)
                                if(pixels[y*image.width+x].a>32)visible++;
                        Assert.That(visible,Is.GreaterThan(64),path+"/"+sprite.name+" is transparent");checkedFrames++;
                    }
                } finally {Object.DestroyImmediate(image);}
            }
            Assert.That(checkedFrames,Is.GreaterThanOrEqualTo(342));
            foreach(string guid in AssetDatabase.FindAssets("t:AnimationClip",new[]{"Assets/Resources/Sprites/Player/RedHairGirl"}))
            {
                var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(guid));
                foreach(var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
                    if(binding.type==typeof(SpriteRenderer) && binding.propertyName=="m_Sprite")
                        foreach(var key in AnimationUtility.GetObjectReferenceCurve(clip,binding))
                        {Assert.That(key.value,Is.InstanceOf<Sprite>(),clip.name);Assert.That(((Sprite)key.value).rect.width,Is.GreaterThan(1),clip.name);}
            }
        }
        [Test] public void ActionFramesHaveRealImageAreasForEveryDirectionAndPhase()
        {
            foreach(string key in new[]{"hero_axe","hero_bow","hero_hammer","hunter","warrior","farmer","cook","healer","aunt","uncle","child"})
                foreach(var facing in new[]{CharacterFacing.Right,CharacterFacing.Front,CharacterFacing.Back})
                    foreach(var phase in new[]{ActionPhase.Start,ActionPhase.Loop,ActionPhase.End})
                    {
                        var frame=CharacterActionVisual.Frame(key,facing,phase);Assert.That(frame,Is.Not.Null,key);
                        Assert.That(frame.rect.width,Is.GreaterThan(1),key+" "+facing+" "+phase);
                        Assert.That(frame.rect.height,Is.GreaterThan(1),key+" "+facing+" "+phase);
                        Assert.That(frame.bounds.size.sqrMagnitude,Is.GreaterThan(.01f),key);
                    }
        }
    }
}
