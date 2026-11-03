using NUnit.Framework;
using UnityEngine;

namespace IL6.Tests
{
    public class CompanionIdleFacingTests
    {
        [TestCase("Hunter")][TestCase("Warrior")][TestCase("Farmer")][TestCase("Cook")]
        [TestCase("Healer")][TestCase("Aunt")][TestCase("Uncle")][TestCase("Child")]
        public void CompanionStartsFrontAndReturnsFrontAfterMovement(string role)
        {
            var go=new GameObject("idle-facing-test");
            try
            {
                var renderer=go.AddComponent<SpriteRenderer>();
                var visual=go.AddComponent<DirectionalCharacterVisual>();visual.Configure(role);
                var front=DirectionalCharacterVisual.DirectionSprite(visual.CharacterKey,CharacterFacing.Front);
                Assert.That(renderer.sprite,Is.SameAs(front),"Spawned companions face the camera");
                visual.AnimateMovement(Vector2.left,.2f);
                Assert.That(visual.Facing,Is.EqualTo(CharacterFacing.Left));Assert.That(renderer.flipX,Is.True);
                visual.AnimateMovement(Vector2.zero,.02f);
                Assert.That(visual.Facing,Is.EqualTo(CharacterFacing.Front));Assert.That(renderer.sprite,Is.SameAs(front));Assert.That(renderer.flipX,Is.False);
                visual.AnimateMovement(Vector2.up,.2f);
                Assert.That(visual.Facing,Is.EqualTo(CharacterFacing.Back));
                visual.AnimateMovement(Vector2.zero,.02f);
                Assert.That(renderer.sprite,Is.SameAs(front));
            }
            finally {Object.DestroyImmediate(go);}
        }
    }
}
