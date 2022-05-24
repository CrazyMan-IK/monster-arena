using System;
using UnityEngine;
using MonsterArena.Models;
using MonsterArena.Interfaces;

namespace MonsterArena
{
    public class TutorialAbility : MonoBehaviour, IMonsterAbility
    {
        public event Action<Transform> Killed = null;

        public float Cooldown => 0;
        public bool CanUse => false;

        public void Initialize(MonsterInformation information, LayerMask monstersLayerMask)
        {

        }

        public void Use()
        {

        }

        public float TransformSpeed(float speed)
        {
            return 0;
        }

        public float TransformReceivedDamage(float damage)
        {
            return 0;
        }

        public float TransformRange(float range)
        {
            return 0;
        }
    }
}
