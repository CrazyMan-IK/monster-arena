using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonsterArena.Models;
using DG.Tweening;

namespace MonsterArena
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class MonsterAI : MonoBehaviour
    {
        private const float _AccelerationSpeed = 3;
        private const float _RotationSpeed = 3;
        private const float _AttackDistance = 2.4f;

        public event Action Died = null;

        [SerializeField] private Projector _shadow = null;

        private Rigidbody _rigidbody = null;
        private Collider _collider = null;
        private MonsterAI _target = null;

        private LayerMask _enemyLayerMask = default;
        private MonsterInformation _information = null;

        private float _movementSpeed = 0;
        private float _hp = 0;

        public bool IsAttacking { get; private set; }
        public bool IsAlive => _hp > 0;
        public float AttackSpeedMultiplier => _information.AttackSpeed;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
        }

        private void Update()
        {
            if (_target == null)
            {
                var colliders = Physics.OverlapSphere(transform.position, 20, _enemyLayerMask);

                foreach (var collider in colliders)
                {
                    if (collider.TryGetComponent(out MonsterAI other) && other.enabled && other.IsAlive && other != this)
                    {
                        _target = other;
                        _target.Died += OnTargetDied;
                    }
                }

                IsAttacking = false;
                return;
            }

            var forward = _rigidbody.rotation * Vector3.forward;
            var angle = Quaternion.AngleAxis(_RotationSpeed * Time.deltaTime * Vector3.SignedAngle(forward, _target._rigidbody.position - _rigidbody.position, Vector3.up), Vector3.up);

            _rigidbody.MoveRotation(_rigidbody.rotation * angle);

            IsAttacking = Vector3.Distance(_rigidbody.position, _target._rigidbody.position) <= _AttackDistance;
            if (IsAttacking)
            {
                _movementSpeed = Mathf.Lerp(_movementSpeed, 0, _AccelerationSpeed * Time.deltaTime);
            }
            else
            {
                _movementSpeed = Mathf.Lerp(_movementSpeed, _information.MovementSpeed, _AccelerationSpeed * Time.deltaTime);
            }

            _rigidbody.MovePosition(_rigidbody.position + _movementSpeed * Time.deltaTime * forward);
        }

        public void Initialize(LayerMask enemyLayerMask, MonsterInformation information)
        {
            _enemyLayerMask = enemyLayerMask;
            _information = information;

            _hp = information.HP;
        }

        public void Attack()
        {
            //_target.TakeDamage(_information.Damage);
            if (!IsAlive)
            {
                return;
            }

            var pos = transform.forward * _AttackDistance;
            if (_target != null)
            {
                pos = _target.transform.position;
            }

            var colliders = Physics.OverlapSphere(pos, _information.AttackArea, _enemyLayerMask);
            foreach (var collider in colliders)
            {
                if (collider.TryGetComponent(out MonsterAI other) && other.enabled && other.IsAlive && other != this)
                {
                    other.TakeDamage(_information.Damage);
                }
            }
        }

        public void HideBody()
        {
            transform.DOLocalMoveY(-1.5f, 10).SetRelative();
            transform.DOScale(0.1f, 10);
            _shadow.transform.DOLocalMove(Vector3.zero, 10);
        }

        private void TakeDamage(float damage)
        {
            if (!enabled)
            {
                return;
            }

            if (damage <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(damage));
            }

            _hp -= damage;
            if (!IsAlive)
            {
                Died?.Invoke();
                enabled = false;
                _collider.enabled = false;
                _rigidbody.isKinematic = true;
            }
        }

        private void OnTargetDied()
        {
            if (_target == null)
            {
                return;
            }

            _target.Died -= OnTargetDied;

            _target = null;
        }
    }
}
