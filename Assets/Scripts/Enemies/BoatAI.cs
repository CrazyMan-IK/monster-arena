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
    public class BoatAI : MonoBehaviour, IInput
    {
        private const float _RotationSpeed = 5.0f;

        public event Action AbilityUsed = null;
        public event Action PropThrowed = null;

        [SerializeField] private List<Transform> _waypoints = new List<Transform>();
        [SerializeField] private LayerMask _propLayerMask = default;
        [SerializeField] private LayerMask _helicopterLayerMask = default;
        [SerializeField] private float _additionalHitAngle = 90;
        [SerializeField] private float _radius;

        private readonly Collider[] _lastColliders = new Collider[256];
        private readonly RaycastHit[] _lastHits = new RaycastHit[64];
        private Monster _monster = null;
        private Transform _lastHeliTarget = null;
        private Vector3[] _path = null;
        private int _currentPropWaypoint = 0;
        private int _currentWaypoint = 0;
        private float _time = 0;
        private bool _isLocked = false;

        public Vector2 Direction { get; private set; }

        private void Awake()
        {
            _monster = GetComponent<Monster>();
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

            if (!_monster.IsAlive)
            {
                _currentWaypoint = 0;
            }
            
            if (_lastHeliTarget != null)
            {
                _lastHeliTarget = null;
            }

            var origin = transform.position + _monster.Helicopter.CurrentHeight / 2.0f * Vector3.back;
            var centerXZ = origin.GetXZ();

            var count = Physics.SphereCastNonAlloc(origin, 10, Vector3.up, _lastHits, 1000, _helicopterLayerMask);
            for (int i = 0; i < count; i++)
            {
                var collider = _lastHits[i].collider;
                var xzPos = collider.transform.position.GetXZ();

                if (collider.TryGetComponent(out Helicopter helicopter) && helicopter.IsAlive &&
                    Vector2.Distance(centerXZ, xzPos) < 10)
                {
                    _lastHeliTarget = helicopter.transform;
                    break;
                }
            }

            if (_lastHeliTarget != null)
            {
                if(_monster.IsThrowing)
                    return;
                
                var center = _monster.Helicopter.transform.position/* +
                             _monster.Helicopter.CurrentHeight / 2.0f * Vector3.forward*/;
                var target = (center - transform.position).GetXZ();
                Direction = Vector2.Lerp(Direction, target.normalized / 500, _RotationSpeed * Time.deltaTime);
            }
            else
            {
                float time = Time.time / _radius;
                Direction = new Vector2(Mathf.Cos(time), Mathf.Sin(time));
            }

            PropThrowed?.Invoke();
        }

        public void Lock()
        {
            _isLocked = true;
        }

        private void OnDied(Monster monster, DamageSource source)
        {
            _path = null;
            _lastHeliTarget = null;

            _currentWaypoint = 0;
            _currentPropWaypoint = 0;
        }
    }
}