using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterArena
{
    [RequireComponent(typeof(TrailRenderer))]
    [ExecuteAlways]
    public class SpeedBasedTrail : MonoBehaviour
    {
        [SerializeField] private float _minimalSpeed = 0.1f;

        private TrailRenderer _trail = null;
        private Vector3 _previousPosition = Vector3.zero;

        private void Awake()
        {
            _trail = GetComponent<TrailRenderer>();

            _previousPosition = transform.position;
        }

        private void Update()
        {
            _trail.emitting = Vector3.Distance(_previousPosition, transform.position) > _minimalSpeed;

            _previousPosition = transform.position;
        }
    }
}
