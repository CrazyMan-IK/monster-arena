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
        private const string _Radius = "_Radius";
        private const string _Angle = "_Angle";
        private const string _Fill = "_Fill";
        private const string _CircleActive = "_CircleActive";
        private const string _OrthographicSize = "_OrthographicSize";
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
        private Prop _prop = null;
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
        public bool HasProp => _prop != null;
        public bool IsStunned { get; private set; } = false;

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

            //_maxHP = information.HP;
            _hp = _maxHP;

            _information = information;

            _shadow.material.SetFloat(_Radius, AttackArea);
            _shadow.material.SetFloat(_Angle, information.AttackAngle);
            _shadow.material.SetFloat(_OrthographicSize, _shadow.orthographicSize);

            _animation.Initialize(information, 3 / _attackDelay);
            
            _attackTimer = new Timer((_attackDelay - _animation.GetAttackDuration()) * 3.7f);
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

            _shadow.material.SetFloat(_Radius, Mathf.Lerp(_shadow.material.GetFloat(_Radius), AttackArea, Time.deltaTime * _RadiusChangingSpeedMultiplier));
            _shadow.material.SetFloat(_Fill, Mathf.Min(_tickedTime / _targetTime, 1.0f));

            if (!HasProp)
            {
                var count = Physics.OverlapSphereNonAlloc(transform.position, 1, _lastColliders, _propLayerMask);
                for (int i = 0; i < count; i++)
                {
                    var collider = _lastColliders[i];

                    if (collider.TryGetComponent(out Prop prop) && prop.IsBase)
                    {
                        _animation.IsAttacking = true;
                        break;
                    }
                }
            }

            _attackTimer.Update(Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!HasProp && other.TryGetComponent(out Prop prop) && !prop.IsFaded && !prop.IsThrowed)
            {
                /*if (prop.IsBase)
                {
                    _near
                }
                else*/
                if (!prop.IsBase)
                {
                    _prop = prop;
                    
                    _prop.Take(_propRoot);
                }
            }
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

            _shadow.material.SetInt(_CircleActive, 1);

            _hp = _maxHP;
            _animation.Rigidbody.isKinematic = false;
            _animation.Rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            _collider.enabled = true;
            
            _animation.IsAlive = true;

            _attackTimer.Reset();
        }

        public void ThrowProp()
        {
            if (!HasProp)
            {
                return;
            }

            var center = transform.position + _helicopter.CurrentHeight / 2.0f * Vector3.back;
            var centerXZ = center.GetXZ();

            var count = Physics.SphereCastNonAlloc(center, 10, Vector3.up, _lastHits, 1000, _helicopterLayerMask);
            for (int i = 0; i < count; i++)
            {
                var collider = _lastHits[i].collider;
                var xzPos = _lastHits[i].transform.position.GetXZ();

                if (collider.TryGetComponent(out Helicopter helicopter) && helicopter.IsAlive && Vector2.Distance(centerXZ, xzPos) < 10)
                {
                    if (!_animation.IsThrowing)
                    {
                        _attackTimer.Stop();

                        _tickedTime = _animation.GetCurrentTime();
                    }

                    //_prop.Throw(transform, collider.transform.position);
                    //_prop = null;
                    _animation.IsThrowing = true;
                        
                    _lastTarget = helicopter.transform;

                    return;
                }
            }
            //_animation.IsThrowing = true;
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

            _shadow.material.SetInt(_CircleActive, 0);
            
            _hp = 0;
            _animation.Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            _animation.Rigidbody.isKinematic = true;
            _collider.enabled = false;

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

        //private float _lastTime = 0;

        private void OnAttacked()
        {
            /*for (int i = 0; i < 16384; i++)
            {
                var point = transform.position.GetXZ() + URandom.insideUnitCircle * AttackArea;
                var isInside = point.IsInsideCircleSector(transform.position.GetXZ(), transform.localEulerAngles.y, AttackArea, _information.AttackAngle);

                Debug.DrawRay(point.AsXZ(), Vector3.up, isInside ? Color.green : Color.red, 1.0f);
            }
            return;*/

            if (!IsAlive || IsStunned)
            {
                return;
            }

            //float distance = float.MaxValue;
            //Monster closest = null;
            if (_animation.IsThrowing && HasProp)
            {
                _attackTimer.Start();

                //var prop = Instantiate(_propsPrefabs[URandom.Range(0, _propsPrefabs.Count)], _propRoot.position, Quaternion.identity);
                
                _prop.Throw(transform, _lastTarget.position + Vector3.up * 0.5f, _damage);
                _prop = null;

                _animation.IsThrowing = false;

                _tickedTime = 0;
                
                //Debug.Log($"Secs: {Time.time - _lastTime}");

                //_lastTime = Time.time;

                return;
            }
            
            var count = Physics.OverlapSphereNonAlloc(transform.position, 1, _lastColliders, _propLayerMask);
            for (int i = 0; i < count; i++)
            {
                var collider = _lastColliders[i];

                if (collider.TryGetComponent(out Prop prop) && prop.IsBase)
                {
                    prop.Demolish(transform.position);

                    //break;
                }
            }
            
            _animation.IsAttacking = false;
            _bomb.Explode(0.05f);

            //UnityEditor.EditorApplication.isPaused = true;

            /*var center = transform.position + Vector3.back * 10;//_movement.CurrentHeight / 2.0f;

            var count = Physics.SphereCastNonAlloc(center, 5, Vector3.up, _lastColliders, 1000, _helicopterLayerMask);
            for (int i = 0; i < count; i++)
            {
                var collider = _lastColliders[i].collider;

                if (collider.TryGetComponent(out Helicopter helicopter) && helicopter.HP > 0)
                {
                    //var current = Vector2.Distance(transform.position.GetXZ(), collider.transform.position.GetXZ());
                    *//*if (_prop != null && _animation.IsThrowing && current < distance)
                    {
                        distance = current;
                        closest = monster;

                        continue;
                    }*//*
                    if (!collider.transform.position.GetXZ().IsInsideCircleSector(transform.position.GetXZ(), transform.localEulerAngles.y, AttackArea, _information.AttackAngle))
                    {
                        continue;
                    }

                    if (monster._tutorialHP > 0)
                    {
                        monster.TakeDamage(monster._maxHP / monster._tutorialHP, DamageSource.Player);
                    }
                    else if (Level >= monster.Level)
                    {
                        monster.Die(DamageSource.Player);
                    }
                    else
                    {
                        monster.TakeDamage(monster._maxHP / (monster.Level - Level + 1), DamageSource.Player);
                    }
                    
                    if (monster.IsAlive)
                    {
                        continue;
                    }

                    LevelChanged?.Invoke(++Level);
                    
                    Killed?.Invoke(this, monster.transform);
                }
            }*/
        }

        private void OnKilledByAbility(Transform target)
        {
            LevelChanged?.Invoke(++Level);
                    
            Killed?.Invoke(this, target);
        }
    }
}
