using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace IL6.Tests
{
    public class AnimalSizeTests
    {
        [TestCase("rabbit")][TestCase("snow_hare")][TestCase("fox")][TestCase("dog")][TestCase("wolf")]
        [TestCase("boar")][TestCase("deer")][TestCase("bear")][TestCase("mammoth")][TestCase("hawk")]
        public void PaintedBodySizeMatchesSpeciesAcrossAnimationImports(string key)
        {
            var profile=AnimalSizeProfile.For(key);float largestWidth=0;
            var side=AnimalMotionVisual.SpriteFor(key,CharacterFacing.Right,false);
            float ppu=side.pixelsPerUnit;
            foreach(string direction in new[]{"front","back","side"})
                foreach(string pose in new[]{"idle_"+direction,"move_"+direction+"_0","move_"+direction+"_1"})
                {
                    var sprite=Resources.Load<Sprite>("Sprites/Animals/Motion20261026/"+key+"/"+pose);
                    Assert.That(sprite.pixelsPerUnit,Is.EqualTo(ppu));
                    var size=PaintedSize(sprite)*profile.ActorScale;
                    largestWidth=Mathf.Max(largestWidth,size.x);
                    if(pose=="idle_side" && key!="hawk") Assert.That(size.y,Is.EqualTo(profile.Size).Within(.005f),key);
                }
            if(key=="hawk")Assert.That(largestWidth,Is.EqualTo(profile.Size).Within(.005f));
        }

        [Test] public void RelativeScaleKeepsFoxSmallAndDeerSubstantial()
        {
            Assert.That(AnimalSizeProfile.For("fox").Size,Is.LessThan(.7f));
            Assert.That(AnimalSizeProfile.For("deer").Size,Is.GreaterThan(1.8f));
            Assert.That(AnimalSizeProfile.For("snow_hare").Size/AnimalSizeProfile.For("rabbit").Size,Is.InRange(1f,1.3f));
            Assert.That(AnimalSizeProfile.For("dog").Size,Is.LessThan(AnimalSizeProfile.For("wolf").Size));
            Assert.That(AnimalSizeProfile.For("mammoth").Size,Is.InRange(3f,4f));
        }

        private static Vector2 PaintedSize(Sprite sprite)
        {
            var texture=new Texture2D(2,2);texture.LoadImage(File.ReadAllBytes(AssetDatabase.GetAssetPath(sprite.texture)));
            var pixels=texture.GetPixels32();int minX=texture.width,minY=texture.height,maxX=0,maxY=0;
            for(int y=0;y<texture.height;y++)for(int x=0;x<texture.width;x++)if(pixels[y*texture.width+x].a>127)
            {minX=Mathf.Min(minX,x);maxX=Mathf.Max(maxX,x);minY=Mathf.Min(minY,y);maxY=Mathf.Max(maxY,y);}
            Object.DestroyImmediate(texture);
            return new Vector2(maxX-minX+1,maxY-minY+1)/sprite.pixelsPerUnit;
        }
    }
}
