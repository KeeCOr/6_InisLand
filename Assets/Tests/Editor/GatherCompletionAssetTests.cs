using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public sealed class GatherCompletionAssetTests
    {
        [TestCase("wood")][TestCase("stone")][TestCase("plant")]
        public void MaterialFramesShareCanvasPivotAndScale(string material)
        {
            foreach(string phase in new[]{"start","loop","end"})
            {
                var sprite=Resources.Load<Sprite>("Sprites/FX/Gather20261103/"+material+"_"+phase);
                Assert.That(sprite,Is.Not.Null);
                Assert.That(sprite.rect.size,Is.EqualTo(new Vector2(256,256)));
                Assert.That(sprite.pivot.y/sprite.rect.height,Is.EqualTo(.3f).Within(.001f));
                Assert.That(sprite.bounds.size.x,Is.EqualTo(1.35f).Within(.001f));
                Assert.That(sprite.texture.filterMode,Is.EqualTo(FilterMode.Bilinear));
            }
        }
    }
}
