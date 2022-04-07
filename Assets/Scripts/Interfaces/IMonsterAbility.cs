using System;
using UnityEngine;
using MonsterArena.Models;

namespace MonsterArena.Interfaces
{
    public interface IMonsterAbility
    {
        public event Action<Transform> Killed;

        float Cooldown { get; }
        bool CanUse { get; }

        void Initialize(MonsterInformation information, LayerMask monstersLayerMask);
        void Use();
        float TransformSpeed(float speed);
        float TransformReceivedDamage(float damage);
        float TransformRange(float range);
    }
}
