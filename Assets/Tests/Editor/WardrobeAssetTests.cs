using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace IL6.Tests
{
    public class WardrobeAssetTests
    {
        [Test]
        public void AllUpdatedWorldPosesAndActionsRetainTransparentMargins()
        {
            foreach(string key in new[]{"hero_axe","hero_bow","hero_hammer","child","uncle","healer","hunter","cook","warrior","farmer","aunt"})
            {
                foreach(var facing in new[]{CharacterFacing.Front,CharacterFacing.Back,CharacterFacing.Right})
                {
                    for(int i=0;i<2;i++) AssertWholePose(DirectionalCharacterVisual.WalkSprite(key,facing,i));
                    if(facing!=CharacterFacing.Right) AssertWholePose(DirectionalCharacterVisual.DirectionSprite(key,facing));
                }
                AssertWholePose(key.StartsWith("hero_")?DirectionalCharacterVisual.HeroSideIdle(key):SpriteBank.CompanionSpriteForRole(key));
            }
            foreach(string key in new[]{"axe","bow","logging"})
            {
                var sprites=Resources.LoadAll<Sprite>("Sprites/Player/RedHairGirl/"+key);
                Assert.That(sprites.Length,Is.EqualTo(key=="logging"?12:16));
                foreach(var sprite in sprites) AssertWholePose(sprite);
            }
        }
        private static void AssertWholePose(Sprite sprite)
        {
            Assert.That(sprite,Is.Not.Null);
            var source=new Texture2D(2,2,TextureFormat.RGBA32,false);
            try
            {
                source.LoadImage(File.ReadAllBytes(AssetDatabase.GetAssetPath(sprite.texture)));
                var r=sprite.rect;int x=Mathf.RoundToInt(r.x),y=Mathf.RoundToInt(r.y),width=Mathf.RoundToInt(r.width),height=Mathf.RoundToInt(r.height);
                var pixels=source.GetPixels(x,y,width,height);
                int opaque=0;
                for(int py=0;py<height;py++) for(int px=0;px<width;px++)
                {
                    if(pixels[py*width+px].a<=.5f) continue;
                    opaque++;
                    Assert.That(px>0 && py>0 && px<width-1 && py<height-1,Is.True,
                        sprite.name+" crosses a frame boundary at "+px+","+py);
                }
                Assert.That(opaque,Is.GreaterThan(100),sprite.name);
            }
            finally { Object.DestroyImmediate(source); }
        }
    }
}
