using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public class WalkFrameTests
    {
        [Test] public void StepsAlternateByDistanceAndStopReturnsToEnd()
        {
            var cycle=new WalkFrameCycle();cycle.Tick(Vector2.up*5,.02f);
            Assert.That(cycle.Phase,Is.EqualTo(ActionPhase.Start));Assert.That(cycle.Frame,Is.Zero);
            cycle.Tick(Vector2.up*5,.16f);Assert.That(cycle.Phase,Is.EqualTo(ActionPhase.Loop));Assert.That(cycle.Frame,Is.EqualTo(1));
            cycle.Tick(Vector2.up*5,.15f);Assert.That(cycle.Frame,Is.Zero);
            cycle.Tick(Vector2.zero,.02f);Assert.That(cycle.Moving,Is.False);Assert.That(cycle.Phase,Is.EqualTo(ActionPhase.End));
            cycle.Tick(Vector2.right,.02f);Assert.That(cycle.Phase,Is.EqualTo(ActionPhase.Start));
        }
        [Test] public void PauseAndSubThresholdJitterDoNotStep()
        {
            var cycle=new WalkFrameCycle();cycle.Tick(Vector2.one*.01f,.2f);Assert.That(cycle.Moving,Is.False);
            cycle.Tick(Vector2.up*5,.16f);int frame=cycle.Frame;cycle.Tick(Vector2.up*5,0);Assert.That(cycle.Frame,Is.EqualTo(frame));
        }
        [Test] public void EveryRoleWeaponAndDirectionHasTwoDistinctStepSprites()
        {
            foreach(string key in new[]{"hero_axe","hero_bow","hero_hammer","hunter","warrior","farmer","cook","healer","aunt","uncle","child"})
                foreach(var facing in new[]{CharacterFacing.Front,CharacterFacing.Back,CharacterFacing.Right})
                {
                    var a=DirectionalCharacterVisual.WalkSprite(key,facing,0);var b=DirectionalCharacterVisual.WalkSprite(key,facing,1);
                    Assert.That(a,Is.Not.Null,key+facing);Assert.That(b,Is.Not.Null,key+facing);Assert.That(a,Is.Not.SameAs(b));
                    Assert.That(a.bounds.size.y,Is.EqualTo(b.bounds.size.y).Within(.35f),key+facing+" frame scale");
                    Assert.That(a.pivot.y/a.rect.height,Is.InRange(0f,.3f));
                    Assert.That(b.pivot.y/b.rect.height,Is.InRange(0f,.3f));
                }
        }
    }
}
