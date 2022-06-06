using System;
using UnityEngine;

namespace MonsterArena.Interfaces
{
    public interface IHealthComponent
    {
        float HP { get; }
        float MaxHP { get; }
        bool IsAlive { get; }
        GameObject gameObject { get; }
    }
}
