using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public sealed class LevelStatUpgradeTests
    {
        [Test]
        public void OffersReachAll20ChoicesWithoutDuplicatesAndExcludeMasteredStats()
        {
            var go=new GameObject("Offer pool test");var p=go.AddComponent<PlayerProgression>();
            try
            {
                var seen=new HashSet<RuneKind>();
                for(uint seed=1;seed<=300;seed++)
                {
                    var offered=p.PickThreeOffer(seed);Assert.That(offered.Count,Is.EqualTo(3));
                    Assert.That(new HashSet<RuneKind>(offered).Count,Is.EqualTo(3));
                    CollectionAssert.AreEqual(offered,p.PickThreeOffer(seed));
                    foreach(var kind in offered)seen.Add(kind);
                }
                Assert.That(seen.Count,Is.EqualTo(20));
                var mastered=new[]{RuneKind.DamageUp,RuneKind.FireRateUp,RuneKind.HpUp,RuneKind.RangeUp,RuneKind.MoveSpeedUp,RuneKind.MeleeDamageUp,RuneKind.RangedDamageUp};
                foreach(var kind in mastered)for(int i=0;i<3;i++)p.ApplyRune(kind);
                for(uint seed=1;seed<=100;seed++)foreach(var kind in p.PickThreeOffer(seed))CollectionAssert.DoesNotContain(mastered,kind);
            }
            finally {Object.DestroyImmediate(go);}
        }

        [Test]
        public void WeaponDamageUpgradesStayScopedAndStackWithCommonDamage()
        {
            var go=new GameObject("Scoped stat test");var p=go.AddComponent<PlayerProgression>();
            try
            {
                p.ApplyRune(RuneKind.DamageUp);p.ApplyRune(RuneKind.MeleeDamageUp);
                Assert.That(p.DamageMultiplierFor(false),Is.EqualTo(1.5625f));
                Assert.That(p.DamageMultiplierFor(true),Is.EqualTo(1.25f));
                p.ApplyRune(RuneKind.RangedDamageUp);
                Assert.That(p.DamageMultiplierFor(true),Is.EqualTo(1.5625f));
                p.ApplyRune(RuneKind.MeleeDamageUp);p.ApplyRune(RuneKind.MeleeDamageUp);p.ApplyRune(RuneKind.MeleeDamageUp);
                Assert.That(p.GetStacks(RuneKind.MeleeDamageUp),Is.EqualTo(3));
                Assert.That(p.DamageMultiplierFor(false),Is.EqualTo(2.75f).Within(.001f));
                Assert.That(p.DamageMultiplierFor(true),Is.EqualTo(1.5625f));
                p.ApplyRune(RuneKind.FireRateUp);p.ApplyRune(RuneKind.HpUp);p.ApplyRune(RuneKind.RangeUp);p.ApplyRune(RuneKind.MoveSpeedUp);
                Assert.That(p.CooldownMultiplier,Is.EqualTo(.85f));Assert.That(p.BonusMaxHp,Is.EqualTo(25));
                Assert.That(p.BonusRange,Is.EqualTo(1));Assert.That(p.MoveSpeedMultiplier,Is.EqualTo(1.15f));
            }
            finally {Object.DestroyImmediate(go);}
        }

        [Test]
        public void ClassificationMatchesActualWeaponEffectsAndHasDistinctArt()
        {
            Assert.That(RuneChoicePresentation.Scope(RuneKind.MeleeDamageUp),Is.EqualTo(RuneWeaponScope.Melee));
            foreach(var kind in new[]{RuneKind.RangedDamageUp,RuneKind.Pierce,RuneKind.IceArrow,RuneKind.MultiShot,RuneKind.Detonator})Assert.That(RuneChoicePresentation.Scope(kind),Is.EqualTo(RuneWeaponScope.Ranged));
            foreach(var kind in new[]{RuneKind.DamageUp,RuneKind.HpUp})
                Assert.That(RuneChoicePresentation.Scope(kind),Is.EqualTo(RuneWeaponScope.Common));
            var paths=new HashSet<string>();var colors=new HashSet<Color>();
            foreach(var kind in new[]{RuneKind.MeleeDamageUp,RuneKind.RangedDamageUp,RuneKind.HpUp})
            {
                Assert.That(paths.Add(RuneChoicePresentation.ScopeIconPath(kind)),Is.True);
                Assert.That(Resources.Load<Texture2D>(RuneChoicePresentation.ScopeIconPath(kind)),Is.Not.Null);
                Assert.That(colors.Add(RuneChoicePresentation.ScopeColor(kind)),Is.True);
                Assert.That(RuneChoicePresentation.ScopeLabel(kind),Is.Not.Empty);
            }
        }
    }
}
