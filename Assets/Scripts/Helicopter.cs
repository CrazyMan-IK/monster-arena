using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using URandom = UnityEngine.Random;
using DG.Tweening;
using Cinemachine;
using MonsterArena.Models;
using MonsterArena.Extensions;
using MonsterArena.Interfaces;
using UnityEngine.AI;

namespace MonsterArena
{
    public enum CauseType
    {
        None,
        Pickup,
        Die,
        Sell
    }

    [RequireComponent(typeof(HelicopterMovement))]
    public class Helicopter : MonoBehaviour, IHealthComponent
    {
        private const float _CameraSpeed = 5;
        private const float _RadiusChangingSpeedMultiplier = 15;
        
        public event Action<int, CauseType> CargoChanged = null;
        public event Action ModifiersChanged = null;
        public event Action LevelChanged = null;
        public event Action Died = null;

        [SerializeField] private HelicopterModifiers _modifiers = null;
        [SerializeField] private Resource _resourcePrefab = null;
        [SerializeField] private Missile _missilePrefab = null;
        [SerializeField] private ParticleSystem _explosionEffectPrefab = null;
        [SerializeField] private List<Renderer> _baseRenderers = new List<Renderer>();
        [SerializeField] private Renderer _missiles = null;
        [SerializeField] private Transform _rotor = null;
        [SerializeField] private Transform _leftSP = null;
        [SerializeField] private Transform _rightSP = null;
        [SerializeField] private Transform _forwardTrigger = null;
        [SerializeField] private LayerMask _castingLayerMask = default;
        [SerializeField] private Projector _shadow = null;
        [SerializeField] private float _healDuration = 1;
        [SerializeField] private float _healStep = 0.1f;
        [SerializeField] private float _radius = 4;
        [SerializeField] private CinemachineTargetGroup _cameraPosition = default;

        private readonly RaycastHit[] _lastColliders = new RaycastHit[64];
        private HelicopterMovement _movement = null;
        private Monster _target = null;
        private Vector3 _baseSize = Vector3.one;
        private Vector3 _baseRotorSize = Vector3.one;
        private bool _isLeft = false;
        private bool _isInsideHealZone = false;
        private Timer _timer = null;
        private float _healTimer = 0;
        private float _currentHealDuration = 0;
        private float _hp = 10;
        private readonly Dictionary<Monster, CinemachineTargetGroup.Target> _constraints = new Dictionary<Monster, CinemachineTargetGroup.Target>();

        public float CurrentHeight => _movement.CurrentHeight;
        public float HP => _hp / MaxHP;
        public float MaxHP => _modifiers.TransformHealth(10);
        public float Radius => _radius;
        public bool IsAlive => _hp > 0;
        public int MaxCargo => _modifiers.TransformCargo(5);
        public int Cargo { get; private set; } = 0;
        public int Level => _modifiers.CurrentLevel;
        public float CargoVisual => _modifiers.CargoVisual;
        public HelicopterModifiers Modifiers => _modifiers;

        private void Awake()
        {
            _movement = GetComponent<HelicopterMovement>();
            _movement.Initialize(this, _modifiers);

            _shadow.material.SetFloat(Constants.Radius, 0);
            _shadow.material.SetInt(Constants.CircleActive, 1);

            _baseSize = transform.localScale;
            _baseRotorSize = _rotor.localScale;

            _currentHealDuration = _healDuration;
        }

        private void Start()
        {
            _hp = MaxHP;

            OnModifiersChanged();
        }

        private void OnEnable()
        {
            _timer ??= new Timer(0.5f);
            _timer.Ticked += Shot;

            _modifiers.Changed += OnModifiersChanged;
            _modifiers.LevelChanged += OnModifiersLevelChanged;

            Cargo = PlayerPrefs.GetInt(Constants.CargoKey, 0);
        }

        private void OnDisable()
        {
            _timer.Ticked -= Shot;

            _modifiers.Changed -= OnModifiersChanged;
            _modifiers.LevelChanged -= OnModifiersLevelChanged;
        }

