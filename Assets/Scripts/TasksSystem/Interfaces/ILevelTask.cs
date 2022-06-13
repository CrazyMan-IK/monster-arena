using System;
using UnityEngine;

namespace MonsterArena.TasksSystem.Interfaces
{
    public interface ILevelTask
    {
        ILevelTaskModel ToModel();
    }

    public interface ILevelTaskModel
    {
        event Action Completed;
        event Action Updated;

        bool IsCompleted { get; }
        Sprite Icon { get; }
        float Progress { get; }
        string Description { get; }
        string Status { get; }

        void Enable();
    }
}
