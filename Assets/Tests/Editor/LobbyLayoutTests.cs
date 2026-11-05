using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public class LobbyLayoutTests
    {
        [TestCase(1280,720)] [TestCase(1600,900)] [TestCase(1920,1080)] [TestCase(2400,1080)]
        public void ThreeSlotCardsAndButtonsFitWithoutOverlap(float width,float height)
        {
            var panel=LobbyLayout.Panel(width,height);
            Assert.That(panel.xMin,Is.GreaterThanOrEqualTo(0));Assert.That(panel.yMin,Is.GreaterThan(132));
            Assert.That(panel.xMax,Is.LessThanOrEqualTo(width));Assert.That(panel.yMax,Is.LessThan(height));
            for(int i=0;i<3;i++)
            {
                var slot=LobbyLayout.Slot(panel,i);var next=LobbyLayout.Continue(slot);var fresh=LobbyLayout.New(slot);
                Assert.That(panel.Contains(slot.min),Is.True);Assert.That(panel.Contains(slot.max-Vector2.one),Is.True);
                Assert.That(next.Overlaps(fresh),Is.False);Assert.That(next.height,Is.GreaterThanOrEqualTo(44));
                for(int j=i+1;j<3;j++)Assert.That(slot.Overlaps(LobbyLayout.Slot(panel,j)),Is.False);
                var style=new GUIStyle{font=UiTheme.HudFont,fontSize=18};
                foreach(var label in new[]{"이어서 하기","새로 하기","저장 슬롯 3"})Assert.That(style.CalcSize(new GUIContent(label)).x,Is.LessThan(next.width-24));
            }
        }
    }
}
