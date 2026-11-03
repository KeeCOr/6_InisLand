using UnityEngine;
namespace IL6
{
    public enum SunsetWarningStage { None, Approaching, Imminent }
    public sealed class SunsetWarningState
    {
        public const float NoticeDuration=4f;
        private DayNightController _cycle;
        private int _day=-1;
        private bool _announced,_urgentAnnounced;
        public SunsetWarningStage Stage {get;private set;}
        public float Age {get;private set;}
        public int SecondsRemaining {get;private set;}
        public int AlertCount {get;private set;}
        public bool Visible=>Stage==SunsetWarningStage.Imminent || Stage==SunsetWarningStage.Approaching && Age<NoticeDuration;
        public bool Advance(DayNightController cycle,float dt,bool blocked)
        {
            if(cycle==null){Stage=SunsetWarningStage.None;return false;}
            if(_cycle!=cycle || _day!=cycle.Day)
            {_cycle=cycle;_day=cycle.Day;_announced=false;_urgentAnnounced=false;Age=0;Stage=SunsetWarningStage.None;AlertCount=0;}
            if(cycle.Phase!=Phase.Day){Stage=SunsetWarningStage.None;return false;}
            float remaining=Mathf.Max(0,cycle.PhaseDurationSec-cycle.ElapsedInPhase);
            SecondsRemaining=Mathf.CeilToInt(remaining);
            if(blocked)return false;
            Age+=Mathf.Max(0,dt);
            float warning=Mathf.Min(15f,cycle.PhaseDurationSec*.5f),urgent=Mathf.Min(5f,warning/3f);
            if(remaining<=urgent && !_urgentAnnounced)
            {_urgentAnnounced=true;_announced=true;Stage=SunsetWarningStage.Imminent;Age=0;AlertCount++;return true;}
            if(remaining<=warning && !_announced)
            {_announced=true;Stage=SunsetWarningStage.Approaching;Age=0;AlertCount++;return true;}
            return false;
        }
    }
}
