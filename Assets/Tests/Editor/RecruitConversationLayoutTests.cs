using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public class RecruitConversationLayoutTests
    {
        [TestCase(1280,720)][TestCase(1600,720)][TestCase(1280,960)][TestCase(960,540)]
        public void DialogueAndChoicesFitOnDesktopAndMobile(int width,int height)
        {
            var r=RecruitConversationLayout.ForSize(width,height);
            var screen=new Rect(0,0,width,height);
            foreach(var rect in new[]{r.Character,r.Bubble,r.Name,r.Role,r.Dialogue,r.Details,r.Accept,r.Decline})
            { Assert.That(screen.Contains(rect.min),Is.True);Assert.That(screen.Contains(rect.max),Is.True); }
            Assert.That(r.Accept.Overlaps(r.Decline),Is.False);
            Assert.That(r.Dialogue.Overlaps(r.Details),Is.False);
            Assert.That(r.Accept.height,Is.GreaterThanOrEqualTo(44));
            Assert.That(r.Bubble.Contains(r.Accept.center),Is.True);
            Assert.That(r.Bubble.yMin,Is.GreaterThanOrEqualTo(height*.4f));
            Assert.That(r.Name.Overlaps(r.Dialogue),Is.False);
            Assert.That(r.Name.Overlaps(r.Role),Is.False);
            Assert.That(r.Role.Overlaps(r.Dialogue),Is.False);
            Assert.That(r.Details.Overlaps(r.Accept),Is.False);
            Assert.That(r.Character.xMax,Is.LessThan(r.Name.xMin));
            Assert.That(r.Character.Overlaps(r.Bubble),Is.False);
            Assert.That(r.Accept.width,Is.GreaterThan(r.Decline.width));
            foreach(var rect in new[]{r.Name,r.Role,r.Dialogue,r.Details,r.Accept,r.Decline})
            { Assert.That(r.Bubble.Contains(rect.min),Is.True);Assert.That(r.Bubble.Contains(rect.max),Is.True); }
            Assert.That(r.Accept.xMin-r.Bubble.xMin,Is.GreaterThanOrEqualTo(r.Bubble.width*.15f));
            Assert.That(r.Bubble.xMax-r.Decline.xMax,Is.GreaterThanOrEqualTo(r.Bubble.width*.15f));
        }
    }
}
