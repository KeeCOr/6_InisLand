using UnityEngine;
using IL6.Events;

namespace IL6
{
    public sealed class VillageChildGrowth : MonoBehaviour
    {
        public int BirthDay;
        public int AgeDays;

        public bool IsAdult => AgeDays >= 11;

        public int FoodUnitsX2
        {
            get
            {
                if (AgeDays < 3) return 1;
                if (AgeDays < 7) return 2;
                return 2;
            }
        }

        private Companion _companion;
        private CompanionFamily _family;
        private System.Action _unsubDay;
        private int _lastAppliedDay = -1;

        private void Awake()
        {
            _companion = GetComponent<Companion>();
            _family = GetComponent<CompanionFamily>();
        }

        private void Start()
        {
            ApplyStageStats(true);
            _unsubDay = EventBus.Instance.Subscribe<DayStartedPayload>(p => OnDayStarted(p.Day));
        }

        private void OnDestroy()
        {
            _unsubDay?.Invoke();
        }

        private void OnDayStarted(int day)
        {
            if (_lastAppliedDay == day) return;
            _lastAppliedDay = day;

            if (BirthDay <= 0) BirthDay = day;
            AgeDays = Mathf.Max(AgeDays, day - BirthDay);
            ApplyStageStats(false);
        }

        private void ApplyStageStats(bool fillHp)
        {
            if (_companion == null) _companion = GetComponent<Companion>();
            if (_companion == null) return;
            if (_family == null) _family = GetComponent<CompanionFamily>();
            if (_family != null) _family.IsChild = AgeDays < 11;

            if (AgeDays < 3)
            {
                _companion.IsCombat = false;
                _companion.MoveSpeed = 3.2f;
                _companion.AttackRange = 1.0f;
                _companion.Damage = 1;
                _companion.AttackCooldown = 2.5f;
                _companion.SetMaxHp(25, fillHp);
                transform.localScale = Vector3.one * 0.7f;
                return;
            }

            if (AgeDays < 7)
            {
                _companion.IsCombat = false;
                _companion.MoveSpeed = 4.0f;
                _companion.AttackRange = 1.2f;
                _companion.Damage = 2;
                _companion.AttackCooldown = 2.2f;
                _companion.SetMaxHp(35, fillHp);
                transform.localScale = Vector3.one * 0.82f;
                return;
            }

            if (AgeDays < 11)
            {
                _companion.IsCombat = true;
                _companion.MoveSpeed = 4.3f;
                _companion.AttackRange = 4.0f;
                _companion.Damage = 4;
                _companion.AttackCooldown = 1.8f;
                _companion.SetMaxHp(45, fillHp);
                transform.localScale = Vector3.one * 0.92f;
                return;
            }

            _companion.IsCombat = true;
            _companion.MoveSpeed = 4.5f;
            _companion.AttackRange = 5.0f;
            _companion.Damage = 6;
            _companion.AttackCooldown = 1.6f;
            _companion.SetMaxHp(50, fillHp);
            transform.localScale = Vector3.one;
            if (_family != null)
            {
                _family.IsChild = false;
                if (_family.BiologicalSex == CompanionFamily.Sex.Unknown)
                    _family.BiologicalSex = (BirthDay + gameObject.GetInstanceID()) % 2 == 0
                        ? CompanionFamily.Sex.Female
                        : CompanionFamily.Sex.Male;
            }
            Destroy(this);
        }
    }
}
