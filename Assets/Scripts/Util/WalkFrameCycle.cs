using UnityEngine;
namespace IL6
{
    public sealed class WalkFrameCycle
    {
        private float _distance,_elapsed;
        public bool Moving { get; private set; }
        public ActionPhase Phase { get; private set; }=ActionPhase.End;
        public int Frame { get; private set; }
        public void Tick(Vector2 velocity,float deltaTime)
        {
            if(deltaTime<=0) return;
            bool moving=velocity.sqrMagnitude>.0025f;
            if(!moving)
            { Moving=false;Phase=ActionPhase.End;Frame=0;_distance=0;_elapsed=0;return; }
            if(!Moving) {Moving=true;Phase=ActionPhase.Start;_distance=0;_elapsed=0;}
            _elapsed+=deltaTime;_distance+=velocity.magnitude*deltaTime;
            if(_elapsed>=.1f) Phase=ActionPhase.Loop;
            Frame=Phase==ActionPhase.Start?0:Mathf.FloorToInt(_distance/.75f)%2;
        }
    }
}
