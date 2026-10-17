using System.Collections.Generic;
using UnityEngine;

namespace IL6
{
    public sealed class SettlementGoalManager : MonoBehaviour
    {
        public static SettlementGoalManager Instance { get; private set; }

        public struct GoalView
        {
            public string Title;
            public string Detail;
            public string Progress;
            public bool Completed;
        }

        private sealed class GoalDef
        {
            public string Id;
            public string Title;
            public string Detail;
            public System.Func<GameSession, bool> Complete;
            public System.Func<GameSession, string> Progress;
            public System.Action<GameSession> Reward;
        }

        private static readonly List<GoalDef> _defs = new()
        {
            new GoalDef
            {
                Id = "food_security",
                Title = "식량 확보",
                Detail = "농장 2개와 식량 창고를 짓고 식량 35를 모으세요.",
                Complete = s => Count(BuildingKind.Farm) >= 2 && Count(BuildingKind.FoodStorage) >= 1 && s.Resources.Get(ResourceKind.Food) >= 35,
                Progress = s => $"농장 {Count(BuildingKind.Farm)}/2  창고 {Count(BuildingKind.FoodStorage)}/1  식량 {s.Resources.Get(ResourceKind.Food)}/35",
                Reward = s =>
                {
                    s.Resources.IncreaseCap(ResourceKind.Food, 25);
                    s.Resources.Add(ResourceKind.Food, 15);
                },
            },
            new GoalDef
            {
                Id = "fortified_ring",
                Title = "마을 방어",
                Detail = "울타리 14개, 목공소, 망루 또는 초소를 준비하세요.",
                Complete = s => Count(BuildingKind.Fence) >= 14 && Count(BuildingKind.Carpenter) >= 1
                    && (Count(BuildingKind.Watchtower) >= 1 || Count(BuildingKind.LookoutPost) >= 1),
                Progress = s => $"울타리 {Count(BuildingKind.Fence)}/14  목공 {Count(BuildingKind.Carpenter)}/1  망루 {Count(BuildingKind.Watchtower) + Count(BuildingKind.LookoutPost)}/1",
                Reward = s =>
                {
                    s.Resources.Add(ResourceKind.Wood, 20);
                    RepairAllBuildings(24);
                },
            },
            new GoalDef
            {
                Id = "warm_core",
                Title = "따뜻한 중심",
                Detail = "모닥불을 2레벨로 올리고 화로와 대장간을 지으세요.",
                Complete = s => Highest(BuildingKind.Campfire) >= 2 && Count(BuildingKind.Brazier) >= 1 && Count(BuildingKind.Blacksmith) >= 1,
                Progress = s => $"불 Lv.{Highest(BuildingKind.Campfire)}/2  화로 {Count(BuildingKind.Brazier)}/1  대장간 {Count(BuildingKind.Blacksmith)}/1",
                Reward = s =>
                {
                    s.Resources.Add(ResourceKind.Stone, 10);
                    s.Resources.Add(ResourceKind.Frostbloom, 5);
                },
            },
            new GoalDef
            {
                Id = "hunting_route",
                Title = "사냥 준비",
                Detail = "사냥꾼의 집을 짓고 3일째까지 고기 20을 모으세요.",
                Complete = s => Count(BuildingKind.HuntersHut) >= 1 && s.Cycle != null && s.Cycle.Day >= 3 && s.Resources.Get(ResourceKind.Meat) >= 20,
                Progress = s => $"사냥집 {Count(BuildingKind.HuntersHut)}/1  날짜 {(s.Cycle != null ? s.Cycle.Day : 1)}/3  고기 {s.Resources.Get(ResourceKind.Meat)}/20",
                Reward = s =>
                {
                    s.Resources.Add(ResourceKind.Meat, 10);
                    s.Resources.Add(ResourceKind.Wood, 8);
                },
            },
            new GoalDef
            {
                Id = "seasoned_party",
                Title = "숙련된 동료",
                Detail = "훈련소와 동료 6명을 준비하고 동료 총 레벨 10을 달성하세요.",
                Complete = s => Count(BuildingKind.TrainingCamp) >= 1 && RecruitableNpc.CurrentCompanionCount() >= 6 && TotalCompanionLevels() >= 10,
                Progress = s => $"훈련소 {Count(BuildingKind.TrainingCamp)}/1  동료 {RecruitableNpc.CurrentCompanionCount()}/6  레벨 {TotalCompanionLevels()}/10",
                Reward = s =>
                {
                    var comps = Object.FindObjectsByType<Companion>(FindObjectsSortMode.None);
                    foreach (var c in comps) if (c != null && !c.IsDead) c.GrantXp(2);
                },
            },
        };

        private readonly HashSet<string> _completed = new();
        private readonly List<GoalView> _views = new();
        public IReadOnlyList<GoalView> Goals => _views;

        private float _pollTimer;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            RebuildViews();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            _pollTimer += Time.unscaledDeltaTime;
            if (_pollTimer < 0.6f) return;
            _pollTimer = 0f;

            var s = GameSession.Instance;
            if (s == null || s.Resources == null) return;

            foreach (var d in _defs)
            {
                if (_completed.Contains(d.Id)) continue;
                if (!d.Complete(s)) continue;

                _completed.Add(d.Id);
                d.Reward?.Invoke(s);
                GameFeel.FloatText(Vector3.zero, $"목표 달성: {d.Title}", new Color(1f, 0.86f, 0.45f));

                var am = AchievementManager.Instance;
                if (am != null)
                {
                    am.NewlyUnlocked.Enqueue(new AchievementManager.Entry
                    {
                        Id = d.Id,
                        Title = $"목표 달성: {d.Title}",
                        Detail = d.Detail,
                    });
                }
            }

            RebuildViews();
        }

        private void RebuildViews()
        {
            _views.Clear();
            var s = GameSession.Instance;
            foreach (var d in _defs)
            {
                bool done = _completed.Contains(d.Id);
                _views.Add(new GoalView
                {
                    Title = d.Title,
                    Detail = d.Detail,
                    Progress = s != null && s.Resources != null ? d.Progress(s) : "",
                    Completed = done,
                });
            }
        }

        private static int Count(BuildingKind kind)
        {
            int count = 0;
            var buildings = Object.FindObjectsByType<Building>(FindObjectsSortMode.None);
            foreach (var b in buildings)
            {
                if (b == null || b.CurrentHp <= 0 || b.Kind != kind) continue;
                count++;
            }
            return count;
        }

        private static int Highest(BuildingKind kind)
        {
            int level = 0;
            var buildings = Object.FindObjectsByType<Building>(FindObjectsSortMode.None);
            foreach (var b in buildings)
            {
                if (b == null || b.CurrentHp <= 0 || b.Kind != kind) continue;
                if (b.Level > level) level = b.Level;
            }
            return level;
        }

        private static int TotalCompanionLevels()
        {
            int total = 0;
            var comps = Object.FindObjectsByType<Companion>(FindObjectsSortMode.None);
            foreach (var c in comps)
            {
                if (c == null || c.IsDead) continue;
                total += Mathf.Max(1, c.Level);
            }
            return total;
        }

        private static void RepairAllBuildings(int amount)
        {
            var buildings = Object.FindObjectsByType<Building>(FindObjectsSortMode.None);
            foreach (var b in buildings)
            {
                if (b == null || b.CurrentHp <= 0) continue;
                b.RepairHp(amount);
            }
        }
    }
}
