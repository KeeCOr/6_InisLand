using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public class SnowEnvironmentStyleTests
    {
        [Test] public void SnowTileIsOpaqueQuietAndHasNoStrongEdgeSeam()
        {
            var sprite=Resources.Load<Sprite>("Sprites/Environment/Snowfield/snow_ground");Assert.That(sprite,Is.Not.Null);
            var tex=sprite.texture;float lo=1,hi=0,edge=0;int samples=0;
            for(int y=0;y<tex.height;y+=16)for(int x=0;x<tex.width;x+=16)
            {var c=tex.GetPixel(x,y);Assert.That(c.a,Is.EqualTo(1f));lo=Mathf.Min(lo,c.grayscale);hi=Mathf.Max(hi,c.grayscale);}
            Assert.That(hi-lo,Is.InRange(.015f,.25f),"Snow must have visible but quiet painted variation");
            for(int i=0;i<tex.height;i+=16){edge+=Mathf.Abs(tex.GetPixel(0,i).grayscale-tex.GetPixel(tex.width-1,i).grayscale);samples++;}
            for(int i=0;i<tex.width;i+=16){edge+=Mathf.Abs(tex.GetPixel(i,0).grayscale-tex.GetPixel(i,tex.height-1).grayscale);samples++;}
            Assert.That(edge/samples,Is.LessThan(.08f),"Repeating ground must not create a high contrast tile seam");
            Assert.That(SpriteBank.SnowRock().texture.width,Is.GreaterThan(64),"World rock must use the matching painted resource");
        }
    }
}
