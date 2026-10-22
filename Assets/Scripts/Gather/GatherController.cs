using UnityEngine;

namespace IL6
{
    /// <summary>
    /// 통합 행동 버튼으로 가까운 Gatherable을 시작.
    /// 진행 중에는 다른 채집 차단. 완료되면 자원 적립.
    /// </summary>
    public sealed class GatherController : MonoBehaviour
    {
        public float InteractRadius = 32f;
        public ResourceStore Store { get; set; }
        public PlayerController Player { get; set; }
        public InputReader Input { get; set; }

        private Gatherable _active;
        private float _progress;
        private PlayerActionState _actions;
        private void Start()
        {
            _actions = GetComponent<PlayerActionState>();
            if (_actions != null) _actions.LoopImpact += OnLoggingImpact;
        }
        private void OnDestroy() { if (_actions != null) _actions.LoopImpact -= OnLoggingImpact; }
        private void OnLoggingImpact()
        {
            if (_active == null || !_active.IsTree || Store == null) return;
            var health = _active.EnsureTreeHealth();
            health.Chop(Mathf.RoundToInt(25 * CompanionTrait.GatherSpeedMultiplierFor(_active)), Store);
            _progress = 1f - (float)health.CurrentHp / health.MaxHp;
            if (health.CurrentHp == 0) Cancel();
        }

        public bool IsActive => _active != null;
        public float Progress => _progress;
        public Gatherable ActiveTarget => _active;

        public float CompanionAssistRange = 4.0f;
        public int CompanionAssistMax = 2;
        private float _assistRecheck;

        private void Update()
        {
            if (Store == null || Player == null || Input == null) return;

            if (_active != null)
            {
                if (Player.IsDead || (Input != null && Input.MoveAxis.sqrMagnitude > .001f)) { Cancel(); return; }
                float speedMul = CompanionTrait.GatherSpeedMultiplierFor(_active);
                if (!_active.IsTree) _progress += Time.deltaTime * speedMul / Mathf.Max(.1f,_active.DurationSec);
                else _progress = 1f - (float)_active.EnsureTreeHealth().CurrentHp / _active.EnsureTreeHealth().MaxHp;
                // 유휴 동료 자동 합류 — 0.5초마다 탐색
                _assistRecheck -= Time.deltaTime;
                if (_assistRecheck <= 0f)
                {
                    _assistRecheck = 0.5f;
                    AssignNearbyHelpers();
                }
                if (_progress >= 1f)
                {
                    _active.OnGathered(Store);
                    Cancel();
                }
            }
            else if (_actions != null && (_actions.State == PlayerAction.Logging || _actions.State == PlayerAction.Gather)
                && _actions.Phase != ActionPhase.End) _actions.FinishGathering();
        }

        private void AssignNearbyHelpers()
        {
            if (_active == null) return;
            var comps = Object.FindObjectsByType<Companion>(FindObjectsSortMode.None);
            // Follow 모드 + 유휴 동료만 모음
            var candidates = new System.Collections.Generic.List<Companion>();
            int alreadyOnTask = 0;
            foreach (var c in comps)
            {
                if (c == null || c.IsDead) continue;
                if (c.CurrentMode == Companion.Mode.Working && c.Target == _active) { alreadyOnTask++; continue; }
                if (c.CurrentMode != Companion.Mode.Follow) continue;
                if (Vector2.Distance(c.transform.position, _active.transform.position) > CompanionAssistRange) continue;
                candidates.Add(c);
            }
            int slotsLeft = CompanionAssistMax - alreadyOnTask;
            if (slotsLeft <= 0) return;
            // 가까운 순 정렬
            candidates.Sort((a, b) =>
                Vector2.Distance(a.transform.position, _active.transform.position)
                .CompareTo(Vector2.Distance(b.transform.position, _active.transform.position)));
            int n = Mathf.Min(slotsLeft, candidates.Count);
            for (int i = 0; i < n; i++) candidates[i].AssignGather(_active);
        }

