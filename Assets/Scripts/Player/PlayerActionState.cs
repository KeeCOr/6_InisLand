using System;
using UnityEngine;

namespace IL6
{
    public enum PlayerAction { Idle, Move, Attack, Logging, Gather, Dead }
    public enum ActionPhase { Start, Loop, End }
    public sealed class PlayerActionState : MonoBehaviour
    {
        public PlayerAction State { get; private set; } = PlayerAction.Idle;
        public ActionPhase Phase { get; private set; } = ActionPhase.Start;
        public bool IsBusy => State == PlayerAction.Attack || State == PlayerAction.Logging || State == PlayerAction.Gather || State == PlayerAction.Dead;
        public event Action LoopImpact;
        private Animator _animator;
        private PlayerAction _next = PlayerAction.Idle;
        private float _elapsed, _duration = .2f, _loopDuration;
        private bool _impactSent;
        private RuntimeAnimatorController _controller;
        private void Awake() { _animator = GetComponent<Animator>(); PlayPhase(ActionPhase.Start); }
        public bool TryBegin(PlayerAction action, Vector2 facing, float loopDuration = 0f)
        {
            if (IsBusy || InputReader.HudBlocked || Time.timeScale <= 0f) return false;
            State = action; _next = PlayerAction.Idle; _loopDuration = loopDuration;
            var body = GetComponent<SpriteRenderer>();
            if (body != null && Mathf.Abs(facing.x) > .01f) body.flipX = facing.x < 0;
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = Vector2.zero;
            PlayPhase(ActionPhase.Start);
            return true;
        }
        public void SetMoving(bool moving)
        {
            if (IsBusy) return;
            var wanted = moving ? PlayerAction.Move : PlayerAction.Idle;
            if (State == wanted) return;
            // Locomotion follows the latest input; work and combat still finish exclusively.
            State = wanted; _next = wanted; _loopDuration = 0;
            PlayPhase(wanted == PlayerAction.Idle ? ActionPhase.End : ActionPhase.Start);
        }
        public void FinishGathering()
        {
            if (State != PlayerAction.Logging && State != PlayerAction.Gather) return;
            _next = PlayerAction.Idle; PlayPhase(ActionPhase.End);
        }
        public void Die() { State = PlayerAction.Dead; _next = PlayerAction.Dead; PlayPhase(ActionPhase.Start); }
        public void ResetToIdle() { State = PlayerAction.Idle; _next = PlayerAction.Idle; _loopDuration = 0; PlayPhase(ActionPhase.Start); }
        private void Update()
        {
            if (State != PlayerAction.Dead && (Time.timeScale <= 0f || InputReader.HudBlocked)) return;
            if (_animator != null && _controller != _animator.runtimeAnimatorController) PlayPhase(Phase);
            _elapsed += State == PlayerAction.Dead ? Time.unscaledDeltaTime : Time.deltaTime;
            if (Phase == ActionPhase.Loop && State == PlayerAction.Logging && !_impactSent && _elapsed >= _duration * .5f)
            { _impactSent = true; LoopImpact?.Invoke(); }
            if (_elapsed < _duration) return;
            if (Phase == ActionPhase.Start) PlayPhase(ActionPhase.Loop);
            else if (Phase == ActionPhase.Loop)
            {
                if (State == PlayerAction.Attack || State == PlayerAction.Dead) PlayPhase(ActionPhase.End);
                else if (State == PlayerAction.Logging) { _elapsed -= _duration; _impactSent = false; }
            }
            else if (State != PlayerAction.Dead)
            { bool settlingIdle = State == PlayerAction.Idle && _next == PlayerAction.Idle;
                State = _next; _loopDuration = 0; PlayPhase(settlingIdle ? ActionPhase.Loop : ActionPhase.Start); }
        }
        private void PlayPhase(ActionPhase phase)
        {
            Phase = phase; _elapsed = 0; _impactSent = false;
            var attack = GetComponent<PlayerAttackController>();
            string weapon = attack != null && attack.WeaponIndex == 1 ? "bow" : "axe";
            var clip = Resources.Load<AnimationClip>($"Sprites/Player/RedHairGirl/Actions/{weapon}_{State}_{phase}");
            _duration = clip != null ? Mathf.Max(.05f, clip.length) : .2f;
            if (phase == ActionPhase.Loop && State == PlayerAction.Logging && _loopDuration > 0) _duration = _loopDuration;
            if (_animator == null) return;
            _controller = _animator.runtimeAnimatorController;
            _animator.updateMode = State == PlayerAction.Dead ? AnimatorUpdateMode.UnscaledTime : AnimatorUpdateMode.Normal;
            _animator.speed = clip != null ? clip.length / _duration : 1f;
            string name = $"{State}_{phase}";
            if (_animator.HasState(0, Animator.StringToHash(name))) _animator.Play(name, 0, 0);
        }
    }
}
