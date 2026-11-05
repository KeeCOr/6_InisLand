using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace IL6.Tests
{
    public sealed class AdventureMenuLayoutTests
    {
        [TestCase(960,540)] [TestCase(1280,720)] [TestCase(1920,1080)] [TestCase(2400,1080)]
        public void ControlsAndAttributionStayInsideMenuWithoutOverlapping(int width,int height)
        {
            foreach(float density in new[]{.8f,1f})
            {
                float scale=Mathf.Min(width/1280f,height/720f)*density;
                var panel=AdventureMenuLayout.Panel(width/scale,height/scale);
                Assert.That(panel.xMin,Is.GreaterThanOrEqualTo(14));Assert.That(panel.yMin,Is.GreaterThanOrEqualTo(14));
                Assert.That(panel.xMax,Is.LessThanOrEqualTo(width/scale-14));Assert.That(panel.yMax,Is.LessThanOrEqualTo(height/scale-14));
                var parts=new List<Rect>{AdventureMenuLayout.Title(panel),AdventureMenuLayout.Continue(panel),AdventureMenuLayout.Save(panel),AdventureMenuLayout.Research(panel),AdventureMenuLayout.SettingsTitle(panel),AdventureMenuLayout.Credits(panel),AdventureMenuLayout.Restart(panel)};
                for(int i=0;i<4;i++)
                {
                    var row=AdventureMenuLayout.Setting(panel,i);parts.Add(row);
                    var children=new[]{AdventureMenuLayout.SettingLabel(row),AdventureMenuLayout.SettingTrack(row),AdventureMenuLayout.SettingValue(row)};
                    for(int j=0;j<children.Length;j++)
                    {
                        Assert.That(row.Contains(children[j].min),Is.True);Assert.That(children[j].yMax,Is.LessThanOrEqualTo(row.yMax));Assert.That(children[j].xMax,Is.LessThanOrEqualTo(row.xMax));
                        for(int k=j+1;k<children.Length;k++)Assert.That(children[j].Overlaps(children[k]),Is.False);
                    }
                }
                for(int i=0;i<parts.Count;i++)
                {
                    Assert.That(panel.Contains(parts[i].min),Is.True);Assert.That(parts[i].xMax,Is.LessThanOrEqualTo(panel.xMax-16));Assert.That(parts[i].yMax,Is.LessThanOrEqualTo(panel.yMax-16));
                    for(int j=i+1;j<parts.Count;j++)Assert.That(parts[i].Overlaps(parts[j]),Is.False,$"Menu parts {i}/{j}");
                }
                foreach(var button in new[]{AdventureMenuLayout.Continue(panel),AdventureMenuLayout.Save(panel),AdventureMenuLayout.Research(panel),AdventureMenuLayout.Restart(panel)})Assert.That(button.height,Is.GreaterThanOrEqualTo(44));
            }
        }
        [Test] public void KoreanCopyAndCompleteMusicCreditFitAtAuthoredFontSizes()
        {
            var panel=AdventureMenuLayout.Panel(1280,720);
            var style=new GUIStyle{font=UiTheme.HudFont,fontSize=16,fontStyle=FontStyle.Bold};
            Assert.That(style.CalcSize(new GUIContent("연구 목록 · 건물별 업그레이드")).x,Is.LessThan(AdventureMenuLayout.Research(panel).width-4));
            style.fontSize=14;
            foreach(string label in new[]{"화면 UI 크기","월드 확대","배경음","효과음"})Assert.That(style.CalcSize(new GUIContent(label)).x,Is.LessThan(136-4));
            style.fontSize=12;style.fontStyle=FontStyle.Normal;style.wordWrap=true;
            var credits=AdventureMenuLayout.Credits(panel);
            Assert.That(style.CalcHeight(new GUIContent(MusicCredits.Attribution),credits.width-20),Is.LessThanOrEqualTo(credits.height-20));
        }
    }
}
