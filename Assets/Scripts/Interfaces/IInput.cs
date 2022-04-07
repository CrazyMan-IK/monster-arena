using System;
using UnityEngine;

namespace MonsterArena.Interfaces
{
    public interface IInput
    {
        event Action AbilityUsed;

        Vector2 Direction { get; }

        void Lock();
    }
}
