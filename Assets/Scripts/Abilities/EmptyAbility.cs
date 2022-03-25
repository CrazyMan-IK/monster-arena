using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonsterArena.Models;
using MonsterArena.Interfaces;

namespace MonsterArena
{
    public class EmptyAbility : MonoBehaviour, IMonsterAbility
    {
        public float Cooldown => 0;

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

        public float TransformDamage(float damage)
        {
            return damage;
        }
    }
}
