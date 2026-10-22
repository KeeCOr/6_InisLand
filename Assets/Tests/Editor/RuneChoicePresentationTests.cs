using System;
using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public class RuneChoicePresentationTests
    {
        [Test] public void AllChoicesHaveCompactNonemptyEffectsAndActiveRunesHaveIcons()
        {
            int artCount=0;
            foreach(RuneKind kind in Enum.GetValues(typeof(RuneKind))) {
                Assert.That(RuneChoicePresentation.Name(kind),Is.Not.Empty);
                for(int level=1;level<=3;level++) {
                    var stats=RuneChoicePresentation.Stats(kind,level);Assert.That(stats.Count,Is.InRange(1,4));
                    foreach(var token in stats) {Assert.That(token.Value.Length,Is.LessThanOrEqualTo(8));Assert.That(token.Hint,Is.Not.Empty);}
                }
                string path=RuneChoicePresentation.IconPath(kind);
                if(path!=null) {artCount++;Assert.That(Resources.Load<Texture2D>(path),Is.Not.Null,path);}
            }
            Assert.That(artCount,Is.EqualTo(12));
        }
        [Test] public void CompactValuesMatchRuntimePoisonLightningAndHealingAtEachStage()
        {
            foreach(var kind in new[]{RuneKind.PoisonBlade,RuneKind.LightningStrike,RuneKind.Vampirism}) {
                var go=new GameObject("Rune compact preview test");var progression=go.AddComponent<PlayerProgression>();
                try {for(int level=1;level<=3;level++) {
                    progression.ApplyRune(kind);var tokens=RuneChoicePresentation.Stats(kind,level);
                    if(kind==RuneKind.PoisonBlade) {
                        Assert.That(tokens[0].Value,Is.EqualTo(progression.PoisonDpsCalc+"/초"));
                        Assert.That(tokens[1].Value,Is.EqualTo(progression.PoisonDurationCalc+"초"));
                    } else if(kind==RuneKind.LightningStrike) {
                        Assert.That(tokens[0].Value,Is.EqualTo(Mathf.RoundToInt(progression.LightningChance*100)+"%"));
                        Assert.That(tokens[1].Value,Is.EqualTo(progression.LightningDmg.ToString()));
                        Assert.That(tokens[2].Value,Is.EqualTo("×"+progression.LightningJumps));
                    } else Assert.That(tokens[0].Value,Is.EqualTo("+"+progression.VampirismHeal));
                }} finally {UnityEngine.Object.DestroyImmediate(go);}
            }
        }
        [TestCase(1280,720)][TestCase(1920,720)][TestCase(1280,960)]
        public void CardsIconsMetricsAndDetailsFitWithoutOverlap(int width,int height)
        {
            var layout=new RuneChoiceLayout(width,height);
            Assert.That(layout.Panel.xMin,Is.GreaterThanOrEqualTo(0));Assert.That(layout.Panel.yMin,Is.GreaterThanOrEqualTo(0));
            for(int i=0;i<3;i++) {
                var card=layout.Card(i);Assert.That(layout.Panel.Contains(card.min),Is.True);Assert.That(layout.Panel.Contains(card.max),Is.True);
                Assert.That(card.Overlaps(layout.Details),Is.False);
                Rect[] regions={RuneChoiceLayout.Title(card),RuneChoiceLayout.Info(card),RuneChoiceLayout.Icon(card),RuneChoiceLayout.Trigger(card),RuneChoiceLayout.Progress(card),RuneChoiceLayout.Stat(card,0),RuneChoiceLayout.Stat(card,1),RuneChoiceLayout.Stat(card,2),RuneChoiceLayout.Stat(card,3)};
                for(int a=0;a<regions.Length;a++) {
                    Assert.That(card.Contains(regions[a].min),Is.True);Assert.That(card.Contains(regions[a].max),Is.True);
                    for(int b=a+1;b<regions.Length;b++) Assert.That(regions[a].Overlaps(regions[b]),Is.False,$"Regions {a}/{b}");
                }
            }
        }
        [Test] public void StatAtlasAndMasterSupplyUseAllRequiredIcons()
        {
            Assert.That(Resources.Load<Texture2D>("UI/Generated20261018/rune_stats"),Is.Not.Null);
            var gift=RuneChoicePresentation.Stats(RuneKind.ResourceGift,3);
            Assert.That(gift.Count,Is.EqualTo(4));Assert.That(gift[3].Icon,Is.EqualTo(RuneStatIcon.Frostbloom));
            Assert.That(gift[3].Value,Is.EqualTo("+1"));
        }
        [Test] public void TitlesProgressAndEffectGroupsShareTheCardCenter()
        {
            var layout=new RuneChoiceLayout(1280,720);
            for(int i=0;i<3;i++) {
                var card=layout.Card(i);
                Assert.That(RuneChoiceLayout.Title(card).center.x,Is.EqualTo(card.center.x));
                Assert.That(RuneChoiceLayout.Progress(card).center.x,Is.EqualTo(card.center.x));
                for(int count=1;count<=4;count++) {
                    var first=RuneChoiceLayout.Stat(card,0,count);
                    var last=RuneChoiceLayout.Stat(card,count-1,count);
                    Assert.That(first.center.x,Is.EqualTo(card.center.x));
                    Assert.That((first.yMin+last.yMax)/2,Is.EqualTo(card.y+279));
                    Assert.That(last.yMax,Is.LessThanOrEqualTo(card.yMax-20));
                }
            }
        }
    }
}
