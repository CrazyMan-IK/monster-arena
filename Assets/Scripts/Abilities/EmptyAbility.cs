using System;
using UnityEngine;
using MonsterArena.Models;
using MonsterArena.Interfaces;

namespace MonsterArena
{
    public class EmptyAbility : MonoBehaviour, IMonsterAbility
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
            return speed;
        }

        public float TransformReceivedDamage(float damage)
        {
            return damage;
        }

        public float TransformRange(float range)
        {
            return range;
        }
    }
}
