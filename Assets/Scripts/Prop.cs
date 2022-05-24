using System.Collections;
using System.Collections.Generic;
using MonsterArena.Extensions;
using UnityEngine;
using RayFire;

namespace MonsterArena
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(MeshCollider))]
    [RequireComponent(typeof(BoxCollider))]
    public class Prop : MonoBehaviour
    {
        private const float _SpeedMultiplier = 14.14f;

        [SerializeField] private bool _canAutoFade = true;

        private RayfireRigid _rayfireRigidbody = null;
        private Rigidbody _rigidbody = null;
        private MeshCollider _collider = null;
        private BoxCollider _trigger = null;
        private Vector3 _direction = Vector3.forward;
        private float _damage = 0;
        private Timer _timeoutTimer = null;

        [field: SerializeField] public bool IsBase { get; private set; } = false;
        public bool IsThrowed { get; private set; } = false;
        public bool IsFaded { get; private set; } = false;
        public Transform ThrowingParent { get; private set; } = null;
        public Vector3 Direction => _direction;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<MeshCollider>();
            _trigger = GetComponent<BoxCollider>();
        }

        private void Start()
        {
            _rayfireRigidbody = GetComponent<RayfireRigid>();

            if (_canAutoFade && !IsBase)
            {
                _timeoutTimer.Start();
            }
        }

        private void OnEnable()
        {
            _timeoutTimer ??= new Timer(90);
            _timeoutTimer.Ticked += OnTimeout;

            _timeoutTimer.Stop();
        }

        private void OnDisable()
        {
            _timeoutTimer.Ticked -= OnTimeout;
        }

        private void Update()
        {
            _timeoutTimer.Update(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (!IsThrowed)
            {
                return;
            }

            _rigidbody.MovePosition(_rigidbody.position + _SpeedMultiplier * Time.deltaTime * _direction);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsThrowed && other.TryGetComponent(out Helicopter helicopter))
            {
                Destroy(gameObject);

                helicopter.TakeDamage(_damage);
            }
        }

        public void Take(Transform parent)
        {
            _timeoutTimer.Stop();

            //_rigidbody.isKinematic = true;
            if (_rayfireRigidbody != null)
            {
                _rayfireRigidbody.simulationType = SimType.Kinematic;
                _rayfireRigidbody.ResetRigid();
            }
            else
            {
                _rigidbody.isKinematic = true;
                _rigidbody.interpolation = RigidbodyInterpolation.None;
            }

            transform.parent = parent;
            transform.position = parent.position;

            _collider.enabled = false;
            _trigger.enabled = false;
        }

        public void Throw(Transform parent, Vector3 targetPosition, float damage)
        {
            if (IsThrowed)
            {
                return;
            }
            
            _damage = damage;

            ThrowingParent = parent;

            transform.parent = null;
            IsThrowed = true;

            _direction = targetPosition - transform.position;//Vector3.Scale(targetPosition - transform.position, Vector3.right + Vector3.forward);
            _direction.Normalize();

            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

            _collider.enabled = true;
            _trigger.enabled = true;
        }

        public void Demolish(Vector3 position)
        {
            //transform.position = position;

            if (_rayfireRigidbody != null)
            {
                _rayfireRigidbody.Demolish();
            }

            Invoke(nameof(ResetState), 60);
        }

        private void ResetState()
        {
            if (_rayfireRigidbody != null)
            {
                _rayfireRigidbody.ResetRigid();
            }
        }

        private void OnTimeout()
        {
            IsFaded = true;

            _trigger.enabled = false;

            _rayfireRigidbody.Fade();
        }
    }
}
