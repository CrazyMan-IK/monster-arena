using System;
using URandom = UnityEngine.Random;
using UnityEngine;
using MonsterArena.People.Interfaces;

namespace MonsterArena.People
{
    public class IdlePeopleState : PeopleState
    {
        private readonly Collider[] _lastColliders = new Collider[8];
        private readonly Timer _timer;

        public IdlePeopleState()
        {
            _timer = new Timer(URandom.Range(1, 5));

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
            bool hasMonsters = false;

            var count = Physics.OverlapSphereNonAlloc(People.transform.position, People.ViewRadius, _lastColliders, People.MonstersLayerMask);
            for (int i = 0; i < count; i++)
            {
                var collider = _lastColliders[i];

                if (collider.TryGetComponent(out Monster monster) && monster.IsAlive)
                {
                    hasMonsters = true;
                    break;
                }
            }

            if (hasMonsters)
            {
                People.ChangeState(new PanicPeopleState());
            }
            
            _timer.Update(deltaTime);
        }

        private void OnTimerTicked()
        {
            People.ChangeState(new WalkPeopleState());
        }
    }
}
