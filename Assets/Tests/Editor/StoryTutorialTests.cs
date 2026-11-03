using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public class StoryTutorialTests
    {
        private static void FinishDialogue(StoryTutorial story) {for(int i=0;i<20 && story.IsDialogue;i++) story.AdvanceDialogue();}
        [Test] public void StoryMovesFromFarewellThroughRealTasksToRebuildingPromise()
        {
            var story=new StoryTutorial();Assert.That(story.CurrentLine.Speaker,Is.EqualTo("Hero"));
            Assert.That(story.Observe(100,100,true),Is.False,"Dialogue cannot be completed by world actions");
            FinishDialogue(story);Assert.That(story.Stage,Is.EqualTo(StoryTutorialStage.Explore));
            Assert.That(story.AdvanceDialogue(),Is.False,"Task needs play rather than clicking Continue");
            Assert.That(story.Observe(1,0,false),Is.False);Assert.That(story.Observe(1.5f,0,false),Is.True);
            FinishDialogue(story);Assert.That(story.Stage,Is.EqualTo(StoryTutorialStage.GatherWood));
            Assert.That(story.Observe(0,0,false),Is.False);Assert.That(story.Observe(0,1,false),Is.True);
            FinishDialogue(story);Assert.That(story.Stage,Is.EqualTo(StoryTutorialStage.BuildFence));
            Assert.That(story.Observe(0,0,false),Is.False);Assert.That(story.Observe(0,0,true),Is.True);
            FinishDialogue(story);Assert.That(story.IsComplete,Is.True);
        }
        [Test] public void StormIsRevealedOnlyAfterFarewellAndSkipAlwaysCompletes()
        {
            var story=new StoryTutorial();Assert.That(story.StormRevealed,Is.False);
            for(int i=0;i<4;i++) story.AdvanceDialogue();Assert.That(story.StormRevealed,Is.True);
            story.Skip();Assert.That(story.IsComplete,Is.True);Assert.That(story.IsDialogue,Is.False);
        }
        [Test] public void ResourcesAndLegacySaveMigrationAreSafe()
        {
            Assert.That(DialoguePortraits.ForRole("Father"),Is.Not.Null);
            Assert.That(DialoguePortraits.ForRole("아버지"),Is.SameAs(DialoguePortraits.ForRole("Father")));
            Assert.That(Resources.LoadAll<Sprite>("Sprites/Story20261026/village_ruins").Length,Is.EqualTo(4));
            var old=JsonUtility.FromJson<SaveFileV1>("{\"version\":1,\"currentDay\":8}");
            Assert.That(old.storyTutorialEnabled,Is.False);Assert.That(old.storyVillageRuined,Is.False);
            var save=new SaveFileV1 {storyTutorialEnabled=true,storyTutorialStage=3,storyVillageRuined=true};
            var restored=JsonUtility.FromJson<SaveFileV1>(JsonUtility.ToJson(save));
            Assert.That(restored.storyTutorialStage,Is.EqualTo(3));Assert.That(restored.storyVillageRuined,Is.True);
        }
    }
}
