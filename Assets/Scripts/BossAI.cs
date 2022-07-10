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

        //private Transform _lastPropTarget = null;
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
                
                var center = _monster.Helicopter.transform.position +
                             _monster.Helicopter.CurrentHeight / 2.0f * Vector3.forward;
                var target = (center - transform.position).GetXZ();

                Direction = Vector2.Lerp(Direction, target.normalized / 500, _RotationSpeed * Time.deltaTime);
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
            _lastHeliTarget = null;
        }
    }
}