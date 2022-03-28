using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using URandom = UnityEngine.Random;
using MonsterArena.Interfaces;
using MonsterArena.Extensions;

namespace MonsterArena
{
    [RequireComponent(typeof(Monster))]
    [RequireComponent(typeof(Target))]
    public class EnemyAI : MonoBehaviour, IInput
    {
        public event Action AbilityUsed = null;

        [SerializeField] private List<Transform> _waypoints = new List<Transform>();
        [SerializeField] private LayerMask _monstersLayerMask = default;

        private readonly Collider[] _lastColliders = new Collider[8];
        private Monster _monster = null;
        private Target _target = null;
        private Monster _lastTarget = null;
        private int _currentWaypoint = 0;

        public Vector2 Direction { get; private set; }

        private void Awake()
        {
            _monster = GetComponent<Monster>();
            _target = GetComponent<Target>();

            _target.TargetColor = URandom.ColorHSV(0, 1, 0.8f, 1, 0.8f, 1, 1, 1);
        }

        private void Update()
        {
            _target.enabled = _monster.IsAlive;
            if (_target.indicator != null)
            {
                _target.indicator.Activate(_monster.IsAlive);
            }

            if (_lastTarget != null && !_lastTarget.IsAlive)
            {
                _lastTarget = null;
            }

            if (_lastTarget == null)
            {
                var count = Physics.OverlapSphereNonAlloc(transform.position, _monster.Information.ViewArea, _lastColliders, _monstersLayerMask);
                for (int i = 0; i < count; i++)
                {
                    var collider = _lastColliders[i];

                    if (collider.TryGetComponent(out Monster monster) && monster != _monster)
                    {
                        _lastTarget = monster;
                        break;
                    }
                }
            }

            if (_lastTarget != null)
            {
                var target = (_lastTarget.transform.position - transform.position).GetXZ();

                Direction = target.magnitude > _monster.AttackArea / 1.25f ? (target / Mathf.Max(target.magnitude, _monster.AttackArea)) : (target.normalized / 500);
                //Direction = (target / Mathf.Max(target.magnitude, 5)).GetXZ();
            }
            else
            {
                var waypoint = _waypoints[_currentWaypoint];

                var target = (waypoint.position - transform.position).GetXZ();
                if (target.magnitude <= 1)
                {
                    _currentWaypoint++;

                    if (_currentWaypoint == _waypoints.Count)
                    {
                        _currentWaypoint = 0;
                    }
                }

                Direction = target / Mathf.Max(target.magnitude, 1);
            }
        }

        private void OnDrawGizmos()
        {
            for (int i = 0; i < _waypoints.Count; i++)
            {
                var waypoint = _waypoints[i];

                if (i < _waypoints.Count - 1)
                {
                    Gizmos.DrawLine(waypoint.position + Vector3.up, _waypoints[i + 1].position + Vector3.up);

                    continue;
                }

                Gizmos.DrawLine(waypoint.position + Vector3.up, _waypoints[0].position + Vector3.up);
            }

            for (int i = 0; i < _waypoints.Count; i++)
            {
                var waypoint = _waypoints[i];

                Gizmos.DrawSphere(waypoint.position + Vector3.up, 0.1f);
            }
        }
    }
}
