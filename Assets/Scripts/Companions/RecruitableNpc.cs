using UnityEngine;
using IL6.Events;

namespace IL6
{
    public sealed class RecruitableNpc : MonoBehaviour
    {
        public Transform Player;
        public float RecruitRange = 1.8f;
        public KeyCode RecruitKey = KeyCode.F;

        [Header("Profile")]
        public string DisplayName = "Stranger";
        public string Role = "Hunter";
        [TextArea(1, 3)] public string DialogText = "\uD568\uAED8 \uAC00\uACE0 \uC2F6\uC2B5\uB2C8\uB2E4.";
        [Range(0, 5)] public int CombatRating = 3;
        [Range(0, 5)] public int FarmRating = 3;

        [Header("Recruited Companion config")]
        public bool IsCombat = true;
        public float FollowStopDistance = 0.25f;
        public float MoveSpeed = 4.5f;
        public float AttackRange = 5f;
        public int AttackDamage = 6;
        public float AttackCooldown = 1.6f;

        public bool IsPlayerInRange { get; private set; }
        public string DisplayNamePublic => DisplayName;

        private Vector3 _baseScale;
        private bool _recruited;

        private void Awake()
        {
            var rb = GetComponent<Rigidbody2D>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.bodyType = RigidbodyType2D.Kinematic;

            var col = GetComponent<CircleCollider2D>();
            if (col == null)
            {
                col = gameObject.AddComponent<CircleCollider2D>();
                col.radius = 0.35f;
            }
        }

        private void Start()
        {
            _baseScale = Vector3.Scale(transform.localScale, SpriteBank.CompanionScaleForRole(Role));
            transform.localScale = _baseScale;
            if (Player == null)
            {
                var p = GameObject.FindWithTag("Player");
                if (p != null) Player = p.transform;
            }
        }

        private void Update()
        {
            if (Player == null)
            {
                IsPlayerInRange = false;
                return;
            }

            float d = Vector2.Distance(transform.position, Player.position);
            IsPlayerInRange = d <= RecruitRange;

            if (IsPlayerInRange)
            {
                float pulse = 1f + Mathf.Sin(Time.time * 6f) * 0.08f;
                transform.localScale = _baseScale * pulse;
                if (Input.GetKeyDown(RecruitKey)) Recruit();
            }
            else
            {
                transform.localScale = _baseScale;
            }
        }

        public const int FreeCapacity = 12;
        public const int CapacityPerHouse = 4;

        public static int VillageCapacity()
        {
            int houses = 0;
            var bs = Object.FindObjectsByType<Building>(FindObjectsSortMode.None);
            foreach (var b in bs)
            {
                if (b == null || b.CurrentHp <= 0) continue;
                if (b.Kind == BuildingKind.House) houses++;
            }
            return FreeCapacity + houses * CapacityPerHouse;
        }

        public static int CurrentCompanionCount()
        {
            int n = 0;
            var cs = Object.FindObjectsByType<Companion>(FindObjectsSortMode.None);
            foreach (var c in cs)
            {
                if (c != null && !c.IsDead) n++;
            }
            return n;
        }

        public bool CanRecruit() => CurrentCompanionCount() < VillageCapacity();

        public static void ResetDailyRecruits(int day) { }

        public void Recruit()
        {
            if (!CanRecruit()) return;
            if (_recruited) return;
            _recruited = true;

            uint traitSeed = unchecked((uint)DisplayName.GetHashCode() ^ (uint)Role.GetHashCode() ^ (uint)Time.frameCount);
            var trait = CompanionTrait.AssignRandom(gameObject, Role, new SeededRng(traitSeed));
            string dialog = trait != null && trait.Kind != CompanionTraitKind.None
                ? $"{DialogText}\nTrait: {trait.DisplayName} - {trait.Description}"
                : DialogText;

            var rb = GetComponent<Rigidbody2D>();
            if (rb != null) rb.bodyType = RigidbodyType2D.Dynamic;

            var comp = gameObject.AddComponent<Companion>();
            comp.Player = Player;
            comp.IsCombat = IsCombat;
            comp.FollowStopDistance = FollowStopDistance;
            comp.MoveSpeed = MoveSpeed;
            comp.AttackRange = AttackRange;
            comp.Damage = AttackDamage;
            comp.AttackCooldown = AttackCooldown;
            comp.GatherReach = 0.7f;

            var family = gameObject.GetComponent<CompanionFamily>();
            if (family == null) family = gameObject.AddComponent<CompanionFamily>();
            family.BiologicalSex = CompanionFamily.SexForRole(Role);
            family.IsChild = SpriteBank.IsChildRole(Role);

            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                var spr = SpriteBank.CompanionSpriteForRole(Role);
                if (spr != null) sr.sprite = spr;
                sr.color = Color.white;
            }

            transform.localScale = _baseScale;
            gameObject.name = $"{DisplayName}(Recruited)";

            EventBus.Instance.Emit(new CompanionRecruitedPayload(DisplayName, Role, dialog));
            Destroy(this);
        }
    }
}
