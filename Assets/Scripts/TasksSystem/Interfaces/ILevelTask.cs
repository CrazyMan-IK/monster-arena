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
        
        bool IsCompleted { get; }

        void Enable();
        string GetTaskTitle();
        string GetTaskStatus();
    }
}
