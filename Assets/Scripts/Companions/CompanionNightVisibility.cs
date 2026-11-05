using System;
using System.Collections.Generic;
using UnityEngine;

namespace IL6
{
    [DefaultExecutionOrder(300)]
    public sealed class CompanionNightVisibility : MonoBehaviour
    {
        private readonly Dictionary<Renderer,bool> _previous=new Dictionary<Renderer,bool>();
        private Action _unsubNight,_unsubDawn;
        private bool _hidden;

        private void OnEnable()
        {
            _unsubNight=EventBus.Instance.Subscribe<NightStartedPayload>(_=>SetHidden(true));
            _unsubDawn=EventBus.Instance.Subscribe<DawnStartedPayload>(_=>SetHidden(false));
            Refresh();
        }
        private void LateUpdate(){Refresh();}
        private void Refresh()
        {
            var cycle=GameSession.Instance!=null?GameSession.Instance.Cycle:null;
            SetHidden(cycle!=null?cycle.Phase==Phase.Night:_hidden);
        }
        private void SetHidden(bool hidden)
        {
            _hidden=hidden;
            if(!hidden){Restore();return;}
            foreach(var renderer in GetComponentsInChildren<Renderer>(true))Hide(renderer);
            // The ground shadow is scene-owned, rather than a child of the actor.
            var snow=GetComponent<ActorSnowPresentation>();
            if(snow!=null)Hide(snow.Shadow);
        }
        private void Hide(Renderer renderer)
        {
            if(renderer==null)return;
            if(!_previous.ContainsKey(renderer))_previous.Add(renderer,renderer.forceRenderingOff);
            // Keep enabled/AI/shelter/collision states independent from presentation.
            renderer.forceRenderingOff=true;
        }
        private void Restore()
        {
            foreach(var pair in _previous)if(pair.Key!=null)pair.Key.forceRenderingOff=pair.Value;
            _previous.Clear();
        }
        private void OnDisable(){_unsubNight?.Invoke();_unsubDawn?.Invoke();Restore();}
    }
}
