using System;
using System.Collections;
using AYellowpaper;
using UnityEngine;
using URandom = UnityEngine.Random;
using DG.Tweening;
using MonsterArena.Models;
using MonsterArena.Extensions;
using MonsterArena.Interfaces;

namespace MonsterArena
{
    public enum DamageSource
    {
        Player,
        Other
    }

    [RequireComponent(typeof(MonsterAnimation))]
    [RequireComponent(typeof(IMonsterAbility))]
    public class Monster : MonoBehaviour, IHealthComponent
    {
        public event Action WinAnimationCompleted = null;
        public event Action<Monster, DamageSource> Died = null;

        [Header("Main")]
        [SerializeField] private MonsterInformation _information = null;
        [SerializeField] private InterfaceReference<IMonsterAttack> _monsterAttack;
        [SerializeField] private Resource _resourcePrefab = null;
        [SerializeField] private Renderer _renderer = null;
        [SerializeField] private Collider _collider = null;
        [SerializeField] private MonsterAnimationEventsRepeater _repeater = null;
        [SerializeField] private LayerMask _helicopterLayerMask = default;
        [SerializeField] private float _attackDelay = 3;
        [SerializeField] private float _maxHP = 1;

        [field: SerializeField] public int ResourcesCount { get; private set; } = 1;
        [field: SerializeField] public int Level { get; private set; } = 1;

        [SerializeField, HideInInspector] private Helicopter _helicopter = null;
        private readonly RaycastHit[] _lastHits = new RaycastHit[64];
        private MonsterAnimation _animation = null;
        private Timer _attackTimer = null;
        private float _hp = 0;
        private int _cellsConsumed = 0;
        private bool _isWin = false;

        public Renderer Renderer => _renderer;
        public Collider Collider => _collider;
        public Rigidbody Rigidbody => _animation.Rigidbody;
        public MonsterInformation Information => _information;
        public Helicopter Helicopter => _helicopter;
        public float HP => _hp / _maxHP;
        public float MaxHP => _maxHP;
        public bool IsAlive => _hp > 0;
        public float MovementSpeed => _information.MovementSpeed;
        public bool IsStunned { get; private set; } = false;
        public bool IsThrowing => _animation.IsThrowing;

        private void Awake()
        {
            _animation = GetComponent<MonsterAnimation>();
            
            if (_information != null)
            {
                Initialize(_information);
            }
        }

        public void Initialize(MonsterInformation information)
        {
            _hp = _maxHP;
            _information = information;

            _animation.Initialize(information, _animation.GetAttackDuration() / 3 / 0.66f);
            
            _attackTimer = new Timer(_attackDelay);
            _attackTimer.Ticked += Attack;
        }

        private void OnValidate()
        {
            _helicopter = FindObjectOfType<Helicopter>();
        }

        private void OnEnable()
        {
            _repeater.Winned += OnWinAnimationCompleted;
            _repeater.Attacked += OnAttacked;
        }

        private void OnDisable()
        {
            _repeater.Winned -= OnWinAnimationCompleted;
            _repeater.Attacked -= OnAttacked;
        }

        private void OnDestroy()
        {
            _attackTimer.Ticked -= Attack;
        }

        private void Update()
        {
            _animation.IsAttacking = false;
            if (_information == null || !IsAlive || IsStunned)
            {
                return;
            }

            if (HelicopterInRange(out Helicopter _))
                _attackTimer.Update(Time.deltaTime);
            else
                _attackTimer.Reset();
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, _information.ViewArea);
        }

        public void Revive()
        {
            if (IsAlive)
            {
                return;
            }

            _hp = _maxHP;
            _animation.Rigidbody.isKinematic = false;
            _animation.Rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            _collider.enabled = true;
            
            _animation.IsAlive = true;

            _attackTimer.Reset();
            _attackTimer.Start();
        }

        private void Attack()
        {
            if(!IsAlive)
                return;
            
            if (HelicopterInRange(out Helicopter helicopter))
            {
                _attackTimer.Stop();
                _monsterAttack.Value.StartAttack();
            }
        }

        public bool HelicopterInRange(out Helicopter helicopter)
        {
            var center = transform.position + _helicopter.CurrentHeight / 2.0f * Vector3.back;
            var centerXZ = center.GetXZ();
            helicopter = null;
            
            var count = Physics.SphereCastNonAlloc(center, 10, Vector3.up, _lastHits, 1000, _helicopterLayerMask);
            for (int i = 0; i < count; i++)
            {
                var collider = _lastHits[i].collider;
                var xzPos = _lastHits[i].transform.position.GetXZ();

                if (collider.TryGetComponent(out Helicopter target) && target.IsAlive &&
                    Vector2.Distance(centerXZ, xzPos) < 10)
                {
                    helicopter = target;
                    return true;
                }
            }

            return false;
        }

        public void TakeDamage(float damage, DamageSource source)
        {
            if (_isWin)
            {
                return;
            }
            
            if (damage < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(damage));
            }

            _hp -= damage;

            if (!IsAlive)
            {
                Die(source);

                return;
            }

            _animation.IsAlive = true;
        }

        public void Die(DamageSource source)
        {
            if (!_animation.IsAlive || IsAlive || _isWin)
            {
                return;
            }

            _hp = 0;
            _animation.Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            _animation.Rigidbody.isKinematic = true;
            _collider.enabled = false;
            _monsterAttack.Value.Reset();
            _animation.IsAttacking = false;

            Died?.Invoke(this, source);

            SpawnResources();

            _animation.IsAlive = false;
        }

        public void RunWinningAnimation()
        {
            _animation.ActivateWinAnimation(false);
            _isWin = true;
        }

        public void RunWinningAnimationOnce()
        {
            _animation.ActivateWinAnimation(true);
        }

        private void SpawnResources()
        {
            for (int i = 0; i < ResourcesCount; i++)
            {
                var duration = URandom.Range(0.5f, 0.75f);

                var resource = Instantiate(_resourcePrefab, transform.position, Quaternion.identity);
                resource.transform.DOScale(Vector3.one * URandom.Range(0.5f, 1.0f), duration).From(Vector3.zero);
                resource.transform.DOJump(transform.position + URandom.insideUnitCircle.AsXZ() * 3, URandom.Range(1.0f, 1.5f), 1, duration);
            }
        }
        
        private void OnWinAnimationCompleted()
        {
            WinAnimationCompleted?.Invoke();
        }

        private void OnAttacked()
        {
            if (!IsAlive || IsStunned)
                return;
            
            StartTimer();
            _monsterAttack.Value.Hit(_information.Damage);
            _animation.IsAttacking = false;
        }

        private void StartTimer()
        {
            _attackTimer.Reset();
            _attackTimer.Start();
        }
    }
}
