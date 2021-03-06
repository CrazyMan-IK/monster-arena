using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonsterArena.Models;
using MonsterArena.Extensions;
using DG.Tweening;

namespace MonsterArena
{
    [RequireComponent(typeof(Rigidbody))]
    public class MonsterAnimation : MonoBehaviour
    {
        [SerializeField] private Animator _animator = null;
        [SerializeField] private AnimationClip _attackClip = null;
        [SerializeField] private MonsterAnimationEventsRepeater _attackEventRepeater = null;
        [SerializeField] private ParticleSystem _hitEffect = null;
        [SerializeField] private Transform _hitPositionTarget = null;
        [SerializeField] private float _accelerationMultiplier = 5;

        private Rigidbody _rigidbody = null;
        private MonsterInformation _information = null;

        private Vector3 _previousPosition = Vector3.zero;
        private float _delta = 0;
        private float _attackSpeedMultiplier = 1;

        public Rigidbody Rigidbody => _rigidbody;// != null ? _rigidbody : _rigidbody = GetComponent<Rigidbody>();
        public bool IsAttacking { get; set; } = false;
        public bool IsThrowing { get; set; } = false;
        public bool IsAlive { get; set; } = true;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();

            _previousPosition = _rigidbody.position;
        }

        private void OnEnable()
        {
            _attackEventRepeater.Attacked += OnAttacked;
            _attackEventRepeater.Died += OnDied;
        }

        private void OnDisable()
        {
            _attackEventRepeater.Attacked -= OnAttacked;
            _attackEventRepeater.Died -= OnDied;
        }

        public void Initialize(MonsterInformation information, float attackSpeedMultiplier)
        {
            _information = information;

            _attackSpeedMultiplier = attackSpeedMultiplier;
        }

        private void Update()
        {
            _animator.SetBool(Constants.Attack, IsAttacking || IsThrowing);
            _animator.SetFloat(Constants.AttackSpeedMultiplier, _attackSpeedMultiplier); //IsThrowing || !IsAttacking ? 1 : 2);
            _animator.SetBool(Constants.Alive, IsAlive);

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

            _delta = Mathf.Lerp(_delta, _information.MovementSpeed * Mathf.Clamp01(currentDelta * 10), _accelerationMultiplier * Time.deltaTime);

            _animator.SetFloat(Constants.Speed, _delta);

            _previousPosition = _rigidbody.position;
        }

        public float GetAttackDuration()
        {
            return _attackClip.length / _attackSpeedMultiplier;
        }

        public float GetFirstAttackTime()
        {
            return _attackClip.events[0].time;
        }

        public float GetCurrentTime()
        {
            var state = _animator.GetCurrentAnimatorStateInfo(1);
            //var transition = _animator.GetAnimatorTransitionInfo(1);
            var nextState = _animator.GetNextAnimatorStateInfo(1);

            state = state.IsName(Constants.Attack) ? state : nextState;

            //return state.IsName(_Attack) ? state.normalizedTime * GetFirstAttackTime() : 0;
            //return state.IsName(_Attack) ? state.normalizedTime * state.length : 0;
            return state.IsName(Constants.Attack) ? state.normalizedTime * state.length * state.speed * state.speedMultiplier : 0;
            //return state.IsName(_Attack) ? state.normalizedTime : 0;
        }
        
        public void StartAbility(float speed = 1.0f)
        {
            _animator.SetBool(Constants.Ability, true);
            _animator.SetFloat(Constants.AbilitySpeedMultiplier, speed);
        }

        public void StopAbility()
        {
            _animator.SetBool(Constants.Ability, false);
        }

        public void ActivateWinAnimation(bool isOnce)
        {
            //_animator.SetBool(_Win, true);
            _animator.SetBool(Constants.Once, isOnce);
            _animator.SetTrigger(Constants.Win);
            _animator.SetTrigger(Constants.Win);
            _animator.SetTrigger(Constants.Win);
            enabled = false;
        }

        private void OnAttacked()
        {
            //_hitEffect.transform.position = _hitPositionTarget.position;
            //_hitEffect.Play();
        }

        private void OnDied()
        {
            transform.DOLocalMove(Vector3.down * 1.5f, 3.0f).SetRelative();
        }
    }
}
