using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper;
using MonsterArena.Interfaces;
using MonsterArena.Extensions;
using MonsterArena.Models;

namespace MonsterArena
{
    public class MonsterMovement : MonoBehaviour
    {
        private const float _RotationSpeedMultiplier = 15;

        [SerializeField] private InterfaceReference<IInput> _input = null;
        [SerializeField] private MonsterInformation _information = null;
        [SerializeField] private Rigidbody _rigidbody = null;

        public void Initialize(Monster monster)
        {
            _information = monster.Information;
            _rigidbody = monster.Rigidbody;

            _rigidbody.isKinematic = false;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void FixedUpdate()
        {
            if (Mathf.Approximately(_input.Value.Direction.magnitude, 0))
            {
                return;
            }

            var direction = _input.Value.Direction.AsXZ();

            _rigidbody.MoveRotation(Quaternion.Lerp(_rigidbody.rotation, Quaternion.LookRotation(direction, Vector3.up), _RotationSpeedMultiplier * Time.deltaTime));
            _rigidbody.MovePosition(_rigidbody.position + _information.MovementSpeed * Time.deltaTime * direction);
        }
    }
}
