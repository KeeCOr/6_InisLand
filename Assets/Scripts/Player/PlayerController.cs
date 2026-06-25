using UnityEngine;

namespace IL6
{
    /// <summary>
    /// 플레이어 이동 + HP 관리. Rigidbody2D 사용.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(InputReader))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private float doorInteractDistance = 1.5f;
        public int CurrentHp { get; private set; }
        public int MaxHp { get; private set; }
        public bool IsDead => CurrentHp <= 0;

        private Rigidbody2D _rb;
        private InputReader _input;
        private BalanceConfig _balance;
        private PlayerProgression _progression;

        private void Awake()
        {
            // Door.TryHookPlayer() 이 FindWithTag("Player") 를 사용하므로 반드시 설정
            if (!gameObject.CompareTag("Player"))
                gameObject.tag = "Player";

            _rb = GetComponent<Rigidbody2D>();
            _input = GetComponent<InputReader>();
            _balance = BalanceConfig.Instance;
            _progression = GetComponent<PlayerProgression>();
            MaxHp = _balance.PlayerMaxHp;
            CurrentHp = MaxHp;

            var sr = GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite == null)
            {
                var spr = SpriteBank.Player();
                if (spr != null) sr.sprite = spr;
            }

            if (transform.localScale == Vector3.one)
                transform.localScale = new Vector3(0.9f, 1.0f, 1.0f);
            _rb.gravityScale = 0f;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _rb.freezeRotation = true;

            if (_progression != null)
            {
                _progression.OnRuneApplied += OnRuneApplied;
            }
        }

        private void OnDestroy()
        {
            if (_progression != null) _progression.OnRuneApplied -= OnRuneApplied;
        }

        private void OnRuneApplied(RuneKind kind)
        {
            if (kind == RuneKind.HpUp)
            {
                MaxHp = _balance.PlayerMaxHp + _progression.BonusMaxHp;
                CurrentHp = Mathf.Min(MaxHp, CurrentHp + 25);
            }
        }

        private float _snowTimer;
        private float _regenTimer;
        public float RegenIntervalSec = 4f; // 매 4초마다 +1 HP

        private void Update()
        {
            if (IsDead) return;
            _regenTimer += Time.deltaTime;
            if (_regenTimer >= RegenIntervalSec)
            {
                _regenTimer = 0f;
                if (CurrentHp < MaxHp) Heal(1);
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                TryInteractDoor();
            }
        }

        private void FixedUpdate()
        {
            if (IsDead) { _rb.velocity = Vector2.zero; return; }
            _rb.velocity = _input.MoveAxis * _balance.PlayerMoveSpeed;

            if (_rb.velocity.sqrMagnitude > 0.05f)
            {
                _snowTimer -= Time.fixedDeltaTime;
                if (_snowTimer <= 0f)
                {
                    _snowTimer = 0.16f;
                    GameFeel.SnowPuff(transform.position + Vector3.down * 0.2f);
                }
            }
        }

        public void TakeDamage(int amount)
        {
            if (IsDead) return;
            CurrentHp = Mathf.Max(0, CurrentHp - amount);
            GameFeel.HitFlash(this, GetComponent<SpriteRenderer>());
            CameraFollow.Shake(0.18f + amount * 0.012f, 0.25f);

            // 가시 (Thorns) — 인근 적에게 반사 대미지
            if (_progression != null && _progression.ThornsDmg > 0)
            {
                int td = _progression.ThornsDmg;
                float r = _progression.ThornsRadius;
                var hits = Physics2D.OverlapCircleAll(transform.position, r);
                foreach (var h in hits)
                {
                    if (h == null) continue;
                    var z = h.GetComponent<Zombie>();
                    if (z != null && !z.IsDead) { z.TakeDamage(td); continue; }
                    var a = h.GetComponent<AnimalAi>();
                    if (a != null && !a.IsDead) a.TakeDamage(td);
                }
            }
        }

        public void Heal(int amount)
        {
            if (IsDead) return;
            CurrentHp = Mathf.Min(MaxHp, CurrentHp + amount);
        }

        public void ResetState(Vector2 spawnPosition)
        {
            CurrentHp = MaxHp;
            transform.position = spawnPosition;
            _rb.velocity = Vector2.zero;
        }

        // 문 열고 닫는 함수
        private void TryInteractDoor()
        {
            Door nearestDoor = Door.FindNearest(transform.position);

            if (nearestDoor == null)
                return;

            if (!nearestDoor.IsNear(transform.position, doorInteractDistance))
                return;

            nearestDoor.Toggle();
        }
    }
}
