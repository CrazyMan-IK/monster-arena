using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonsterArena.Models;
using MonsterArena.Extensions;

namespace MonsterArena
{
    [RequireComponent(typeof(Rigidbody))]
    public class MonsterAnimation : MonoBehaviour
    {
        private const string _Attack = "Attack";
        private const string _Alive = "Alive";
        private const string _Speed = "Speed";
        private const string _AttackSpeedMultiplier = "AttackSpeedMultiplier";
        private const string _Win = "Win";
        private const string _Once = "Once";
        private const string _Ability = "Ability";

        [SerializeField] private Animator _animator = null;
        [SerializeField] private MonsterAnimationEventsRepeater _attackEventRepeater = null;
        [SerializeField] private ParticleSystem _hitEffect = null;
        [SerializeField] private Transform _hitPositionTarget = null;
        [SerializeField] private float _accelerationMultiplier = 5;

        private Rigidbody _rigidbody = null;
        private MonsterInformation _information = null;

        private Vector3 _previousPosition = Vector3.zero;
        private float _delta = 0;

        public Rigidbody Rigidbody => _rigidbody;// != null ? _rigidbody : _rigidbody = GetComponent<Rigidbody>();
        public bool IsAttacking { get; set; } = false;
        public bool IsAlive { get; set; } = true;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();

            _previousPosition = _rigidbody.position;
        }

        private void OnEnable()
        {
            _attackEventRepeater.Attacked += OnAttacked;
        }

        private void OnDisable()
        {
            _attackEventRepeater.Attacked -= OnAttacked;
        }

        public void Initialize(MonsterInformation information)
        {
            _information = information;
        }

        private void Update()
        {
            _animator.SetBool(_Attack, IsAttacking);
            _animator.SetBool(_Alive, IsAlive);

            if (_rigidbody.isKinematic)
            {
                return;
            }
        }

        private void FixedUpdate()
        {
            if (_rigidbody.isKinematic || _information == null)
            {
                return;
            }

            var currentDelta = (_previousPosition - _rigidbody.position).GetXZ().magnitude;

            _delta = Mathf.Lerp(_delta, _information.MovementSpeed * Mathf.Clamp01(currentDelta * 50), _accelerationMultiplier * Time.deltaTime);

            _animator.SetFloat(_Speed, _delta);

            _previousPosition = _rigidbody.position;
        }

        public void StartAbility()
        {
            _animator.SetBool(_Ability, true);
        }

        public void StopAbility()
        {
            _animator.SetBool(_Ability, false);
        }

        public void ActivateWinAnimation(bool isOnce)
        {
            //_animator.SetBool(_Win, true);
            _animator.SetBool(_Once, isOnce);
            _animator.SetTrigger(_Win);
            enabled = false;
        }

        private void OnAttacked()
        {
            //_hitEffect.transform.position = _hitPositionTarget.position;
            //_hitEffect.Play();
        }
    }
}
