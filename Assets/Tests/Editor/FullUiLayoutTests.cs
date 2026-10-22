using NUnit.Framework;
using UnityEngine;
using System.Reflection;

namespace IL6.Tests
{
    public sealed class FullUiLayoutTests
    {
        [TestCase(960,540)]
        [TestCase(1280,720)]
        [TestCase(1920,1080)]
        [TestCase(2400,1080)]
        public void WorkLauncherLeavesMovementAndCombatControlsClear(int width,int height)
        {
            float scale=Mathf.Min(width/1280f,height/720f);
            float w=width/scale,h=height/scale;
            var launcher=HudLayout.WorkLauncher(w,h);
            Assert.That(launcher.Overlaps(new Rect(16,h-160,144,144)),Is.False);
            Assert.That(launcher.Overlaps(new Rect(w-336,h-198,320,182)),Is.False);
            Assert.That(launcher.yMax,Is.LessThan(h));
            Assert.That(launcher.xMin,Is.GreaterThan(0));
        }
        [Test]
        public void ClockDoesNotCoverMenu()
        {
            var property=typeof(MobileHudLayout).GetProperty("Clock");
            Assert.That(property,Is.Not.Null);
            var r=(Rect)property.GetValue(null);
            Assert.That(r.Overlaps(MobileHudLayout.Menu),Is.False);
            Assert.That(r.Overlaps(MobileHudLayout.Status),Is.False);
        }
        [Test]
        public void RecruitDialogLeavesMovementAndActionsAccessible()
        {
            var r=HudLayout.RecruitDialog();
            Assert.That(r.Overlaps(MobileHudLayout.Movement),Is.False);
            Assert.That(r.Overlaps(MobileHudLayout.Actions),Is.False);
            Assert.That(r.height,Is.GreaterThanOrEqualTo(180));
        }
        [Test]
        public void BuildCatalogCardsStayInsideTheModalAndDoNotOverlap()
        {
            var method=typeof(HudLayout).GetMethod("CatalogCard");
            Assert.That(method,Is.Not.Null);
            var modal=HudLayout.CenterModal(760,450);
            for(int i=0;i<19;i++)
            {
                var r=(Rect)method.Invoke(null,new object[]{i});
                Assert.That(r.xMin,Is.GreaterThanOrEqualTo(modal.xMin));
                Assert.That(r.xMax,Is.LessThanOrEqualTo(modal.xMax));
                Assert.That(r.yMin,Is.GreaterThanOrEqualTo(modal.y+60));
                Assert.That(r.yMax,Is.LessThanOrEqualTo(modal.yMax-60));
                Assert.That(r.height,Is.GreaterThanOrEqualTo(60));
                for(int j=i+1;j<19;j++) Assert.That(r.Overlaps((Rect)method.Invoke(null,new object[]{j})),Is.False);
            }
        }

        [Test]
        public void EveryConstructionChoiceHasARealSpriteAndKoreanName()
        {
            var name=typeof(SimpleHud).GetMethod("BuildingName",BindingFlags.Static|BindingFlags.NonPublic);
            foreach(BuildingKind kind in System.Enum.GetValues(typeof(BuildingKind)))
            {
                Assert.That(SpriteBank.BuildingByKind(kind),Is.Not.Null,kind.ToString());
                Assert.That((string)name.Invoke(null,new object[]{kind}),Does.Match("[가-힣]"));
            }
        }

        [Test]
        public void SecondaryTextKeepsContrastOverBrightSnowBehindPanel()
        {
            var bg=Color.Lerp(Color.white,UiTheme.PanelBg,UiTheme.PanelBg.a);
            float Linear(float c)=>c<=.04045f?c/12.92f:Mathf.Pow((c+.055f)/1.055f,2.4f);
            float Luma(Color c)=>.2126f*Linear(c.r)+.7152f*Linear(c.g)+.0722f*Linear(c.b);
            Assert.That((Luma(UiTheme.TextSubtle)+.05f)/(Luma(bg)+.05f),Is.GreaterThanOrEqualTo(4.5f));
        }
    }
}
