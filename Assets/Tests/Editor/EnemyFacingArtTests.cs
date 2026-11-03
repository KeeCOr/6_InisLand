using System.IO;
using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public sealed class EnemyFacingArtTests
    {
        private static Bounds Visible(Sprite sprite,string path)
        {
            var image=new Texture2D(2,2);image.LoadImage(File.ReadAllBytes(path));
            var pixels=image.GetPixels32();int minX=image.width,minY=image.height,maxX=0,maxY=0;
            for(int y=0;y<image.height;y++)for(int x=0;x<image.width;x++)if(pixels[y*image.width+x].a>32)
            {minX=Mathf.Min(minX,x);minY=Mathf.Min(minY,y);maxX=Mathf.Max(maxX,x+1);maxY=Mathf.Max(maxY,y+1);}
            Object.DestroyImmediate(image);
            Vector3 min=(new Vector2(minX,minY)-sprite.pivot)/sprite.pixelsPerUnit;
            Vector3 max=(new Vector2(maxX,maxY)-sprite.pivot)/sprite.pixelsPerUnit;
            return new Bounds((min+max)/2,max-min);
        }
        [Test] public void AllEnemyTypesHaveDistinctFrontBackAndSideFrames()
        {
            foreach(string key in EnemyFacingVisual.Keys) {
                var front=EnemyFacingVisual.Frame(key,CharacterFacing.Front);var back=EnemyFacingVisual.Frame(key,CharacterFacing.Back);var side=EnemyFacingVisual.Frame(key,CharacterFacing.Right);
                Assert.That(front,Is.Not.Null,key);Assert.That(back,Is.Not.Null,key);Assert.That(side,Is.Not.Null,key);
                Assert.That(front,Is.Not.SameAs(back));Assert.That(back,Is.Not.SameAs(side));
                Assert.That(EnemyFacingVisual.Frame(key,CharacterFacing.Left),Is.SameAs(side));
            }
        }
        [Test] public void DirectionChangesPreserveVisibleHeightAndFootAnchor()
        {
            foreach(string key in EnemyFacingVisual.Keys) {
                var source=Resources.Load<Sprite>("Sprites/Enemies/"+key);
                var reference=Visible(source,"Assets/Resources/Sprites/Enemies/"+key+".png");
                foreach(var facing in new[]{CharacterFacing.Front,CharacterFacing.Back,CharacterFacing.Right}) {
                    var frame=EnemyFacingVisual.Frame(key,facing);
                    var actual=Visible(frame,"Assets/Resources/Sprites/Enemies/Directions20261103/"+frame.name+".png");
                    Assert.That(actual.size.y,Is.EqualTo(reference.size.y).Within(.01f),key+" height");
                    Assert.That(actual.min.y,Is.EqualTo(reference.min.y).Within(.01f),key+" foot");
                    Assert.That(actual.center.x,Is.EqualTo(reference.center.x).Within(.01f),key+" horizontal anchor");
                }
            }
        }
        [Test] public void IdleAndTinyDirectionsKeepLastFacingWithoutRotatingTheRoot()
        {
            var go=new GameObject("Enemy idle facing");var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=SpriteBank.QuadrupedRobot();
            try {
                var facing=EnemyFacingVisual.Apply(renderer,Vector2.left,CharacterFacing.Right);var image=renderer.sprite;
                facing=EnemyFacingVisual.Apply(renderer,new Vector2(.001f,.001f),facing);
                Assert.That(facing,Is.EqualTo(CharacterFacing.Left));Assert.That(renderer.flipX,Is.True);Assert.That(renderer.sprite,Is.SameAs(image));
                Assert.That(go.transform.rotation,Is.EqualTo(Quaternion.identity));Assert.That(go.transform.localScale,Is.EqualTo(Vector3.one));
            } finally {Object.DestroyImmediate(go);}
        }
    }
}