        private void Update()
        {
            float closest = float.MaxValue;
            _target = null;

            var center = transform.position + _movement.CurrentHeight / 4.0f * Vector3.forward;
            _forwardTrigger.position = center;
            _forwardTrigger.rotation = Quaternion.identity;
            center += _movement.CurrentHeight / 4.0f * Vector3.forward;
            var centerXZ = center.GetXZ();

            /*if (!_isInsideHealZone)
            {
                _healTimer = 0;
                _healDuration = _currentHealDuration;
            }*/

            var count = Physics.SphereCastNonAlloc(center, _radius, Vector3.down, _lastColliders, 1000, _castingLayerMask);
            for (int i = 0; i < count; i++)
            {
                var collider = _lastColliders[i].collider;
                var distance = _lastColliders[i].distance;
                var xzPos = _lastColliders[i].transform.position.GetXZ();

                if (collider.TryGetComponent(out Monster monster) && monster != this && Vector2.Distance(centerXZ, xzPos) < _radius && distance < closest)
                {
                    closest = distance;
                    _target = monster;
                }
                else if (Cargo < MaxCargo && collider.TryGetComponent(out Resource resource) && !resource.IsAttracted && Vector2.Distance(centerXZ, xzPos) < _radius / 2.5f)
                {
                    Cargo++;

                    resource.Attract(transform); 
                }
                /*else if (collider.TryGetComponent(out IZone zone) && Vector2.Distance(centerXZ, xzPos) < _radius / 4)
                {
                    /*if (zone is UpgradeZone upgrade)
                    {
                        upgrade.Enter();
                    }*/
                    /*if (Cargo > 0 && zone is MarketZone market)
                    {
                        market.Sell(Cargo);

                        Cargo = 0;

                        CargoChanged?.Invoke();
                    }
                    if (!_isInsideHealZone && zone is HealZone)
                    {
                        _healTimer.Start();
                    }
                }*/
            }

            if (IsAlive)
            {
                var hasTarget = _target != null;

                if (hasTarget && !_constraints.ContainsKey(_target))
                {
                    var constraint = new CinemachineTargetGroup.Target { target = _target.transform, weight = 0, radius = 0 };

                    _constraints.Add(_target, constraint);
                    _cameraPosition.AddMember(constraint.target, constraint.weight, constraint.radius);
                }

                _shadow.material.SetFloat(Constants.Radius, Mathf.Lerp(_shadow.material.GetFloat(Constants.Radius), hasTarget ? _radius : 0, _RadiusChangingSpeedMultiplier * Time.deltaTime));

                for (var i = 0; i < _constraints.Count; i++)
                {
                    var constraint = _constraints.ElementAt(i);
                    var source = constraint.Value;
                    var index = _cameraPosition.m_Targets.Select((s, i) => (s, i)).First(s => s.s.target == source.target).i;

                    if (constraint.Key == _target)
                    {
                        source.weight = Mathf.Lerp(source.weight, 1, _CameraSpeed * Time.deltaTime);
                    }
                    else
                    {
                        source.weight = Mathf.Lerp(source.weight, 0, _CameraSpeed * Time.deltaTime);

                        if (source.weight <= 1E-7)
                        {
                            _constraints.Remove(constraint.Key);
                            i--;

                            _cameraPosition.RemoveMember(source.target);
                            continue;
                        }
                    }

                    _constraints[constraint.Key] = source;

                    _cameraPosition.m_Targets[index] = source;
                }
            }
            
            _timer.Update(Time.deltaTime);

            if (_isInsideHealZone)
            {
                _healTimer += Time.deltaTime;

                if (_healTimer >= _healDuration)
                {
                    _healTimer = 0;
                    _healDuration /= _healStep;

                    Heal(1);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var zones = other.GetComponents<IZone>();
            
            /*if (Cargo < MaxCargo && other.TryGetComponent(out Resource resource) && !resource.IsAttracted)
            {
                Cargo++;

                resource.Attract(transform);
            }
            else */
            //if (Cargo < MaxCargo && other.TryGetComponent(out resource) && resource.IsAttracted)
            if (other.TryGetComponent(out Resource resource) && resource.IsAttracted)
            {
                if (resource.IsConsumed)
                {
                    return;
                }

                OnCargoChanged(1, CauseType.Pickup);

                resource.Consume();
            }
            else if (zones.Length > 0)
            {
                foreach (var zone in zones)
                {
                    if (_movement.AirMultiplier > 0 && zone is MenuZone upgrade)
                    {
                        upgrade.Enter();
                    }
                    if (Cargo > 0 && zone is MarketZone market)
                    {
                        market.Sell(Cargo);

                        var oldCargo = Cargo;
                        Cargo = 0;
                        
                        OnCargoChanged(-oldCargo, CauseType.Sell);
                    }
                    if (zone is HealZone)
                    {
                        _isInsideHealZone = true;

                        _healTimer = 0;
                        _healDuration = _currentHealDuration;
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out MenuZone upgrade))
            {
                upgrade.Exit();
            } 
            if (other.TryGetComponent(out HealZone _))
            {
                _isInsideHealZone = false;
            }
        }

        public void Revive()
        {
            if (IsAlive)
            {
                return;
            }

            _shadow.material.SetInt(Constants.CircleActive, 1);

            _hp = MaxHP;

            _movement.Reset();
            gameObject.SetActive(true);

            _timer.Reset();
            
            _constraints.Clear();
            _cameraPosition.m_Targets = new CinemachineTargetGroup.Target[1] { _cameraPosition.m_Targets[0] };
        }

        public void TakeDamage(float damage)
        {
            //damage = _ability.TransformReceivedDamage(damage);

            if (damage < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(damage));
            }

            _hp -= damage;

            if (!IsAlive)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            _hp = Mathf.Min(_hp + amount, MaxHP);
        }

        public void Die()
        {
            if (IsAlive)
            {
                return;
            }

            _shadow.material.SetInt(Constants.CircleActive, 0);

            _hp = 0;

            Died?.Invoke();

            SpawnResources();
            var oldCargo = Cargo;
            Cargo = 0;
            OnCargoChanged(-oldCargo, CauseType.Die);

            //_constraints.Clear();
            //_cameraPosition.m_Targets = new CinemachineTargetGroup.Target[1] { _cameraPosition.m_Targets[0] };

            Instantiate(_explosionEffectPrefab, transform.position, Quaternion.identity);
            gameObject.SetActive(false);
        }
        
        private void SpawnResources()
        {
            for (int i = 0; i < Cargo; i++)
            {
                var duration = URandom.Range(0.5f, 0.75f);
                var pos = transform.position + URandom.insideUnitCircle.AsXZ() * 3;
                NavMesh.SamplePosition(pos, out NavMeshHit hit, 3, 1);
                var targetPos = hit.position;

                if (float.IsInfinity(targetPos.x) || float.IsInfinity(targetPos.y) || float.IsInfinity(targetPos.z))
                {
                    Physics.Raycast(pos, Vector3.down, out RaycastHit hit2, 1000);
                    targetPos = hit2.point;
                }

                var resource = Instantiate(_resourcePrefab, transform.position, Quaternion.identity);
                resource.transform.DOScale(Vector3.one * URandom.Range(0.5f, 1.0f), duration).From(Vector3.zero);
                resource.transform.DOJump(targetPos, URandom.Range(1.0f, 1.5f), 1, duration);
            }
        }
        
        private void Shot()
        {
            if (_target == null)
            {
                return;
            }

            var sp = _isLeft ? _leftSP : _rightSP;
            var missile = Instantiate(_missilePrefab, sp.position, sp.rotation);
            //missile.Initialize(_target.transform.position + Vector3.up, _modifiers.TransformDamage(1));
            missile.Initialize(_target, _modifiers.TransformDamage(0.5f), _modifiers.DamageVisual);

            _isLeft = !_isLeft;
        }

        private void OnModifiersLevelChanged()
        {
            LevelChanged?.Invoke();
        }

        private void OnModifiersChanged()
        {
            foreach (var renderer in _baseRenderers)
            {
                renderer.material = _modifiers.HealthVisual;
            }
            //_renderer.material = _modifiers.HealthVisual;
            _missiles.material = _modifiers.DamageVisual;
            transform.localScale = _baseSize * (_modifiers.CargoVisual + 1);
            _rotor.localScale = _baseRotorSize * (_modifiers.SpeedVisual + 1);

            OnCargoChanged(0, CauseType.None);

            ModifiersChanged?.Invoke();
        }

        private void OnCargoChanged(int difference, CauseType cause)
        {
            CargoChanged?.Invoke(difference, cause);
            
            PlayerPrefs.SetInt(Constants.CargoKey, Cargo);
        }
    }
}
