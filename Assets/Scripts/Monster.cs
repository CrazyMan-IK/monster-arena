using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using URandom = UnityEngine.Random;
using DG.Tweening;
using MonsterArena.Models;
using MonsterArena.Extensions;
using MonsterArena.Interfaces;
using RayFire;
using Source.EnemyView;

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
        private const float _RadiusChangingSpeedMultiplier = 5;
        private const float _RotationSpeedMultiplier = 15;
        private const float _CellsToLevelUp = 3;
        private const float _StunDuration = 2.5f;

        public event Action WinAnimationCompleted = null;
        public event Action<int> LevelChanged = null;
        public event Action<Monster, Transform> Killed = null;
        public event Action<Monster, DamageSource> Died = null;

        [Header("Main")]
        [SerializeField] private MonsterInformation _information = null;
        [SerializeField] private ParticleSystem _stunEffect = null;
        [SerializeField] private Resource _resourcePrefab = null;
        [SerializeField] private Renderer _renderer = null;
        [SerializeField] private Projector _shadow = null;
        [SerializeField] private Collider _collider = null;
        [SerializeField] private RayfireBomb _bomb = null;
        //[SerializeField] private Vector3 _bombOffset = Vector3.zero;
        [SerializeField] private Transform _propRoot = null;
        [SerializeField] private MonsterAnimationEventsRepeater _repeater = null;
        [SerializeField] private FieldOfView _fieldOfView;
        [SerializeField] private LayerMask _propLayerMask = default;
        [SerializeField] private LayerMask _helicopterLayerMask = default;
        [SerializeField] private float _attackDelay = 3;
        [SerializeField] private float _maxHP = 1;
        [SerializeField] private float _damage = 1;

        [field: SerializeField] public int ResourcesCount { get; private set; } = 1;
        [field: SerializeField] public int Level { get; private set; } = 1;

        [SerializeField, HideInInspector] private Helicopter _helicopter = null;
        private readonly Collider[] _lastColliders = new Collider[8];
        private readonly RaycastHit[] _lastHits = new RaycastHit[64];
        private MonsterAnimation _animation = null;
        private IMonsterAbility _ability = null;
        //private Prop _prop = null;
        //private Helicopter _lastClosestHelicopter = null;
        private Transform _lastTarget = null;
        private Timer _attackTimer = null;
        private float _hp = 0;
        private float _targetTime = 0;
        private float _tickedTime = 0;
        private int _cellsConsumed = 0;
        //private bool _isPlayer = false;
        private bool _isWin = false;

        public Renderer Renderer => _renderer;
        public Collider Collider => _collider;
        public Rigidbody Rigidbody => _animation.Rigidbody;
        public MonsterInformation Information => _information;
        public Helicopter Helicopter => _helicopter;
        public float HP => _hp / _maxHP;
        public float MaxHP => _maxHP;
        public bool IsAlive => _hp > 0;
        public float AbilityCooldown => _ability.Cooldown;
        public float MovementSpeed => _ability.TransformSpeed(_information.MovementSpeed);
        public float AttackArea => _ability.TransformRange(_information.AttackArea);// * (_prop != null ? 1.5f : 1.0f);
        public float CameraDistance => _ability.TransformRange(10);// * (_prop != null ? 1.5f : 1.0f);// _prop != null ? 15 : 10);
        public bool CanUseAbility => _ability.CanUse;
        //public bool HasProp => _prop != null;
        public bool IsStunned { get; private set; } = false;
        public bool IsThrowing => _animation.IsThrowing;

        private void Awake()
        {
            _animation = GetComponent<MonsterAnimation>();
            _ability = GetComponent<IMonsterAbility>();

            _targetTime = _animation.GetFirstAttackTime();
            
            _shadow.material = new Material(_shadow.material);

            if (_information != null)
            {
                Initialize(_information);
            }

            var localPos = _bomb.transform.localPosition;
            var localRot = _bomb.transform.localRotation;
            _bomb.transform.SetParent(_propRoot);
            _bomb.transform.localPosition = localPos;
            _bomb.transform.localRotation = localRot;
        }

        private void Start()
        {
            LevelChanged?.Invoke(Level);
        }

        public void Initialize(MonsterInformation information)
        {
            _ability.Initialize(information, _helicopterLayerMask);

            _hp = _maxHP;

            _information = information;

            _shadow.material.SetFloat(Constants.Radius, AttackArea);
            _shadow.material.SetFloat(Constants.Angle, information.AttackAngle);
            _shadow.material.SetFloat(Constants.OrthographicSize, _shadow.orthographicSize);

            _animation.Initialize(information, _animation.GetAttackDuration() / 3 / 0.66f);
            
            _attackTimer = new Timer(_attackDelay);
            _attackTimer.Ticked += ThrowProp;
        }

        private void OnValidate()
        {
            _helicopter = FindObjectOfType<Helicopter>();
        }

        private void OnEnable()
        {
            _repeater.Winned += OnWinAnimationCompleted;
            _repeater.Attacked += OnAttacked;

            _ability.Killed += OnKilledByAbility;
        }

        private void OnDisable()
        {
            _repeater.Winned -= OnWinAnimationCompleted;
            _repeater.Attacked -= OnAttacked;

            _ability.Killed -= OnKilledByAbility;
        }

        private void OnDestroy()
        {
            _attackTimer.Ticked -= ThrowProp;
        }

        private void Update()
        {
            _animation.IsAttacking = false;
            if (_information == null || !IsAlive || IsStunned)
            {
                return;
            }

            if (_animation.IsThrowing)
            {
                _tickedTime += Time.deltaTime;
            }

            _shadow.material.SetFloat(Constants.Radius, Mathf.Lerp(_shadow.material.GetFloat(Constants.Radius), AttackArea, Time.deltaTime * _RadiusChangingSpeedMultiplier));
            _shadow.material.SetFloat(Constants.Fill, Mathf.Min(_tickedTime / _targetTime, 1.0f));

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

            _shadow.material.SetInt(Constants.CircleActive, 1);

            _hp = _maxHP;
            _animation.Rigidbody.isKinematic = false;
            _animation.Rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            _collider.enabled = true;
            
            _animation.IsAlive = true;

            _attackTimer.Reset();
            _attackTimer.Start();
        }

        public void ThrowProp()
        {
            if (HelicopterInRange(out Helicopter helicopter))
            {
                if (!_animation.IsThrowing)
                {
                    _attackTimer.Stop();

                    _tickedTime = _animation.GetCurrentTime();
                }

                SetThrowing(true);

                _lastTarget = helicopter.transform;
            }
        }

        private bool HelicopterInRange(out Helicopter helicopter)
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

        public void EnableRadiusPreview()
        {
            _shadow.material.SetInt("_CircleActive", 1);
        }

        public void DisableRadiusPreview()
        {
            _shadow.material.SetInt("_CircleActive", 0);
        }

        public void UseAbility()
        {
            _ability.Use();
        }

        public void TakeDamage(float damage, DamageSource source)
        {
            if (_isWin)
            {
                return;
            }

            damage = _ability.TransformReceivedDamage(damage);

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
            if (!_animation.IsAlive || IsAlive && _ability.TransformReceivedDamage(_hp) <= 0 || _isWin)
            {
                return;
            }

            _shadow.material.SetInt(Constants.CircleActive, 0);
            
            _hp = 0;
            _animation.Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            _animation.Rigidbody.isKinematic = true;
            _collider.enabled = false;
            SetThrowing(false);
            
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

        private IEnumerator Stun()
        {
            IsStunned = true;
            _stunEffect.Play();

            yield return new WaitForSeconds(_StunDuration);

            _stunEffect.Stop();
            IsStunned = false;
        }

        private void OnWinAnimationCompleted()
        {
            WinAnimationCompleted?.Invoke();
        }

        private void OnAttacked()
        {
            if (!IsAlive || IsStunned)
            {
                return;
            }

            if (_animation.IsThrowing)
            {
                _attackTimer.Start();

                SetThrowing(false);
                _tickedTime = 0;
                
                if(_fieldOfView.TryFindVisibleTarget(out Helicopter helicopter))
                    helicopter.TakeDamage(_information.Damage);
                
                return;
            }
            
            _animation.IsAttacking = false;
        }

        private void SetThrowing(bool active)
        {
            _animation.IsThrowing = active;
            _fieldOfView.gameObject.SetActive(active);
        }

        private void OnKilledByAbility(Transform target)
        {
            LevelChanged?.Invoke(++Level);
                    
            Killed?.Invoke(this, target);
        }
    }
}
