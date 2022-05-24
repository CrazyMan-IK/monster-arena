using URandom = UnityEngine.Random;
using UnityEngine;
using UnityEngine.AI;
using MonsterArena.People.Interfaces;
using MonsterArena.Extensions;

namespace MonsterArena.People
{
    public class WalkPeopleState : PeopleState
    {
        private readonly Collider[] _lastColliders = new Collider[8];

        protected override void OnEnter()
        {
            var newPos = People.BasePosition + URandom.insideUnitCircle.AsXZ() * People.WalkRadius;

            NavMesh.SamplePosition(newPos, out var hit, People.WalkRadius, 1);

            People.MoveTo(hit.position);
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
        }

        protected override void OnReachDestination()
        {
            People.ChangeState(new IdlePeopleState());
        }
    }
}
