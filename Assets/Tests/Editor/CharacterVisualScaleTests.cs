using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace IL6.Tests
{
    public sealed class CharacterVisualScaleTests
    {
        [TestCase("hunter",1.08f)] [TestCase("warrior",1.12f)] [TestCase("farmer",1.08f)] [TestCase("cook",1.08f)]
        [TestCase("healer",1.08f)] [TestCase("aunt",1.08f)] [TestCase("uncle",1.08f)] [TestCase("child",.94f)]
        public void ActualOpaqueLocomotionHeightMatchesHeroScaleAcrossAllViews(string key,float target)
        {
            var sprites=new List<Sprite>{SpriteBank.CompanionSpriteForRole(key)};
            foreach(var facing in new[]{CharacterFacing.Front,CharacterFacing.Back,CharacterFacing.Right})
            {
                if(facing!=CharacterFacing.Right)sprites.Add(DirectionalCharacterVisual.DirectionSprite(key,facing));
                for(int i=0;i<2;i++)sprites.Add(DirectionalCharacterVisual.WalkSprite(key,facing,i));
            }
            float max=0;
            foreach(var sprite in sprites)
            {
                float height=OpaqueHeight(sprite)*SpriteBank.CompanionScaleForRole(key).y;
                Assert.That(height,Is.InRange(target*.9f,target+.001f),key+"/"+sprite.name);max=Mathf.Max(max,height);
            }
            Assert.That(max,Is.EqualTo(target).Within(.002f));
            foreach(var facing in new[]{CharacterFacing.Front,CharacterFacing.Back,CharacterFacing.Right})foreach(var phase in new[]{ActionPhase.Start,ActionPhase.Loop,ActionPhase.End})
            {
                float height=OpaqueHeight(CharacterActionVisual.Frame(key,facing,phase))*SpriteBank.CompanionScaleForRole(key).y;
                Assert.That(height,Is.InRange(target*.9f,target*1.08f),"Attack tools/poses may extend modestly but must not change character scale");
            }
        }
        private static float OpaqueHeight(Sprite sprite)
        {
            Assert.That(sprite,Is.Not.Null);var image=new Texture2D(2,2);image.LoadImage(File.ReadAllBytes(AssetDatabase.GetAssetPath(sprite)));var rect=sprite.rect;
            int min=image.height,max=-1;var pixels=image.GetPixels32();
            for(int y=(int)rect.yMin;y<(int)rect.yMax;y++)for(int x=(int)rect.xMin;x<(int)rect.xMax;x++)if(pixels[y*image.width+x].a>127){min=Mathf.Min(min,y);max=Mathf.Max(max,y);}
            Object.DestroyImmediate(image);return (max-min+1)/sprite.pixelsPerUnit;
        }
    }
}
