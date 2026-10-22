using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace IL6.Tests
{
    public class MusicTransitionTests
    {
        [UnityTest] public IEnumerator PhaseChangeCrossfadesWithoutRestartingTheSamePhase()
        {
            Music.Stop();Music.PlayForPhase(Phase.Day);yield return null;
            var runner=GameObject.Find("__MusicRunner");var sources=runner.GetComponents<AudioSource>();
            Assert.That(sources.Length,Is.EqualTo(2),"Phase changes need overlapping audio sources");
            Music.PlayForPhase(Phase.Night);
            yield return new WaitForSecondsRealtime(.2f);
            Assert.That(sources[0].volume,Is.GreaterThan(0));Assert.That(sources[1].volume,Is.GreaterThan(0));
            var outgoing=sources[0].clip;var incoming=sources[1].clip;
            Music.PlayForPhase(Phase.Night);
            Assert.That(sources[0].clip,Is.SameAs(outgoing));Assert.That(sources[1].clip,Is.SameAs(incoming));
            yield return new WaitForSecondsRealtime(1f);
            Assert.That(sources[0].volume,Is.Zero);Assert.That(sources[1].volume,Is.EqualTo(Music.Volume*.5f).Within(.001f));
            Assert.That(sources[0].loop && sources[1].loop,Is.True);
            Music.Stop();Assert.That(sources[0].volume+sources[1].volume,Is.Zero);
            Music.PlayForPhase(Phase.Night);Assert.That(sources[0].clip,Is.Not.Null);
            Music.Stop();
        }
    }
}
