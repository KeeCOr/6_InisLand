using NUnit.Framework;
using UnityEngine;

namespace IL6.Tests
{
    public sealed class CharacterProjectileOriginTests
    {
        [Test]
        public void MuzzleFollowsWeaponSideAndRespectsPivotScaleAndPosition()
        {
            var actor = new GameObject("Origin test");
            var texture = new Texture2D(100, 100);
            var sprite = Sprite.Create(texture, new Rect(0, 0, 100, 100), new Vector2(.5f, 0), 100);
            actor.AddComponent<SpriteRenderer>().sprite = sprite;
            actor.transform.position = new Vector3(7, 9, 0);
            actor.transform.localScale = new Vector3(2, 3, 1);
            try
            {
                var right = CharacterProjectileOrigin.Resolve(actor.transform, actor.transform.position + Vector3.right, null);
                var left = CharacterProjectileOrigin.Resolve(actor.transform, actor.transform.position + Vector3.left, null);
                var front = CharacterProjectileOrigin.Resolve(actor.transform, actor.transform.position + Vector3.down, null);
                var back = CharacterProjectileOrigin.Resolve(actor.transform, actor.transform.position + Vector3.up, null);
                Assert.That(right.x, Is.EqualTo(7.68f).Within(.001f));
                Assert.That(left.x, Is.EqualTo(6.32f).Within(.001f));
                Assert.That(right.y, Is.EqualTo(10.68f).Within(.001f));
                Assert.That(front.y, Is.EqualTo(10.56f).Within(.001f));
                Assert.That(back.y, Is.EqualTo(11.1f).Within(.001f));
                Assert.That(front.x, Is.EqualTo(7.02f).Within(.001f));
            }
            finally
            {
                Object.DestroyImmediate(actor);
                Object.DestroyImmediate(sprite);
                Object.DestroyImmediate(texture);
            }
        }
    }
}
