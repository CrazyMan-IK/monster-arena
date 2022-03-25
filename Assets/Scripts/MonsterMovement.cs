using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper;
using MonsterArena.Interfaces;
using MonsterArena.Extensions;
using MonsterArena.Models;

namespace MonsterArena
{
    [RequireComponent(typeof(Monster))]
    public class MonsterMovement : MonoBehaviour
    {
        private const float _AccelerationSpeed = 5;
        private const float _RotationSpeedMultiplier = 15;

        [SerializeField] private Monster _monster = null;
        [SerializeField] private InterfaceReference<IInput> _input = null;

        private float _speedFactor = 1;
        private float _currentSpeed = 0;

        public Monster Monster => _monster;

        private void Awake()
        {
            if (_monster == null)
            {
                _monster = GetComponent<Monster>();
            }

            /*if (_monster != null)
            {
                Initialize(_monster);
            }*/
        }

        private void Start()
        {
            _monster.Rigidbody.isKinematic = false;
            _monster.Rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            _monster.Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }

        public void Initialize(IInput input)
        {
            //_input.Value = input;
            _input = new InterfaceReference<IInput>() { Value = input };

            /*if (_monster == null)
            {
                _information = monster.Information;
                _rigidbody = monster.Rigidbody;
            }

            _rigidbody.isKinematic = false;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;*/
        }

        private void Update()
        {
            _speedFactor = 1;
        }

        private void FixedUpdate()
        {
            if (!_monster.IsAlive || _input.Value == null || _input.Value.Direction == Vector2.zero)
            {
                return;
            }

            var direction = _input.Value.Direction.AsXZ();

            _currentSpeed = Mathf.Lerp(_currentSpeed, _monster.MovementSpeed * _speedFactor, _AccelerationSpeed * Time.deltaTime);

            _monster.Rigidbody.MoveRotation(Quaternion.Lerp(_monster.Rigidbody.rotation, Quaternion.LookRotation(direction, Vector3.up), _RotationSpeedMultiplier * Time.deltaTime));
            _monster.Rigidbody.MovePosition(_monster.Rigidbody.position + _currentSpeed * Time.deltaTime * direction);
        }

        private void LateUpdate()
        {
            _speedFactor = 1;
        }

        public void ApplySpeedFactor(float factor)
        {
            _speedFactor = factor;
        }
    }
}
