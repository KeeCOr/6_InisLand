using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public sealed class GeneratedUiSkinTests
    {
        [TestCase("buttons")]
        [TestCase("panel")]
        [TestCase("pill")]
        public void RuntimeUiUsesGeneratedRasterSkin(string name)
        {
            var texture=Resources.Load<Texture2D>("UI/Generated20261014/"+name);
            Assert.That(texture,Is.Not.Null);
            Assert.That(texture.width,Is.GreaterThanOrEqualTo(512));
            Assert.That(texture.filterMode,Is.EqualTo(FilterMode.Bilinear));
        }
        [TestCase(false)]
        [TestCase(true)]
        public void NineSliceSourceBoundsAreValid(bool pill)
        {
            var uv=GeneratedUiSkin.SourceBounds(pill);
            Assert.That(uv.width,Is.GreaterThan(.5f).And.LessThanOrEqualTo(1f));
            Assert.That(uv.height,Is.GreaterThan(.1f).And.LessThanOrEqualTo(1f));
            Assert.That(uv.xMin,Is.GreaterThanOrEqualTo(0));
            Assert.That(uv.yMin,Is.GreaterThanOrEqualTo(0));
        }
    }
}
