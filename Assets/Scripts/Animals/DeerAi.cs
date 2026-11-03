using UnityEngine;

namespace IL6
{
    /// <summary>
    /// 도주형 동물 — 토끼/여우/사슴/멧돼지/흰토끼.
    /// 플레이어가 FleeRadius 안에 들어오면 도망, 그 외엔 정지.
    /// </summary>
    public sealed class DeerAi : AnimalAi
    {
        public float FleeRadius = 2.8f;
        public float FleeSpeed = 2.6f;
        public float FleeAcceleration = 10f;
        public float AttackFleeDuration = 2.5f;
        private Vector2 _attackerPosition;
        private Vector2 _attackEscapeDirection;
        private float _attackedUntil;

        protected override void OnAttacked(Vector2 attackerPosition)
        {
            _attackerPosition=attackerPosition;
            _attackEscapeDirection=(Vector2)transform.position-attackerPosition;
            if(_attackEscapeDirection.sqrMagnitude<.0001f)_attackEscapeDirection=Vector2.right;
            _attackedUntil=Time.time+Mathf.Max(0f,AttackFleeDuration);
        }

        protected override void DoBehavior()
        {
            Vector2 toMe = (Vector2)(transform.position - _player.position);
            float d = toMe.magnitude;
            bool threatened=d<FleeRadius;
            if(IsAttackAlert || Time.time<_attackedUntil)
            {
                Vector2 away=(Vector2)transform.position-_attackerPosition;
                if(!threatened || away.sqrMagnitude<toMe.sqrMagnitude)
                    toMe=away.sqrMagnitude>.0001f?away:_attackEscapeDirection;
                threatened=true;
            }
            Vector2 target=threatened?toMe.normalized*Mathf.Max(0,FleeSpeed):Vector2.zero;
            _rb.velocity=Vector2.MoveTowards(_rb.velocity,target,Mathf.Max(.1f,FleeAcceleration)*Time.fixedDeltaTime);
        }
    }
}
