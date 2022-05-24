using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterArena
{
    [RequireComponent(typeof(Rigidbody))]
    public class Resource : MonoBehaviour
    {
        private const float _SpeedMultiplier = 50.0f;
        private const float _SpeedAccelerationMultiplier = 25.0f;

        private Transform _attractTarget = null;
        private Rigidbody _rigidbody = null;
        private float _speed = 0;

        public bool IsAttracted => _attractTarget != null;
        public bool IsConsumed { get; private set; } = false;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (_attractTarget == null)
            {
                return;
            }

            _speed = Mathf.Lerp(_speed, _SpeedMultiplier, _SpeedAccelerationMultiplier * Time.deltaTime);

            _rigidbody.MovePosition(Vector3.MoveTowards(_rigidbody.position, _attractTarget.position, _speed * Time.deltaTime));
        }

        public void Attract(Transform target)
        {
            if (_attractTarget != null)
            {
                return;
            }

            _attractTarget = target;
        }
        
        public void Consume()
        {
            IsConsumed = true;

            Destroy(gameObject);
        }
    }
}
