using MonsterArena.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterArena.Interfaces
{
    public interface IMonsterAbility
    {
        float Cooldown { get; }

        void Initialize(MonsterInformation information, LayerMask monstersLayerMask);
        void Use();
        float TransformSpeed(float speed);
        float TransformReceivedDamage(float damage);
        float TransformRange(float range);
    }
}
