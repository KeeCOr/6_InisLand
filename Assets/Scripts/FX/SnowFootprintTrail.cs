using UnityEngine;

namespace IL6
{
    [DefaultExecutionOrder(130)]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SnowFootprintTrail : MonoBehaviour
    {
        private const float StepDistance=.42f;
        private SpriteRenderer _renderer;
        private Vector2 _last;
        private float _distance;
        private bool _ready,_left;
        private void Awake(){_renderer=GetComponent<SpriteRenderer>();}
        private void OnEnable(){_ready=false;_distance=0;}
        private void LateUpdate()
        {
            Vector2 position=transform.position;
            var field=SnowFootprintField.Instance;
            Vector2 delta=position-_last;
            if(!_ready || field==null || !field.Accepting || !_renderer.enabled || delta.sqrMagnitude>4)
            {_ready=true;_last=position;_distance=0;return;}
            float length=delta.magnitude;
            if(length<.001f){_last=position;return;}
            Vector2 direction=delta/length,current=_last;
            float needed=StepDistance-_distance;
            while(length>=needed)
            {
                current+=direction*needed;length-=needed;
                var lateral=new Vector2(direction.y,-direction.x)*(_left?-.095f:.095f);
                field.Stamp(current+lateral,direction,_left);_left=!_left;
                _distance=0;needed=StepDistance;
            }
            _distance+=length;_last=position;
        }
    }
}
