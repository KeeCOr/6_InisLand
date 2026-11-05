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
        [SerializeField] private float doorInteractDistance = 2.2f;
        public Door NearbyDoor
        {
            get { var door=Door.FindNearest(transform.position); return door!=null && door.IsNear(transform.position,doorInteractDistance) ? door : null; }
        }
        public int CurrentHp { get; private set; }
        public int MaxHp { get; private set; }
        public bool IsDead => CurrentHp <= 0;

        private Rigidbody2D _rb;
        private InputReader _input;
        private BalanceConfig _balance;
        private PlayerProgression _progression;

        // 플레이어 애니메이션
        private Animator _animator;
        private SpriteRenderer _spriteRenderer;
        private bool _facingLeft;
        public PlayerActionState Actions { get; private set; }

        private void Awake()
        {
            ActorBodyCollision.Configure(gameObject,false);
            YSort.Ensure(gameObject);
            // Door.TryHookPlayer() 이 FindWithTag("Player") 를 사용하므로 반드시 설정
            if (!gameObject.CompareTag("Player"))
                gameObject.tag = "Player";

            _rb = GetComponent<Rigidbody2D>();
            _input = GetComponent<InputReader>();
            _balance = BalanceConfig.Instance;
            _progression = GetComponent<PlayerProgression>();
            MaxHp = _balance.PlayerMaxHp;
            CurrentHp = MaxHp;

            // 애니메이터 가져오기
            _animator = GetComponent<Animator>();

            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = Color.white;
                var fallback = GetComponent<ColorFallback>();
                if (fallback != null) fallback.Tint = Color.white;
                var playerSprite = SpriteBank.PlayerIdle();
                if (playerSprite != null) _spriteRenderer.sprite = playerSprite;
            }
            var attacker = GetComponent<PlayerAttackController>();
            SetWeaponVisual(attacker != null ? attacker.WeaponIndex : 0);
            Actions = GetComponent<PlayerActionState>() ?? gameObject.AddComponent<PlayerActionState>();
            var directions=GetComponent<DirectionalCharacterVisual>() ?? gameObject.AddComponent<DirectionalCharacterVisual>();
            directions.Configure("Hero");

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

        }

        private void FixedUpdate()
        {
            if (IsDead)
            {
                _rb.velocity = Vector2.zero;
                UpdateMoveAnimation(Vector2.zero);
                return;
            }

            Vector2 moveDirection = _input.MoveAxis;
            if (Actions != null && Actions.BlocksMovement) { _rb.velocity = Vector2.zero; return; }
            if (Actions != null) Actions.SetMoving(moveDirection.sqrMagnitude > .001f);
            float speedMul = _progression != null ? _progression.MoveSpeedMultiplier : 1f;
            _rb.velocity = moveDirection * _balance.PlayerMoveSpeed * speedMul;

            // Keep the attack facing its target while locomotion remains fully responsive.
            if (Actions == null || Actions.State != PlayerAction.Attack) UpdateMoveAnimation(moveDirection);

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
            if (IsDead)
            {
                GetComponent<GatherController>()?.Cancel();
                Actions?.Die();
                _rb.velocity = Vector2.zero;
            }
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
            Actions?.ResetToIdle();
        }

        // Legacy callers can request opening; normal passage is automatic.
        public void TryInteractDoor()
        {
            if (InputReader.HudBlocked || Time.timeScale <= 0f || IsDead) return;
            Door nearestDoor = NearbyDoor;

            if (nearestDoor == null)
                return;

            nearestDoor.SetOpen(true);
        }

        public void SetWeaponVisual(int weaponIndex)
        {
            if (_animator == null) _animator = GetComponent<Animator>();
            if (_animator == null) _animator = gameObject.AddComponent<Animator>();
            string weapon = weaponIndex == 1 ? "Bow" : "Axe";
            var controller = Resources.Load<RuntimeAnimatorController>("Sprites/Player/RedHairGirl/RedHairGirl" + weapon);
            if (controller != null && _animator.runtimeAnimatorController != controller)
                _animator.runtimeAnimatorController = controller;
        }

        private void UpdateMoveAnimation(Vector2 moveDirection)
        {
            bool isMoving = moveDirection.sqrMagnitude > 0.001f ? true : false;

            if (_animator != null && Actions == null)
                _animator.SetBool("isMove", isMoving);

            if (!isMoving || _spriteRenderer == null)
                return;

            if (moveDirection.x < -0.01f)
                _facingLeft = true;
            else if (moveDirection.x > 0.01f)
                _facingLeft = false;

            // 왼쪽 입력이면 이미지 반전
            _spriteRenderer.flipX = _facingLeft;
        }

    }
}
