using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonsterArena.Models;
using MonsterArena.Extensions;

namespace MonsterArena
{
    [RequireComponent(typeof(Rigidbody))]
    //[RequireComponent(typeof(MonsterAI))]
    public class MonsterAnimation : MonoBehaviour
    {
        private const string _Attack = "Attack";
        private const string _Alive = "Alive";
        private const string _Speed = "Speed";
        private const string _AttackSpeedMultiplier = "AttackSpeedMultiplier";
        private const string _Win = "Win";
        private const string _Once = "Once";

        [SerializeField] private Animator _animator = null;
        [SerializeField] private MonsterAnimationEventsRepeater _attackEventRepeater = null;
        [SerializeField] private ParticleSystem _hitEffect = null;
        [SerializeField] private Transform _hitPositionTarget = null;
        [SerializeField] private float _accelerationMultiplier = 5;

        private Rigidbody _rigidbody = null;
        private MonsterInformation _information = null;
        //private MonsterAI _ai = null;

        private Vector3 _previousPosition = Vector3.zero;
        private float _delta = 0;

        public Rigidbody Rigidbody => _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            //_ai = GetComponent<MonsterAI>();

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
            //_animator.SetBool(_Attack, _ai.IsAttacking);
            //_animator.SetBool(_Alive, _ai.IsAlive);

            if (_rigidbody.isKinematic)
            {
                return;
            }
            //_animator.SetFloat(_Speed, (_previousPosition - _rigidbody.position).GetXZ().magnitude * 100);

            //Debug.Log($"U = RB_Pos: {_rigidbody.position}\nPrev_Pos: {_previousPosition}\n{(_previousPosition - _rigidbody.position).GetXZ().magnitude}\n{_information.MovementSpeed * Time.deltaTime}");
            //_previousPosition = _rigidbody.position;
        }

        private void FixedUpdate()
        {
            if (_rigidbody.isKinematic || _information == null)
            {
                return;
            }

            var currentDelta = (_previousPosition - _rigidbody.position).GetXZ().magnitude;

            //_delta = Mathf.Lerp(_delta, (_previousPosition - _rigidbody.position).GetXZ().magnitude * 100, _accelerationMultiplier * Time.deltaTime);
            //Debug.Log($"FU = RB_Pos: {_rigidbody.position}\nPrev_Pos: {_previousPosition}\n{(_previousPosition - _rigidbody.position).GetXZ().magnitude}\n{_information.MovementSpeed * Time.deltaTime}");
            //Debug.Log($"{_rigidbody.velocity.magnitude}\n{currentDelta}");
            //var currentDelta = (_previousPosition - _rigidbody.position).GetXZ().magnitude * 50 / _information.MovementSpeed;
            _delta = Mathf.Lerp(_delta, _information.MovementSpeed * (Mathf.Approximately(currentDelta, 0) ? 0 : 1), _accelerationMultiplier * Time.deltaTime);

            _animator.SetFloat(_Speed, _delta);

            _previousPosition = _rigidbody.position;
        }

        public void EnableAnimator()
        {
            _animator.enabled = true;
        }

        public void DisableAnimator()
        {
            _animator.enabled = false;
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
            _hitEffect.transform.position = _hitPositionTarget.position;
            _hitEffect.Play();
        }
    }
}
