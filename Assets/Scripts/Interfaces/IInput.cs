using System;
using UnityEngine;

namespace MonsterArena.Interfaces
{
    public interface IInput
    {
        event Action AbilityUsed;
        event Action PropThrowed;

        Vector2 Direction { get; }

        void Lock();
    }
}
