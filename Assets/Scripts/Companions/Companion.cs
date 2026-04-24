using UnityEngine;

namespace IL6
{
    /// <summary>
    /// 부하. 평소엔 Player 를 FollowDistance 거리로 따라다니고, AssignGather 로
    /// Gatherable 이 할당되면 그리 이동해서 자동 채집.
    /// </summary>
    public sealed class Companion : MonoBehaviour
    {
        public Transform Player;
        public float FollowDistance = 1.8f;
        public float FollowStopDistance = 1.2f;
        public float MoveSpeed = 4.5f;
        public float GatherReach = 0.7f;

        public enum Mode { Follow, Working }
        public Mode CurrentMode { get; private set; } = Mode.Follow;
        public Gatherable Target { get; private set; }

        private Vector2 _lastMoveDir = Vector2.right;

        public void AssignGather(Gatherable target)
        {
            Target = target;
            CurrentMode = target != null ? Mode.Working : Mode.Follow;
        }

        private void Start()
        {
            if (Player == null)
            {
                var p = GameObject.FindWithTag("Player");
                if (p != null) Player = p.transform;
            }
        }

        private void Update()
        {
            if (CurrentMode == Mode.Working)
            {
                if (Target == null)
                {
                    CurrentMode = Mode.Follow;
                    return;
                }
                Vector2 toTarget = (Vector2)Target.transform.position - (Vector2)transform.position;
                float dist = toTarget.magnitude;
                if (dist < GatherReach)
                {
                    var session = GameSession.Instance;
                    if (session != null)
                    {
                        Target.OnGathered(session.Resources);
                    }
                    Target = null;
                    CurrentMode = Mode.Follow;
                    return;
                }
                var dir = toTarget.normalized;
                _lastMoveDir = dir;
                transform.position += (Vector3)(dir * MoveSpeed * Time.deltaTime);
            }
            else
            {
                if (Player == null) return;
                Vector2 toPlayer = (Vector2)Player.position - (Vector2)transform.position;
                float dist = toPlayer.magnitude;
                if (dist > FollowStopDistance)
                {
                    var dir = toPlayer.normalized;
                    _lastMoveDir = dir;
                    float speedFactor = Mathf.Clamp01((dist - FollowStopDistance) / FollowDistance);
                    transform.position += (Vector3)(dir * MoveSpeed * speedFactor * Time.deltaTime);
                }
            }
        }
    }
}
