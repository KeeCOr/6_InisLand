using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public class DirectionalCharacterTests
    {
        [Test] public void EveryRoleAndWeaponHasSeparateFrontAndBackArt()
        {
            foreach(string key in new[]{"hero_axe","hero_bow","hero_hammer","hunter","warrior","farmer","cook","healer","aunt","uncle","child"})
            {
                var front=DirectionalCharacterVisual.DirectionSprite(key,CharacterFacing.Front);
                var back=DirectionalCharacterVisual.DirectionSprite(key,CharacterFacing.Back);
                Assert.That(front,Is.Not.Null,key);Assert.That(back,Is.Not.Null,key);
                Assert.That(front,Is.Not.SameAs(back),key);
                Assert.That(front.pivot.y/front.rect.height,Is.InRange(0f,.25f),key+" feet pivot");
                Assert.That(front.bounds.size.y,Is.EqualTo(back.bounds.size.y).Within(.02f),key);
            }
        }
        [Test] public void MovementSelectsFourDirectionsAndRetainsIdleAndDiagonalAxis()
        {
            Assert.That(DirectionalCharacterVisual.ResolveFacing(Vector2.up,CharacterFacing.Right),Is.EqualTo(CharacterFacing.Back));
            Assert.That(DirectionalCharacterVisual.ResolveFacing(Vector2.down,CharacterFacing.Back),Is.EqualTo(CharacterFacing.Front));
            Assert.That(DirectionalCharacterVisual.ResolveFacing(Vector2.left,CharacterFacing.Front),Is.EqualTo(CharacterFacing.Left));
            Assert.That(DirectionalCharacterVisual.ResolveFacing(Vector2.right,CharacterFacing.Left),Is.EqualTo(CharacterFacing.Right));
            Assert.That(DirectionalCharacterVisual.ResolveFacing(Vector2.zero,CharacterFacing.Back),Is.EqualTo(CharacterFacing.Back));
            Assert.That(DirectionalCharacterVisual.ResolveFacing(Vector2.one,CharacterFacing.Back),Is.EqualTo(CharacterFacing.Back));
        }
        [Test] public void LocalizedRolesKeepTheirOwnDirectionSheet()
        {
            Assert.That(DirectionalCharacterVisual.KeyForRole("요리사"),Is.EqualTo("cook"));
            Assert.That(DirectionalCharacterVisual.KeyForRole("전사"),Is.EqualTo("warrior"));
            Assert.That(DirectionalCharacterVisual.KeyForRole("Child"),Is.EqualTo("child"));
        }
    }
}
