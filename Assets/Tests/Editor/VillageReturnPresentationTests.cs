using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public sealed class VillageReturnPresentationTests
    {
        [Test]
        public void SunsetNoticePrecedesIllustrationAndFadesBeforeNight()
        {
            var notice=VillageReturnPresentation.At(.5f,5);
            Assert.That(notice.NoticeAlpha,Is.EqualTo(1));Assert.That(notice.IllustrationAlpha,Is.Zero);Assert.That(notice.ReturnDue,Is.False);
            var returned=VillageReturnPresentation.At(.7f,5);Assert.That(returned.ReturnDue,Is.True);
            var art=VillageReturnPresentation.At(2,5);Assert.That(art.IllustrationAlpha,Is.EqualTo(1));Assert.That(art.NoticeAlpha,Is.Zero);
            var fade=VillageReturnPresentation.At(4.8f,5);Assert.That(fade.IllustrationAlpha,Is.InRange(.3f,.5f));
            var night=VillageReturnPresentation.At(5,5);Assert.That(night.CoverAlpha,Is.Zero);Assert.That(night.IllustrationAlpha,Is.Zero);
            var shortPhase=VillageReturnPresentation.At(.4f,1);Assert.That(shortPhase.IllustrationAlpha,Is.EqualTo(1));
        }
        [Test]
        public void DedicatedStoryArtHasWideScreenResolution()
        {
            var image=Resources.Load<Texture2D>("UI/StoryIllustrations/village_return");Assert.That(image,Is.Not.Null);
            Assert.That(image.width,Is.GreaterThanOrEqualTo(1280));
            Assert.That((float)image.width/image.height,Is.InRange(1.7f,1.85f));
            Assert.That(image.filterMode,Is.EqualTo(FilterMode.Bilinear));
        }
    }
}
