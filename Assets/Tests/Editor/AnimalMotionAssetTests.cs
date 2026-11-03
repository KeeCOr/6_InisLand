using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public class AnimalMotionAssetTests
    {
        [TestCase("deer")][TestCase("rabbit")][TestCase("boar")][TestCase("wolf")][TestCase("bear")]
        [TestCase("mammoth")][TestCase("fox")][TestCase("snow_hare")][TestCase("dog")][TestCase("hawk")]
        public void EveryAnimalHasThreeDirectionsAndTwoDistinctMovementFrames(string key)
        {
            foreach(string direction in new[]{"front","back","side"})
            {
                string path="Sprites/Animals/Motion20261026/"+key+"/";
                var idle=Resources.Load<Sprite>(path+"idle_"+direction);
                var first=Resources.Load<Sprite>(path+"move_"+direction+"_0");
                var second=Resources.Load<Sprite>(path+"move_"+direction+"_1");
                Assert.That(idle,Is.Not.Null,key+direction+" idle missing");
                Assert.That(first,Is.Not.Null,key+direction+" first movement missing");
                Assert.That(second,Is.Not.Null,key+direction+" second movement missing");
                Assert.That(first,Is.Not.SameAs(second));
                Assert.That(first.pixelsPerUnit,Is.EqualTo(idle.pixelsPerUnit));
                Assert.That(second.pixelsPerUnit,Is.EqualTo(idle.pixelsPerUnit));
                if(key=="hawk")Assert.That(first.pivot.y/first.rect.height,Is.InRange(.15f,.8f),"Flight anchors follow the breast, not changing wing tips");
                else Assert.That(first.pivot.y/first.rect.height,Is.InRange(0f,.4f));
            }
        }
    }
}
