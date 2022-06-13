using System;
using UnityEngine;
using MonsterArena.TasksSystem.Interfaces;

namespace MonsterArena.TasksSystem
{
    [Serializable]
    public class KillEnemyTask : MonoBehaviour, ILevelTask
    {
        [SerializeField] private Sprite _icon = null;
        [SerializeField] private Level _level = null;
        [SerializeField] private int _enemiesCount = 10;
        [SerializeField] private int _minLevel = 2;

        public ILevelTaskModel ToModel()
        {
            return new KillEnemyTaskModel(_icon, _level, _enemiesCount, _minLevel);
        }
    }

    public class KillEnemyTaskModel : ILevelTaskModel
    {
        public event Action Completed = null;
        public event Action Updated = null;

        [SerializeField] int _killedEnemiesCount = 0;

        private readonly Sprite _icon = null;
        private readonly Level _level = null;
        private readonly int _enemiesCount = 10;
        private readonly int _minLevel = 2;
        
        public KillEnemyTaskModel(Sprite icon, Level level, int enemiesCount, int minLevel)
        {
            _icon = icon;
            _level = level;
            _enemiesCount = enemiesCount;
            _minLevel = minLevel;
        }
        
        public bool IsCompleted => _killedEnemiesCount >= _enemiesCount;
        public Sprite Icon => _icon;
        public float Progress => _killedEnemiesCount * 1.0f / _enemiesCount;
        public string Description => $"Kill {_enemiesCount} enemies with minimum {_minLevel} level";
        public string Status => $"{_killedEnemiesCount} / {_enemiesCount}";

        public void Enable()
        {
            if (IsCompleted)
            {
                Completed?.Invoke();
                return;
            }
            
            _level.EnemyDied += OnEnemyDied;
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

            Updated?.Invoke();
        }
    }
}
