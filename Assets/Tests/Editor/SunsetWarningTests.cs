using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public sealed class SunsetWarningTests
    {
        [Test]
        public void NoticesFireOnceAt15And5SecondsAndResetOnNextDay()
        {
            var b=ScriptableObject.CreateInstance<BalanceConfig>();b.DayDurationSec=40;
            try
            {
                var cycle=new DayNightController(b);var state=new SunsetWarningState();
                Assert.That(state.Advance(cycle,0,false),Is.False);
                cycle.Restore(new DayNightSnapshot{day=1,phase=Phase.Day,elapsedInPhase=25});
                Assert.That(state.Advance(cycle,0,false),Is.True);Assert.That(state.Stage,Is.EqualTo(SunsetWarningStage.Approaching));
                Assert.That(state.SecondsRemaining,Is.EqualTo(15));
                Assert.That(state.Advance(cycle,4.1f,false),Is.False);Assert.That(state.Visible,Is.False);
                cycle.Restore(new DayNightSnapshot{day=1,phase=Phase.Day,elapsedInPhase=35});
                Assert.That(state.Advance(cycle,0,false),Is.True);Assert.That(state.Stage,Is.EqualTo(SunsetWarningStage.Imminent));
                for(int i=0;i<100;i++)Assert.That(state.Advance(cycle,.01f,false),Is.False);
                Assert.That(state.AlertCount,Is.EqualTo(2));Assert.That(state.Visible,Is.True);
                cycle.Restore(new DayNightSnapshot{day=1,phase=Phase.Evening});state.Advance(cycle,0,false);
                Assert.That(state.Visible,Is.False);
                cycle.Restore(new DayNightSnapshot{day=2,phase=Phase.Day,elapsedInPhase=25});
                Assert.That(state.Advance(cycle,0,false),Is.True);Assert.That(state.AlertCount,Is.EqualTo(1));
            }
            finally {Object.DestroyImmediate(b);}
        }
        [Test]
        public void PausedWarningsDeferAndShortDaysDoNotWarnImmediately()
        {
            var b=ScriptableObject.CreateInstance<BalanceConfig>();b.DayDurationSec=4;
            try
            {
                var cycle=new DayNightController(b);var state=new SunsetWarningState();
                Assert.That(state.Advance(cycle,0,false),Is.False);
                cycle.Restore(new DayNightSnapshot{day=1,phase=Phase.Day,elapsedInPhase=2});
                Assert.That(state.Advance(cycle,10,true),Is.False);Assert.That(state.AlertCount,Is.Zero);
                Assert.That(state.Advance(cycle,0,false),Is.True);
                state.Advance(cycle,10,true);Assert.That(state.Age,Is.Zero);
                cycle.Restore(new DayNightSnapshot{day=1,phase=Phase.Day,elapsedInPhase=3.5f});
                Assert.That(state.Advance(cycle,0,false),Is.True);Assert.That(state.SecondsRemaining,Is.EqualTo(1));
                Assert.That(state.Stage,Is.EqualTo(SunsetWarningStage.Imminent));
                var lateState=new SunsetWarningState();
                Assert.That(lateState.Advance(cycle,0,false),Is.True);Assert.That(lateState.AlertCount,Is.EqualTo(1),"Late resume must not play both warning sounds together");
            }
            finally {Object.DestroyImmediate(b);}
        }
        [Test]
        public void SunsetBellIsCachedShortAndHasSmoothSilentEdges()
        {
            var clip=Sfx.SunsetBellClip();Assert.That(Sfx.SunsetBellClip(),Is.SameAs(clip));
            Assert.That(clip.length,Is.InRange(.64f,.66f));var data=new float[clip.samples];Assert.That(clip.GetData(data,0),Is.True);
            float peak=0;foreach(float x in data){Assert.That(float.IsNaN(x)||float.IsInfinity(x),Is.False);peak=Mathf.Max(peak,Mathf.Abs(x));}
            Assert.That(peak,Is.InRange(.05f,.99f));Assert.That(Mathf.Abs(data[0])+Mathf.Abs(data[data.Length-1]),Is.LessThan(.0001f));
            Assert.That(Resources.Load<Texture2D>("UI/Generated20261103/sunset_warning"),Is.Not.Null);
        }
    }
}
