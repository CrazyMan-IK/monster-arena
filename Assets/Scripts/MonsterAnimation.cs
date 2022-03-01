using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonsterArena.Extensions;

namespace MonsterArena
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(MonsterAI))]
    public class MonsterAnimation : MonoBehaviour
    {
        private const string _Attack = "Attack";
        private const string _Alive = "Alive";
        private const string _Speed = "Speed";
        private const string _AttackSpeedMultiplier = "AttackSpeedMultiplier";
        private const string _Win = "Win";

        [SerializeField] private Animator _animator = null;

        private Rigidbody _rigidbody = null;
        private MonsterAI _ai = null;

        private Vector3 _previousPosition = Vector3.zero;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _ai = GetComponent<MonsterAI>();

            _previousPosition = _rigidbody.position;
        }

        private void Update()
        {
            _animator.SetBool(_Attack, _ai.IsAttacking);
            _animator.SetBool(_Alive, _ai.IsAlive);

            if (_rigidbody.isKinematic)
            {
                return;
            }
            //_animator.SetFloat(_Speed, (_previousPosition - _rigidbody.position).GetXZ().magnitude * 100);
            _animator.SetFloat(_AttackSpeedMultiplier, _ai.AttackSpeedMultiplier);

            _previousPosition = _rigidbody.position;
        }

        private void FixedUpdate()
        {
            if (_rigidbody.isKinematic)
            {
                return;
            }
            _animator.SetFloat(_Speed, (_previousPosition - _rigidbody.position).GetXZ().magnitude * 100);
        }

        public void EnableAnimator()
        {
            _animator.enabled = true;
        }

        public void DisableAnimator()
        {
            _animator.enabled = false;
        }

        public void ActivateWinAnimation()
        {
            //_animator.SetBool(_Win, true);
            _animator.SetTrigger(_Win);
            enabled = false;
        }
    }
}
