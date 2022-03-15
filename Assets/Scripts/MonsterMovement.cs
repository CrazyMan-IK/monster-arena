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
        private const float _RotationSpeedMultiplier = 15;

        [SerializeField] private Monster _monster = null;
        [SerializeField] private InterfaceReference<IInput> _input = null;

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

        private void FixedUpdate()
        {
            if (!_monster.IsAlive || _input.Value == null || _input.Value.Direction == Vector2.zero)
            {
                return;
            }

            var direction = _input.Value.Direction.AsXZ();

            _monster.Rigidbody.MoveRotation(Quaternion.Lerp(_monster.Rigidbody.rotation, Quaternion.LookRotation(direction, Vector3.up), _RotationSpeedMultiplier * Time.deltaTime));
            _monster.Rigidbody.MovePosition(_monster.Rigidbody.position + _monster.Information.MovementSpeed * Time.deltaTime * direction);
        }
    }
}
