using System;
using UnityEngine;
using MonsterArena.TasksSystem.Interfaces;

namespace MonsterArena.TasksSystem
{

    public class UpgradePlayerLevelTask : MonoBehaviour, ILevelTask
    {
        [SerializeField] private Sprite _icon = null;
        [SerializeField] private Helicopter _player = null;
        [SerializeField] private int _level = 2;

        public ILevelTaskModel ToModel()
        {
            return new UpgradePlayerLevelTaskModel(_icon, _player, _level);
        }
    }
    
    [Serializable]
    public class UpgradePlayerLevelTaskModel : ILevelTaskModel
    {
        public event Action Completed = null;
        public event Action Updated = null;

        private readonly Sprite _icon = null;
        private readonly Helicopter _player = null;
        private readonly int _level = 2;

        public UpgradePlayerLevelTaskModel(Sprite icon, Helicopter player, int level)
        {
            _icon = icon;
            _player = player;
            _level = level;
        }

        public bool IsCompleted
        {
            get
            {
                return GetCurrentLevel() >= _level;
            }
        }

        public Sprite Icon => _icon;
        public float Progress => GetCurrentLevel() * 1.0f / _level;
        public string Description => $"Upgrade player to {_level} level";
        public string Status => $"{GetCurrentLevel()} / {_level}";

        public void Enable()
        {
            if (IsCompleted)
            {
                Completed?.Invoke();
                return;
            }

            _player.ModifiersChanged += OnPlayerModifiersChanged;
        }
        
        private int GetCurrentLevel()
        {
            return _player.Level;
        }

        private void OnPlayerModifiersChanged()
        {
            if (IsCompleted)
            {
                _player.ModifiersChanged -= OnPlayerModifiersChanged;
                Completed?.Invoke();
            }

            Updated?.Invoke();
        }
    }
}
