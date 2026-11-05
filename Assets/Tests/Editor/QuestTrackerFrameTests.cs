using System.IO;
using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public sealed class QuestTrackerFrameTests
    {
        [Test] public void GeneratedQuestRibbonIsPresentAndHasTransparentOutsideCorners()
        {
            Assert.That(Resources.Load<Texture2D>(QuestTrackerFrameSkin.RibbonPath),Is.Not.Null);
            var image=new Texture2D(2,2);
            try
            {
                image.LoadImage(File.ReadAllBytes("Assets/Resources/"+QuestTrackerFrameSkin.RibbonPath+".png"));
                Assert.That(image.width,Is.GreaterThan(1000));
                Assert.That(image.GetPixel(0,0).a,Is.LessThan(.1f));
            }
            finally {Object.DestroyImmediate(image);}
        }
        [Test] public void NineSlicesCoverEveryTrackerHeightWithoutOverlapOrStretchedCorners()
        {
            Assert.That(Resources.Load<Texture2D>(QuestTrackerFrameSkin.FramePath),Is.Not.Null);
            foreach(float height in new[]{112,164,208,216,276,300})
            {
                var panel=new Rect(100,100,280,height);float area=0;
                for(int y=0;y<3;y++)for(int x=0;x<3;x++)
                {
                    var cell=QuestTrackerFrameSkin.Slice(panel,x,y);area+=cell.width*cell.height;
                    Assert.That(panel.Contains(cell.center),Is.True);
                    var uv=QuestTrackerFrameSkin.Source(1577,839,x,y);
                    Assert.That(uv.xMin,Is.GreaterThanOrEqualTo(0));Assert.That(uv.xMax,Is.LessThanOrEqualTo(1.00001f));
                    Assert.That(uv.yMin,Is.GreaterThanOrEqualTo(0));Assert.That(uv.yMax,Is.LessThanOrEqualTo(1.00001f));
                    if(x!=1 && y!=1)Assert.That(cell.size,Is.EqualTo(new Vector2(16,16)));
                    for(int yy=0;yy<3;yy++)for(int xx=0;xx<3;xx++)if(yy*3+xx>y*3+x)Assert.That(cell.Overlaps(QuestTrackerFrameSkin.Slice(panel,xx,yy)),Is.False);
                }
                Assert.That(area,Is.EqualTo(panel.width*panel.height).Within(.01f));
            }
        }
        [Test] public void GeneratedBackgroundKeepsQuestTextReadableOverBrightSnow()
        {
            var image=new Texture2D(2,2);image.LoadImage(File.ReadAllBytes("Assets/Resources/"+QuestTrackerFrameSkin.FramePath+".png"));
            float Linear(float c)=>c<=.04045f?c/12.92f:Mathf.Pow((c+.055f)/1.055f,2.4f);
            float Luma(Color c)=>.2126f*Linear(c.r)+.7152f*Linear(c.g)+.0722f*Linear(c.b);
            try
            {
                foreach(float height in new[]{112,148,160,216,300})
                {
                    var panel=new Rect(100,100,280,height);var body=QuestTrackerFrameSkin.Body(panel);
                    Assert.That(body.yMin,Is.EqualTo(panel.y+38));
                    Assert.That(body.yMax,Is.EqualTo(panel.yMax));
                }
                for(int y=35;y<=65;y+=5)for(int x=35;x<=65;x+=5)
                {
                    var pixel=image.GetPixel(image.width*x/100,image.height*y/100);
                    Assert.That(pixel.a,Is.GreaterThan(.95f));var background=Color.Lerp(Color.white,pixel,pixel.a*QuestTrackerFrameSkin.BodyOpacity);
                    foreach(var text in new[]{new Color(.98f,.97f,.92f),new Color(.84f,.89f,.91f),new Color(.62f,.87f,.74f)})
                        Assert.That((Luma(text)+.05f)/(Luma(background)+.05f),Is.GreaterThanOrEqualTo(4.5f));
                }
            }
            finally {Object.DestroyImmediate(image);}
        }
    }
}
