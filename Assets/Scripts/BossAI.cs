using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using URandom = UnityEngine.Random;
using MonsterArena.Interfaces;
using MonsterArena.Extensions;
using UnityEngine.AI;
using System.Linq;

namespace MonsterArena
{
    [RequireComponent(typeof(Monster))]
    public class BossAI : MonoBehaviour, IInput
    {
        private const float _RotationSpeed = 10.0f;
        
        public event Action AbilityUsed = null;
        public event Action PropThrowed = null;

        [SerializeField] private LayerMask _propLayerMask = default;
        [SerializeField] private LayerMask _helicopterLayerMask = default;

        private readonly Collider[] _lastColliders = new Collider[256];
        private readonly RaycastHit[] _lastHits = new RaycastHit[64];
        private Monster _monster = null;
        private Transform _lastPropTarget = null;
        private Transform _lastHeliTarget = null;
        private Vector3 _basePosition = Vector3.zero;
        private bool _isLocked = false;

        public Vector2 Direction { get; private set; }

        private void Awake()
        {
            _monster = GetComponent<Monster>();

            _basePosition = transform.position;
        }

        private void OnEnable()
        {
            _monster.Died += OnDied;
        }

        private void OnDisable()
        {
            _monster.Died -= OnDied;
        }

        private void Update()
        {
            if (_isLocked)
            {
                return;
            }
            
            if (_lastHeliTarget != null)
            {
                _lastHeliTarget = null;
            }

            if (_monster.HasProp)
            {
                _lastPropTarget = null;
            }

            if (!_monster.HasProp && _lastPropTarget == null)
            {
                var minDist = float.MaxValue;

                var count = Physics.OverlapSphereNonAlloc(_basePosition, 100, _lastColliders, _propLayerMask);
                for (int i = 0; i < count; i++)
                {
                    var collider = _lastColliders[i];
                    var dist = Vector3.Distance(_basePosition, collider.transform.position);

                    if (collider.TryGetComponent(out Prop prop) && !prop.IsFaded && !prop.IsThrowed && dist < minDist)
                    {
                        _lastPropTarget = prop.transform;
                        minDist = dist;
                    }
                }
            }
            else
            {
                var center = transform.position + _monster.Helicopter.CurrentHeight / 2.0f * Vector3.back;
                var centerXZ = center.GetXZ();

                var count = Physics.SphereCastNonAlloc(center, 10, Vector3.up, _lastHits, 1000, _helicopterLayerMask);
                for (int i = 0; i < count; i++)
                {
                    var collider = _lastHits[i].collider;
                    var xzPos = collider.transform.position.GetXZ();

                    if (collider.TryGetComponent(out Helicopter helicopter) && helicopter.IsAlive && Vector2.Distance(centerXZ, xzPos) < 10)
                    {
                        _lastHeliTarget = helicopter.transform;
                        break;
                    }
                }
            }

            if (_lastPropTarget != null && !_monster.HasProp)
            {
                var waypoint = _lastPropTarget.position;

                var target = (waypoint - transform.position).GetXZ();

                target.Normalize();

                //Direction = target;
                Direction = target;
            }
            else if (_lastHeliTarget != null)
            {
                var center = _monster.Helicopter.transform.position + _monster.Helicopter.CurrentHeight / 2.0f * Vector3.forward;
                //var target = (_lastTarget.transform.position - transform.position).GetXZ();
                var target = (center - transform.position).GetXZ();

                //Direction = Vector2.Lerp(Direction, target.magnitude > _monster.AttackArea / 1.25f ? (target / Mathf.Max(target.magnitude, _monster.AttackArea)) : (target.normalized / 500), _RotationSpeed * Time.deltaTime);
                Direction = Vector2.Lerp(Direction, target.normalized / 500, _RotationSpeed * Time.deltaTime);
                //Direction = target.magnitude > _monster.AttackArea / 1.25f ? (target / Mathf.Max(target.magnitude, _monster.AttackArea)) : (target.normalized / 500);
                //Direction = (target / Mathf.Max(target.magnitude, 5)).GetXZ();

                /*if (target.magnitude < _monster.Information.ViewArea)
                {
                    PropThrowed?.Invoke();
                }*/
            }
            else
            {
                var target = (_basePosition - transform.position).GetXZ();

                Direction = target / Mathf.Max(target.magnitude, 1);
            }

            PropThrowed?.Invoke();
        }
        
        public void Lock()
        {
            _isLocked = true;
        }

        private void OnDied(Monster monster, DamageSource source)
        {
            _lastPropTarget = null;
            _lastHeliTarget = null;
        }
    }
}
