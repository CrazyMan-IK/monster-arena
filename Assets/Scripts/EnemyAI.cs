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
        private const float _RotationSpeed = 10.0f;
        
        public event Action AbilityUsed = null;
        public event Action PropThrowed = null;

        [SerializeField] private List<Transform> _waypoints = new List<Transform>();
        [SerializeField] private LayerMask _propLayerMask = default;
        [SerializeField] private LayerMask _helicopterLayerMask = default;
        [SerializeField] private float _additionalHitAngle = 90;

        private readonly Collider[] _lastColliders = new Collider[256];
        private readonly RaycastHit[] _lastHits = new RaycastHit[64];
        private Monster _monster = null;
        private Transform _lastPropTarget = null;
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

            if (_monster.HasProp && _lastPropTarget != null)// && !_lastTarget.IsAlive)
            {
                if (_time < 0.5f)
                {
                    _time += Time.deltaTime;
                    //Direction = Vector2.zero;
                    Direction = (_lastPropTarget.position - transform.position).GetXZ().Rotate(_additionalHitAngle * Mathf.Deg2Rad) / 500.0f;
                    //UnityEditor.EditorApplication.isPaused = true;
                    return;
                }
                _time = 0;
                
                var closestWaypoint = _waypoints[0];
                var closestDistance = Vector3.Distance(closestWaypoint.position, transform.position);
                for (int i = 1; i < _waypoints.Count; i++)
                {
                    var distance = Vector3.Distance(_waypoints[i].position, transform.position);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestWaypoint = _waypoints[i];
                        _currentWaypoint = i;
                    }
                }

                var path = new NavMeshPath();
                NavMesh.CalculatePath(transform.position, _waypoints[_currentWaypoint].position, 1, path);

                _path = null;
                if (path.status == NavMeshPathStatus.PathComplete)
                {
                    _path = path.corners;
                }
                _currentPropWaypoint = 0;

                _lastPropTarget = null;
            }
            
            if (_lastHeliTarget != null)
            {
                _lastHeliTarget = null;
            }

            if (!_monster.HasProp && _lastPropTarget == null)
            {
                var minDist = float.MaxValue;

                var count = Physics.OverlapSphereNonAlloc(transform.position, 100, _lastColliders, _propLayerMask);
                for (int i = 0; i < count; i++)
                {
                    var collider = _lastColliders[i];
                    var dist = Vector3.Distance(transform.position, collider.transform.position);

                    if (collider.TryGetComponent(out Prop prop) && !prop.IsFaded && !prop.IsThrowed && dist < minDist)
                    {
                        _lastPropTarget = prop.transform;
                        minDist = dist;
                    }
                }

                if (_lastPropTarget != null)
                {
                    var path = new NavMeshPath();
                    NavMesh.CalculatePath(transform.position, _lastPropTarget.position, 1, path);

                    _path = null;
                    if (path.status == NavMeshPathStatus.PathComplete)
                    {
                        _path = path.corners;
                    }
                    _currentPropWaypoint = 0;
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

            if (_lastPropTarget != null && !_monster.HasProp && _path != null)
            {
                if (_currentPropWaypoint < _path.Length)// || (_lastTarget.position - transform.position).magnitude <= 0.5f)
                {
                    var waypoint = _currentPropWaypoint == _path.Length - 1 ? _lastPropTarget.position : _path[_currentPropWaypoint];

                    var target = (waypoint - transform.position).GetXZ();
                    var mag = target.magnitude;
                    if (mag <= 1.0f)
                    {
                        _currentPropWaypoint++;
                    }

                    target.Normalize();

                    //Direction = target;
                    Direction = target;// * Mathf.Min((_lastTarget.position - transform.position).magnitude, 1);
                }
                else
                {
                    Direction = (_lastPropTarget.position - transform.position).GetXZ();
                    Direction.Normalize();
                    Direction /= 500; //new Vector2(500, -500);
                }
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
            _lastPropTarget = null;
            _lastHeliTarget = null;

            _currentWaypoint = 0;
            _currentPropWaypoint = 0;
        }
    }
}
