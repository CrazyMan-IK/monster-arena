using System.Collections;
using System.Collections.Generic;
using MonsterArena.Interfaces;
using UnityEngine;

namespace MonsterArena
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Renderer))]
    [RequireComponent(typeof(Collider))]
    public class Missile : MonoBehaviour
    {
        [SerializeField] private float _maxSpeed = 10;
        [SerializeField] private float _rotationMultiplier = 10;
        [SerializeField] private float _acceleration = 5;
        [SerializeField] private ParticleSystem _flameEffect = null;
        [SerializeField] private ParticleSystem _destroyEffect = null;
        [SerializeField] private LayerMask _arenaLayer = default;

        private Rigidbody _rigidbody = null;
        private Renderer _renderer = null;
        private Collider _collider = null;
        //private Vector3 _targetPosition = Vector3.zero;
        private IHealthComponent _target = null;
        private float _currentSpeed = 0;
        private float _damage = 0;
        private Timer _timer = null;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _renderer = GetComponent<Renderer>();
            _collider = GetComponent<Collider>();
        }

        private void OnEnable()
        {
            _timer ??= new Timer(3);
            _timer.Ticked += Explode;
        }

        private void OnDisable()
        {
            _timer.Ticked -= Explode;
        }

        /*public void Initialize(Vector3 targetPosition, float damage)
        {
            _targetPosition = targetPosition;
            _damage = damage;
        }*/

        public void Initialize(IHealthComponent target, float damage, Material material)
        {
            _target = target;
            _damage = damage;
            _renderer.material = material;
        }

        private void FixedUpdate()
        {
            if (!_target.IsAlive && _collider.enabled)
            {
                Explode();
                
                return;
            }

            _currentSpeed = Mathf.Lerp(_currentSpeed, _maxSpeed, _acceleration * Time.deltaTime);

            if (_collider.enabled)
            {
                _rigidbody.MoveRotation(Quaternion.RotateTowards(_rigidbody.rotation, Quaternion.LookRotation(_target.gameObject.transform.position + Vector3.up - transform.position), _currentSpeed * _rotationMultiplier * Time.deltaTime));
            }
            _rigidbody.MovePosition(_rigidbody.position + _currentSpeed * Time.deltaTime * (_rigidbody.rotation * Vector3.forward));

            _timer.Update(Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Monster monster) && monster.IsAlive)
            {
                Explode();

                monster.TakeDamage(_damage, DamageSource.Player);
            }
            else if (other.gameObject.layer == _arenaLayer)
            {
                Explode();
            }
        }

        private void Explode()
        {
            Instantiate(_destroyEffect, _rigidbody.position, Quaternion.identity);
            _flameEffect.Stop();
            _collider.enabled = false;
            Destroy(gameObject, 1.2f);
        }
    }
}
