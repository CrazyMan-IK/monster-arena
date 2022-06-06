using System;
using UnityEngine;
using MonsterArena.TasksSystem.Interfaces;

namespace MonsterArena.TasksSystem
{
    [Serializable]
    public class KillEnemyTask : MonoBehaviour, ILevelTask
    {
        [SerializeField] private Level _level = null;
        [SerializeField] private int _enemiesCount = 10;
        [SerializeField] private int _minLevel = 2;

        public ILevelTaskModel ToModel()
        {
            return new KillEnemyTaskModel(_level, _enemiesCount, _minLevel);
        }
    }

    public class KillEnemyTaskModel : ILevelTaskModel
    {
        public event Action Completed = null;

        [SerializeField] int _killedEnemiesCount = 0;

        private readonly Level _level = null;
        private readonly int _enemiesCount = 10;
        private readonly int _minLevel = 2;
        
        public KillEnemyTaskModel(Level level, int enemiesCount, int minLevel)
        {
            _level = level;
            _enemiesCount = enemiesCount;
            _minLevel = minLevel;
        }

        public bool IsCompleted => _killedEnemiesCount >= _enemiesCount;
        
        public void Enable()
        {
            _level.EnemyDied += OnEnemyDied;
        }

        public string GetTaskTitle()
        {
            return "Kill enemy {X}";
        }

        public string GetTaskStatus()
        {
            return "X/X";
        }

        private void OnEnemyDied(Monster enemy)
        {
            if (enemy.Level >= _minLevel)
            {
                _killedEnemiesCount++;
            }

            if (IsCompleted)
            {
                _level.EnemyDied -= OnEnemyDied;

                Completed?.Invoke();
            }
        }
    }
}
