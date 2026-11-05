using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public class ThreatNavigationTests
    {
        [TestCase(1280)] [TestCase(1920)] public void TwoNoticeRowsClearClockNavigationQuestsAndMenu(float width)
        {
            var home=HomeNavigation.Bounds(width);var quest=QuestTrackerLayout.Panel(width,false);
            for(int i=0;i<2;i++)
            {
                var row=ThreatNavigation.Bounds(width,i);Assert.That(row.Overlaps(home),Is.False);Assert.That(row.Overlaps(quest),Is.False);
                Assert.That(row.Overlaps(new Rect(width/2-94,16,188,44)),Is.False);
                Assert.That(row.xMax,Is.LessThan(width-70));
                for(int j=i+1;j<2;j++)Assert.That(row.Overlaps(ThreatNavigation.Bounds(width,j)),Is.False);
            }
        }
        [Test] public void RasterSymbolsAndCountLabelsFitCompactNotice()
        {
            Assert.That(NavigationIconSkin.Arrow,Is.Not.Null);Assert.That(NavigationIconSkin.Home,Is.Not.Null);Assert.That(NavigationIconSkin.Damage,Is.Not.Null);
            var style=new GUIStyle{font=UiTheme.HudFont,fontSize=12,fontStyle=FontStyle.Bold};
            foreach(bool ally in new[]{false,true})Assert.That(style.CalcSize(new GUIContent(new ThreatNotice(Vector3.zero,42,ally).Label)).x,Is.LessThanOrEqualTo(96));
        }
    }
}
