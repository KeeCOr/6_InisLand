using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public class GuideTutorialTests
    {
        [Test] public void HeroMonologueStillRequiresMovementWoodAndFence()
        {
            var story=new StoryTutorial(0,true);Assert.That(story.CurrentLine.Speaker,Is.EqualTo("Hero"));
            story.AdvanceDialogue();Assert.That(story.CurrentLine.Speaker,Is.EqualTo("Hero"));
            while(story.IsDialogue){Assert.That(story.CurrentLine.Speaker,Is.EqualTo("Hero"));Assert.That(story.CurrentLine.Text,Does.Not.Contain("나그네"));story.AdvanceDialogue();}Assert.That(story.Stage,Is.EqualTo(StoryTutorialStage.Explore));
            Assert.That(story.Observe(0,10,true),Is.False);Assert.That(story.Observe(3,0,false),Is.True);
            while(story.IsDialogue){Assert.That(story.CurrentLine.Speaker,Is.EqualTo("Hero"));Assert.That(story.CurrentLine.Text,Does.Not.Contain("나그네"));story.AdvanceDialogue();}Assert.That(story.Observe(0,1,false),Is.True);
            while(story.IsDialogue){Assert.That(story.CurrentLine.Speaker,Is.EqualTo("Hero"));Assert.That(story.CurrentLine.Text,Does.Not.Contain("나그네"));story.AdvanceDialogue();}Assert.That(story.Observe(0,0,true),Is.True);
            while(story.IsDialogue){Assert.That(story.CurrentLine.Speaker,Is.EqualTo("Hero"));Assert.That(story.CurrentLine.Text,Does.Not.Contain("나그네"));story.AdvanceDialogue();}Assert.That(story.IsComplete,Is.True);
        }
        [Test] public void LoadingStoryAndBothDialoguePortraitsAreAvailable()
        {
            Assert.That(NewAdventureLoading.Story.Length,Is.EqualTo(4));
            foreach(var key in new[]{"waiting","storm","ruins","promise"})Assert.That(Resources.Load<Texture2D>("UI/StoryIllustrations/"+key),Is.Not.Null);
            Assert.That(DialoguePortraits.ForRole("Hero"),Is.Not.Null);Assert.That(DialoguePortraits.ForRole("Hunter"),Is.Not.Null);
            var save=JsonUtility.FromJson<SaveFileV1>("{\"version\":1,\"currentDay\":5}");Assert.That(save.npcTutorialEnabled,Is.False);Assert.That(save.storyIntroSeen,Is.False);
        }
    }
}
