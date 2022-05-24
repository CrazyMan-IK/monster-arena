using URandom = UnityEngine.Random;
using UnityEngine;
using UnityEngine.AI;
using MonsterArena.People.Interfaces;
using MonsterArena.Extensions;

namespace MonsterArena.People
{
    public class PanicPeopleState : PeopleState
    {
        private readonly Collider[] _lastColliders = new Collider[8];
        private readonly Timer _timer = null;
        private Vector3 _lastDirection = Vector3.zero;

        public PanicPeopleState()
        {
            _timer = new Timer(6);

            _timer.Ticked += OnTimerTicked;
            _timer.Stop();
        }

        protected override void OnEnter()
        {
            _timer.Start();
        }

        protected override void OnExit()
        {
            _timer.Stop();
        }

        protected override void OnUpdate(float deltaTime)
        {
            Vector3 totalPosition = Vector3.zero;
            int monstersCount = 0;
            
            var count = Physics.OverlapSphereNonAlloc(People.transform.position, People.ViewRadius, _lastColliders, People.MonstersLayerMask);
            for (int i = 0; i < count; i++)
            {
                var collider = _lastColliders[i];

                if (collider.TryGetComponent(out Monster monster) && monster.IsAlive)
                {
                    totalPosition += monster.transform.position;
                    monstersCount++;
                }
            }
            
            if (totalPosition != Vector3.zero)
            {
                _lastDirection = People.transform.position - totalPosition / monstersCount;
                _lastDirection.Normalize();

                Agent.isStopped = true;

                _timer.Stop();
            }
            else
            {
                Agent.isStopped = false;

                _timer.Start();
            }

            People.transform.rotation = Quaternion.Lerp(People.transform.rotation, Quaternion.LookRotation(_lastDirection), 15 * deltaTime);
            Agent.Move(Agent.speed * 3 * deltaTime * _lastDirection);

            _timer.Update(deltaTime);
        }

        private void OnTimerTicked()
        {
            People.ChangeState(new IdlePeopleState());
        }
    }
}