        public void TryStart()
        {
            if (InputReader.HudBlocked || Time.timeScale <= 0f || _active != null || Player == null || Store == null || Player.IsDead) return;
            var hits = Physics2D.OverlapCircleAll(Player.transform.position, InteractRadius);
            Gatherable nearest = null;
            float nearestDist = float.MaxValue;
            foreach (var h in hits)
            {
                var g = h.GetComponent<Gatherable>();
                if (g == null || g.GetComponent<AnimalAi>() != null) continue;
                float d = Vector2.Distance(Player.transform.position, g.transform.position);
                if (d < nearestDist) { nearest = g; nearestDist = d; }
            }
            if (nearest != null)
            {
                StartGathering(nearest);
            }
        }

        public void Cancel()
        {
            _active = null;
            _progress = 0f;
            _actions?.FinishGathering();
        }

        public bool TryStartNearbyWood(float radius)
        {
            if (InputReader.HudBlocked || Time.timeScale <= 0f || Player == null || Store == null || Player.IsDead) return false;
            Gatherable nearest = null;
            float nearestDistance = radius;
            foreach (var hit in Physics2D.OverlapCircleAll(Player.transform.position, radius))
            {
                var target = hit.GetComponent<Gatherable>();
                if (target == null || target.YieldKind != ResourceKind.Wood || target.GetComponent<DiscoverySite>() != null) continue;
                float distance = Vector2.Distance(Player.transform.position, hit.ClosestPoint(Player.transform.position));
                if (distance > nearestDistance) continue;
                nearest = target;
                nearestDistance = distance;
            }
            if (nearest == null) return false;
            if (_active == nearest) return true; // Repeated attack presses keep logging progress.
            return StartGathering(nearest);
        }

        public Gatherable FindNearbyTarget(float radius)
        {
            if (Player == null) return null;
            Gatherable nearest = null;
            float nearestDistance = radius;
            foreach (var hit in Physics2D.OverlapCircleAll(Player.transform.position,radius))
            {
                var target = hit.GetComponent<Gatherable>();
                if (target == null || !target.isActiveAndEnabled || target.GetComponent<AnimalAi>() != null) continue;
                float distance = Vector2.Distance(Player.transform.position,hit.ClosestPoint(Player.transform.position));
                if (distance > nearestDistance) continue;
                nearest = target;
                nearestDistance = distance;
            }
            return nearest;
        }

        public bool TryStartNearby(float radius)
        {
            if (InputReader.HudBlocked || Time.timeScale <= 0f || Player == null || Player.IsDead || Store == null) return false;
            if (IsActive) return true; // Repeated Space keeps the same task and its progress.
            var target = FindNearbyTarget(radius);
            return target != null && StartGathering(target);
        }

        public string NearbyActionLabel(float radius)
        {
            var target = _active != null ? _active : FindNearbyTarget(radius);
            if (target == null) return "공격";
            if (target.GetComponent<DiscoverySite>() != null) return "조사";
            return target.IsTree ? "벌목" : target.YieldKind == ResourceKind.Stone ? "채굴" : "채집";
        }

        /// <summary>Start one gathering action; changing targets requires ending the current action.</summary>
        public bool StartGathering(Gatherable g)
        {
            if (g == null || Player == null || Store == null || Player.IsDead || InputReader.HudBlocked || Time.timeScale <= 0f) return false;
            if (g.GetComponent<AnimalAi>() != null) return false;
            if (_active == g) return true;
            if (_active != null) return false;
            if (g.IsTree && g.EnsureTreeHealth().CurrentHp <= 0) return false;
            if (_actions == null) _actions = GetComponent<PlayerActionState>();
            if (_actions != null && !_actions.TryBegin(g.IsTree ? PlayerAction.Logging : PlayerAction.Gather,
                g.transform.position - Player.transform.position, Mathf.Max(.2f,g.DurationSec / 4f))) return false;
            g.EnsureTreeHealth();
            _active = g;
            _progress = 0f;
            return true;
        }
    }
}
