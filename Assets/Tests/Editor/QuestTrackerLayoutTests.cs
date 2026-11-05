using NUnit.Framework;
using UnityEngine;

namespace IL6.Tests
{
    public sealed class QuestTrackerLayoutTests
    {
        [TestCase(1280,720)] [TestCase(1920,1080)] [TestCase(2400,1080)]
        public void TrackerContentFitsAndLeavesMenuAndActionsClear(int width,int height)
        {
            float scale=Mathf.Min(width/1280f,height/720f),w=width/scale,h=height/scale;
            foreach(bool collapsed in new[]{true,false})
            {
                var panel=QuestTrackerLayout.Panel(w,collapsed);
                Assert.That(panel.xMax,Is.LessThanOrEqualTo(w-16));
                Assert.That(panel.Overlaps(new Rect(w-70,16,54,74)),Is.False);
                Assert.That(panel.Overlaps(new Rect(w-256,h-214,240,198)),Is.False);
                Assert.That(QuestTrackerLayout.Toggle(panel).height,Is.GreaterThanOrEqualTo(44));
                Assert.That(QuestTrackerLayout.Header(panel).Overlaps(QuestTrackerLayout.Count(panel)),Is.False);
                Assert.That(QuestTrackerLayout.Header(panel).Overlaps(QuestTrackerLayout.Toggle(panel)),Is.False);
                Assert.That(QuestTrackerLayout.Count(panel).Overlaps(QuestTrackerLayout.Toggle(panel)),Is.False);
                for(int i=0;i<3;i++)
                {
                    var row=QuestTrackerLayout.Condition(panel,collapsed,i);
                    Assert.That(row.yMax,Is.LessThan(panel.yMax-8));
                    Assert.That(row.Overlaps(QuestTrackerLayout.Title(panel)),Is.False);
                    if(!collapsed)Assert.That(row.Overlaps(QuestTrackerLayout.Detail(panel)),Is.False);
                }
                if(!collapsed)Assert.That(QuestTrackerLayout.Next(panel,1).yMax,Is.LessThan(panel.yMax-8));
            }
        }
        [Test] public void ConditionsKeepOriginalNumbersAndMarkCompletedRequirements()
        {
            var rows=QuestTrackerLayout.Rows("농장 2/2  창고 0/1  식량 1,000/35");
            Assert.That(rows.Length,Is.EqualTo(3));Assert.That(rows[0].Complete,Is.True);Assert.That(rows[1].Complete,Is.False);
            Assert.That(rows[2].Value,Is.EqualTo("1,000/35"));
            rows=QuestTrackerLayout.Rows("불 Lv.2/2  화로 1/1  대장간 0/1");
            Assert.That(rows[0].Label,Is.EqualTo("불"));Assert.That(rows[0].Value,Is.EqualTo("Lv.2/2"));Assert.That(rows[0].Complete,Is.True);
            Assert.That(QuestTrackerLayout.Rows(null),Is.Empty);
        }
        [Test] public void KoreanCopyFitsAtReadableFontSizesWithoutAutomaticShrinking()
        {
            var panel=QuestTrackerLayout.Panel(1280,false);
            var title=new GUIStyle(){font=UiTheme.HudFont,fontSize=18,padding=new RectOffset(),fontStyle=FontStyle.Bold};
            foreach(string text in new[]{"식량 확보","마을 방어","따뜻한 중심","사냥 준비","숙련된 동료","모든 초기 목표 달성","마을의 흔적 둘러보기","첫 목재 줍기","울타리 한 칸 세우기"})
                Assert.That(title.CalcSize(new GUIContent(text)).x,Is.LessThan(QuestTrackerLayout.Title(panel).width-2),text);
            var detail=new GUIStyle(title){fontSize=13,fontStyle=FontStyle.Normal,wordWrap=true};
            var ribbonTitle=new GUIStyle(title){fontSize=16};
            foreach(string text in new[]{"식량 확보","마을 방어","따뜻한 중심","사냥 준비","숙련된 동료","마을 복구 완료","마을의 흔적 둘러보기","첫 목재 줍기","울타리 한 칸 세우기"})
                Assert.That(ribbonTitle.CalcSize(new GUIContent(text)).x,Is.LessThan(QuestTrackerLayout.Header(panel).width-2),text);
            foreach(string text in new[]{"농장 2개와 식량 창고를 짓고 식량 35를 모으세요.","울타리 14개, 목공소, 망루 또는 초소를 준비하세요.","모닥불을 2레벨로 올리고 화로와 대장간을 지으세요.","사냥꾼의 집을 짓고 3일째까지 고기 20을 모으세요.","훈련소와 동료 6명을 준비하고 동료 총 레벨 10을 달성하세요."})
                Assert.That(detail.CalcHeight(new GUIContent(text),QuestTrackerLayout.Detail(panel).width-2),Is.LessThan(42),text);
        }
    }
}
