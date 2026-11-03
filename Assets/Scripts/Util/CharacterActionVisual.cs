using System.Collections.Generic;
using UnityEngine;

namespace IL6
{
    [DefaultExecutionOrder(170)]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class CharacterActionVisual : MonoBehaviour
    {
        private static readonly Dictionary<string,Sprite[]> Sheets=new Dictionary<string,Sprite[]>();
        private SpriteRenderer _renderer;
        private DirectionalCharacterVisual _appearance;
        private PlayerActionState _actions;
        private PlayerAttackController _weapon;
        private PlayerController _player;
        private GatherController _gather;
        private bool _playing,_useActionState,_working;
        private float _age,_workStarted;
        private CharacterFacing _facing;
        public bool IsPlaying=>_playing || _working;
        public ActionPhase Phase {get;private set;}
        public CharacterFacing Facing=>_facing;

        private void Awake()
        {
            _renderer=GetComponent<SpriteRenderer>();_appearance=GetComponent<DirectionalCharacterVisual>();
            _actions=GetComponent<PlayerActionState>();_player=GetComponent<PlayerController>();
            _weapon=GetComponent<PlayerAttackController>();_gather=GetComponent<GatherController>();
        }
        public static Sprite Frame(string key,CharacterFacing facing,ActionPhase phase)
        {
            if(!Sheets.TryGetValue(key,out var sprites) || sprites.Length==0)
            {sprites=Resources.LoadAll<Sprite>("Sprites/Characters/Combat20261103/"+key);Sheets[key]=sprites;}
            string direction=facing==CharacterFacing.Front?"front":facing==CharacterFacing.Back?"back":"side";
            string name="attack_"+direction+"_"+phase;
            foreach(var sprite in sprites)if(sprite.name==name)return sprite;
            return null;
        }
        public void PlayAttack(Vector2 direction)
        {
            _facing=DirectionalCharacterVisual.ResolveFacing(direction,_appearance!=null?_appearance.Facing:CharacterFacing.Right);
            if(_actions==null)_actions=GetComponent<PlayerActionState>();
            _useActionState=_actions!=null && _actions.State==PlayerAction.Attack;
            _playing=true;_age=0;Phase=ActionPhase.Start;
        }
        private string CurrentKey()
        {
            if(_player==null)return _appearance!=null?_appearance.CharacterKey:null;
            if(_weapon==null)_weapon=GetComponent<PlayerAttackController>();
            return _weapon!=null && _weapon.IsConstructionHammer?"hero_hammer":_weapon!=null && _weapon.WeaponIndex==1?"hero_bow":"hero_axe";
        }
        private void LateUpdate()
        {
            if(Time.timeScale<=0 || InputReader.HudBlocked)return;
            if((_player!=null && _player.IsDead) || (GetComponent<Companion>() is Companion ally && ally.IsDead))
            {_playing=false;_working=false;return;}
            if(_actions==null)_actions=GetComponent<PlayerActionState>();
            bool work=_actions!=null && (_actions.State==PlayerAction.Logging || _actions.State==PlayerAction.Gather);
            string key=CurrentKey();
            if(work)
            {
                if(!_working){_workStarted=Time.time;_working=true;}
                _playing=false;
                if(_gather==null)_gather=GetComponent<GatherController>();
                if(_actions.State==PlayerAction.Gather && _gather!=null && _gather.ActiveTarget!=null && !_gather.ActiveTarget.IsRock)
                {_working=false;return;}
                if(_gather!=null && _gather.ActiveTarget!=null)
                    _facing=DirectionalCharacterVisual.ResolveFacing(_gather.ActiveTarget.transform.position-transform.position,_facing);
                Phase=_actions.Phase;
                if(Phase==ActionPhase.Loop)
                {
                    float progress=_actions.State==PlayerAction.Logging?_actions.PhaseProgress:
                        Mathf.Repeat(Time.time-_workStarted,1.5f)/1.5f;
                    Phase=progress<.4f?ActionPhase.Start:progress<.65f?ActionPhase.Loop:ActionPhase.End;
                }
                key=_actions.State==PlayerAction.Logging?"hero_axe":"hero_hammer";
            }
            else
            {
                _working=false;if(!_playing)return;
                _age+=Time.deltaTime;
                if(_useActionState)
                {
                    if(_actions.State!=PlayerAction.Attack){_playing=false;return;}
                    Phase=_actions.Phase;
                }
                else
                {
                    if(_age>=.4f){_playing=false;return;}
                    Phase=_age<.1f?ActionPhase.Start:_age<.22f?ActionPhase.Loop:ActionPhase.End;
                }
            }
            if(string.IsNullOrEmpty(key))return;
            var frame=Frame(key,_facing,Phase);
            if(frame!=null){_renderer.sprite=frame;_renderer.flipX=_facing==CharacterFacing.Left;}
        }
    }
}
