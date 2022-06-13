using System;
using UnityEngine;
using UnityEngine.AI;
using MonsterArena.People.Interfaces;

namespace MonsterArena.People
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]
    public class People : MonoBehaviour
    {
        private NavMeshAgent _agent = null;
        private Animator _animator = null;
        private PeopleState _state = null;
        private Vector3 _lastPosition = Vector3.zero;

        [field: SerializeField] public float WalkRadius { get; private set; } = 20;
        [field: SerializeField] public float ViewRadius { get; private set; } = 5;
        [field: SerializeField] public LayerMask MonstersLayerMask { get; private set; } = default;
        public Vector3 BasePosition { get; private set; } = Vector3.zero;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();

            BasePosition = transform.position;

            ChangeState(new IdlePeopleState());
        }

        private void Update()
        {
            _state.Update(Time.deltaTime);

            var currentSpeed = (transform.position - _lastPosition).magnitude;
            _animator.SetFloat(Constants.Speed, currentSpeed / Time.deltaTime / _agent.speed / 2);

            _lastPosition = transform.position;
        }

        public void ChangeState(PeopleState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (_state != null)
            {
                _state.Exit();
            }
            
            _state = state;
            _state.Enter(this, _agent);
        }

        public void MoveTo(Vector3 position)
        {
            _agent.SetDestination(position);
        }
    }
}
