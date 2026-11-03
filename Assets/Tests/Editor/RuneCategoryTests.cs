using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public sealed class RuneCategoryTests
    {
        private static string Label(RuneKind kind)
        {
            var method=typeof(RuneChoicePresentation).GetMethod("CategoryLabel");Assert.That(method,Is.Not.Null,"Level-up cards must expose the five requested categories");
            return (string)method.Invoke(null,new object[]{kind});
        }
        [Test] public void EffectsMapToTheFiveRequestedCategories()
        {
            Assert.That(Label(RuneKind.HpUp),Is.EqualTo("능력치 상승"));
            foreach(var kind in new[]{RuneKind.AllyBoost,RuneKind.SummonDog,RuneKind.SummonHawk})Assert.That(Label(kind),Is.EqualTo("동료 강화"));
            foreach(var kind in new[]{RuneKind.MeleeDamageUp,RuneKind.PoisonBlade,RuneKind.LightningStrike})Assert.That(Label(kind),Is.EqualTo("근접 무기 강화"));
            foreach(var kind in new[]{RuneKind.RangedDamageUp,RuneKind.MultiShot,RuneKind.IceArrow,RuneKind.Pierce,RuneKind.Detonator})Assert.That(Label(kind),Is.EqualTo("원거리 무기 강화"));
            Assert.That(Label(RuneKind.ResourceGift),Is.EqualTo("자원"));
        }
        [Test] public void OffersDiversifyCategoriesAndStillReachAllFive()
        {
            var go=new GameObject("Five category offers");var progression=go.AddComponent<PlayerProgression>();
            try {
                var seen=new HashSet<string>();
                for(uint seed=1;seed<=200;seed++) {
                    var offer=progression.PickThreeOffer(seed);var labels=new HashSet<string>();
                    foreach(var kind in offer){labels.Add(Label(kind));seen.Add(Label(kind));}
                    Assert.That(labels.Count,Is.EqualTo(3),"Three choices should span three available categories");
                }
                Assert.That(seen.Count,Is.EqualTo(5));
            } finally {Object.DestroyImmediate(go);}
        }
    }
}
