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
    public class EnemyAI : MonoBehaviour, IInput
    {
        private const float _RotationSpeed = 5.0f;

        public event Action AbilityUsed = null;
        public event Action PropThrowed = null;

        [SerializeField] private List<Transform> _waypoints = new List<Transform>();
        [SerializeField] private LayerMask _propLayerMask = default;
        [SerializeField] private LayerMask _helicopterLayerMask = default;
        [SerializeField] private float _additionalHitAngle = 90;

        private readonly Collider[] _lastColliders = new Collider[256];
        private readonly RaycastHit[] _lastHits = new RaycastHit[64];
        private Monster _monster = null;
        //private Transform _lastPropTarget = null;
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

            for (int i = 0; i < _waypoints.Count; i++)
            {
                var current = _waypoints[i];
                var next = i == _waypoints.Count - 1 ? _waypoints[0] : _waypoints[i + 1];

                for (int j = 1; j < 7; j++, i++)
                {
                    var go = new GameObject("Waypoint");

                    go.transform.parent = current.parent;
                    go.transform.position = Vector3.Lerp(current.position, next.position, j / 8.0f);

                    _waypoints.Insert(i + 1, go.transform);
                }
            }
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
                if(_monster.Attacking)
                    return;
                
                var center = _monster.Helicopter.transform.position/* +
                             _monster.Helicopter.CurrentHeight / 2.0f * Vector3.forward*/;
                var target = (center - transform.position).GetXZ();
                Direction = Vector2.Lerp(Direction, target.normalized / 500, _RotationSpeed * Time.deltaTime);
            }
            else
            {
                var waypoints = _waypoints.Select(x => x.position).ToArray();
                ref var currentWaypoint = ref _currentWaypoint;
                if (_path != null)
                {
                    waypoints = _path;
                    currentWaypoint = ref _currentPropWaypoint;
                }

                var waypoint = waypoints[currentWaypoint];

                var target = (waypoint - transform.position).GetXZ();
                if (target.magnitude <= 1)
                {
                    currentWaypoint++;

                    if (currentWaypoint == waypoints.Length)
                    {
                        _path = null;
                        currentWaypoint = 0;
                    }
                }

                Direction = target / Mathf.Max(target.magnitude, 1);
            }

            PropThrowed?.Invoke();
        }

        private void OnDrawGizmos()
        {
            DrawWay(_waypoints.Select(x => x.position), true, Color.white);
            DrawWay(_path, false, Color.black);
        }

        private void DrawWay(IEnumerable<Vector3> waypoints, bool isLooped, Color baseColor)
        {
            var count = waypoints?.Count() ?? 0;

            if (count < 2)
            {
                return;
            }

#if UNITY_EDITOR
            if (UnityEditor.Selection.activeGameObject == gameObject)
            {
                baseColor = Color.red;
            }
#endif

            Gizmos.color = baseColor;

            for (int i = 0; i < count; i++)
            {
                var waypoint = waypoints.ElementAt(i);

                if (i < count - 1)
                {
                    Gizmos.DrawLine(waypoint + Vector3.up, waypoints.ElementAt(i + 1) + Vector3.up);

                    continue;
                }

                if (isLooped)
                {
                    Gizmos.DrawLine(waypoint + Vector3.up, waypoints.First() + Vector3.up);
                }
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(waypoints.ElementAt(0) + Vector3.up, (waypoints.Last() - waypoints.First()).normalized);

            for (int i = 0; i < count; i++)
            {
                var waypoint = waypoints.ElementAt(i);

                Gizmos.color = i == 0 ? Color.green : baseColor;
                Gizmos.DrawSphere(waypoint + Vector3.up, 0.1f);
            }
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