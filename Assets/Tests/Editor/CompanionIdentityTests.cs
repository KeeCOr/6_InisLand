using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public class CompanionIdentityTests
    {
        [TestCase("Hunter",.771241867f)][TestCase("Warrior",.805882400f)]
        [TestCase("Farmer",.786274540f)][TestCase("Cook",.756302552f)]
        [TestCase("Healer",.831785368f)][TestCase("Aunt",.877995667f)]
        [TestCase("Uncle",.742362084f)][TestCase("Child",.750980400f)]
        public void NewArtKeepsActorScaleAndUsesOneScaleAcrossAllPoses(string role,float previousScale)
        {
            var scale=SpriteBank.CompanionScaleForRole(role);Assert.That(scale.x,Is.EqualTo(previousScale).Within(.00001f));
            var idle=SpriteBank.CompanionSpriteForRole(role);Assert.That(idle,Is.Not.Null);
            string key=DirectionalCharacterVisual.KeyForRole(role);
            foreach(var facing in new[]{CharacterFacing.Front,CharacterFacing.Back,CharacterFacing.Right})
            {
                if(facing!=CharacterFacing.Right)
                    Assert.That(DirectionalCharacterVisual.DirectionSprite(key,facing).pixelsPerUnit,Is.EqualTo(idle.pixelsPerUnit).Within(.001f));
                for(int frame=0;frame<2;frame++)
                {
                    var walking=DirectionalCharacterVisual.WalkSprite(key,facing,frame);
                    Assert.That(walking.pixelsPerUnit,Is.EqualTo(idle.pixelsPerUnit).Within(.001f));
                    Assert.That(walking.bounds.size.y*scale.y,Is.InRange(1.1f,1.55f));
                }
            }
            Assert.That(DialoguePortraits.ForRole(role),Is.Not.Null);
        }
        [Test] public void EveryStoryLineIsTheDaughtersMonologueAndHasASceneIllustration()
        {
            foreach(int stage in new[]{0,2,4,6})
            {
                var story=new StoryTutorial(stage);int count=0;
                while(story.IsDialogue && count++<20)
                {
                    Assert.That(story.CurrentLine.Speaker,Is.EqualTo("Hero"));
                    var image=Resources.Load<Texture2D>(story.IllustrationPath);Assert.That(image,Is.Not.Null,story.IllustrationPath);
                    Assert.That((float)image.width/image.height,Is.InRange(1.7f,1.85f));
                    story.AdvanceDialogue();
                }
                Assert.That(count,Is.LessThan(20));
            }
            var opening=new StoryTutorial();Assert.That(opening.IllustrationKey,Is.EqualTo("waiting"));
            for(int i=0;i<3;i++) opening.AdvanceDialogue();Assert.That(opening.IllustrationKey,Is.EqualTo("storm"));
            opening.AdvanceDialogue();Assert.That(opening.IllustrationKey,Is.EqualTo("ruins"));
        }
    }
}
