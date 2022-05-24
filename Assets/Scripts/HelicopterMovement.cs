using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using AYellowpaper;
using MonsterArena.Interfaces;
using MonsterArena.Extensions;
using MonsterArena.Models;

namespace MonsterArena
{
    [RequireComponent(typeof(Rigidbody))]
    public class HelicopterMovement : MonoBehaviour
    {
        private const float _AccelerationSpeed = 5;
        private const float _RotationSpeedMultiplier = 3;

        //[SerializeField] private Monster _monster = null;
        [SerializeField] private PositionConstraint _shadowConstraint = null;
        [SerializeField] private InterfaceReference<IInput> _input = null;
        [SerializeField] private float _targetHeight = 5;
        [SerializeField] private float _speed = 10;
        [SerializeField] private LayerMask _wallLayerMask = default;

        private Rigidbody _rigidbody = null;
        private Helicopter _helicopter = null;
        private HelicopterModifiers _modifiers = null;
        private Vector3 _basePosition = Vector3.zero;
        private Vector3 _lastDirection = Vector3.zero;
        private Quaternion _baseRotation = Quaternion.identity;
        private float _currentSpeed = 0;

        //public Monster Monster => _monster;
        public float AirMultiplier { get; private set; } = 0;
        public float CurrentHeight => _rigidbody.position.y - _basePosition.y;
        public float CargoVisual => _modifiers.CargoVisual;

        private void Awake()
        {
            /*if (_monster == null)
            {
                _monster = GetComponent<Monster>();
            }*/

            /*if (_monster != null)
            {
                Initialize(_monster);
            }*/

            _rigidbody = GetComponent<Rigidbody>();

            _basePosition = _rigidbody.position;
            _baseRotation = _rigidbody.rotation;

            _lastDirection = _rigidbody.rotation * Vector3.forward;
        }

        private void OnEnable()
        {
            if (_input != null && _input.Value != null)
            {
                _input.Value.AbilityUsed += OnAbilityUsed;
                _input.Value.PropThrowed += OnPropThrowed;
            }
        }

        private void OnDisable()
        {
            if (_input != null && _input.Value != null)
            {
                _input.Value.AbilityUsed -= OnAbilityUsed;
                _input.Value.PropThrowed -= OnPropThrowed;
            }
        }

        private void Start()
        {
            /*_monster.Rigidbody.isKinematic = false;
            _monster.Rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            _monster.Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;*/
        }

        public void Initialize(Helicopter helicopter, HelicopterModifiers modifiers)
        {
            _helicopter = helicopter;
            _modifiers = modifiers;
        }

        private void FixedUpdate()
        {
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;

            if (!_helicopter.IsAlive || _input.Value == null)
            {
                return;
            }

            var direction = _input.Value.Direction.AsXZ();
            var mag = direction.magnitude;
            
            if (Physics.Raycast(_rigidbody.position, direction, 1.8f, _wallLayerMask))
            {
                mag = 0;
            }
            
            if (mag == 0)
            {
                //OnPropThrowed();

                direction = _lastDirection;

                //return;
            }

            _currentSpeed = Mathf.Lerp(_currentSpeed, _modifiers.TransformSpeed(_speed) * mag, _AccelerationSpeed * Time.deltaTime);
            var mult = MathExtensions.CubicBezier(AirMultiplier * 1.2f - 0.2f, 0.25f, 0.1f, 0.25f, 1.0f).y;

            //_monster.Rigidbody.MoveRotation(Quaternion.Lerp(_monster.Rigidbody.rotation, Quaternion.LookRotation(direction, Vector3.up), _RotationSpeedMultiplier * Time.deltaTime));
            //_monster.Rigidbody.MovePosition(_monster.Rigidbody.position + _currentSpeed * Time.deltaTime * direction);

            var forward = (_rigidbody.rotation * Vector3.forward).GetXZ().normalized.AsXZ();
            _rigidbody.MovePosition(Vector3.Scale(_rigidbody.position, new Vector3(1, 0, 1)) + _currentSpeed * mult * Time.deltaTime * forward + Vector3.up * Mathf.Lerp(_basePosition.y, _basePosition.y + _targetHeight, mult));
            //_rigidbody.position = new Vector3(_rigidbody.position.x, Mathf.Lerp(_baseHeight, _baseHeight + _targetHeight, mult), _rigidbody.position.z);

            var offset = _shadowConstraint.translationOffset;
            offset.z = CurrentHeight / 2;
            _shadowConstraint.translationOffset = offset;

            var angle = Vector3.SignedAngle(forward, direction, Vector3.down);
            var x = Vector3.Distance(forward, direction) / Mathf.Sqrt(2);
            var y = 1 - x;
            x *= Mathf.Sign(angle);

            var t = _currentSpeed.Remap(0, _modifiers.TransformSpeed(_speed), 0, 25) * mult;

            //transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(direction, Vector3.up) * Quaternion.AngleAxis(t * y, Vector3.right) * Quaternion.AngleAxis(t * x, Vector3.forward), _RotationSpeedMultiplier * Time.deltaTime);

            _rigidbody.MoveRotation(Quaternion.Lerp(_rigidbody.rotation, Quaternion.LookRotation(direction, Vector3.up), _RotationSpeedMultiplier * mult * Time.deltaTime));
            forward = (_rigidbody.rotation * Vector3.forward).GetXZ().normalized.AsXZ();
            _rigidbody.MoveRotation(Quaternion.Lerp(_rigidbody.rotation, Quaternion.LookRotation(forward, Vector3.up) * Quaternion.AngleAxis(t * y, Vector3.right) * Quaternion.AngleAxis(t * x, Vector3.forward), _RotationSpeedMultiplier * 3 * mult * Time.deltaTime));
            //_rigidbody.MoveRotation(Quaternion.Lerp(_rigidbody.rotation, _rigidbody.rotation * Quaternion.AngleAxis(t * y, Vector3.right) * Quaternion.AngleAxis(t * x, Vector3.forward), _RotationSpeedMultiplier * 3 * mult * Time.deltaTime));

            if (mag > 0)
            {
                _lastDirection = direction;

                AirMultiplier = Mathf.Lerp(AirMultiplier, 1, mag * (1 - AirMultiplier) * Time.deltaTime);
            }
        }

        public void Reset()
        {
            _currentSpeed = 0;
            AirMultiplier = 0;

            transform.position = _basePosition;
            transform.rotation = _baseRotation;
        }

        private void OnAbilityUsed()
        {
            //_monster.UseAbility();
        }

        private void OnPropThrowed()
        {
            //_monster.ThrowProp();
        }
    }
}
