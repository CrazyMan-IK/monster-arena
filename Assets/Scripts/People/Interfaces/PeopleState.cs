using System;
using UnityEngine;
using UnityEngine.AI;

namespace MonsterArena.People.Interfaces
{
    public abstract class PeopleState
    {
        protected People People { get; private set; }
        protected NavMeshAgent Agent { get; private set; }
        
        public void Enter(People people, NavMeshAgent agent)
        {
            People = people != null ? people : throw new ArgumentNullException(nameof(people));
            Agent = agent != null ? agent : throw new ArgumentNullException(nameof(agent));

            OnEnter();
        }
        public void Exit()
        {
            OnExit();
        }
        public void Update(float deltaTime)
        {
            if (!Agent.isOnNavMesh)
            {
                //Debug.DrawRay(People.transform.position, Vector3.up * 10, Color.blue);
                return;
            }

            if (Agent.remainingDistance <= Agent.stoppingDistance)
            {
                OnReachDestination();
            }
            
            OnUpdate(deltaTime);
        }

        protected virtual void OnEnter() { }
        protected virtual void OnExit() { }
        protected virtual void OnUpdate(float deltaTime) { }
        protected virtual void OnReachDestination() { }
    }
}
