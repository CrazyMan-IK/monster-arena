using System;
using UnityEngine;
using MonsterArena.TasksSystem.Interfaces;

namespace MonsterArena.TasksSystem
{
    public enum UpgradeDetailType
    {
        Health,
        Damage,
        Cargo,
        Speed
    }

    public class UpgradePlayerTask : MonoBehaviour, ILevelTask
    {
        [SerializeField] private Sprite _icon = null;
        [SerializeField] private Helicopter _player = null;
        [SerializeField] private UpgradeDetailType _detailType = UpgradeDetailType.Health;
        [SerializeField] private int _level = 2;

        public ILevelTaskModel ToModel()
        {
            return new UpgradePlayerTaskModel(_icon, _player, _detailType, _level);
        }
    }
    
    [Serializable]
    public class UpgradePlayerTaskModel : ILevelTaskModel
    {
        public event Action Completed = null;
        public event Action Updated = null;

        private readonly Sprite _icon = null;
        private readonly Helicopter _player = null;
        private readonly UpgradeDetailType _detailType = UpgradeDetailType.Health;
        private readonly int _level = 2;

        public UpgradePlayerTaskModel(Sprite icon, Helicopter player, UpgradeDetailType detailType, int level)
        {
            _icon = icon;
            _player = player;
            _detailType = detailType;
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
        public string Description => $"Upgrade player {_detailType} to {_level} level";
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
            var level = -1;
            switch (_detailType)
            {
                case UpgradeDetailType.Health:
                    level = _player.Modifiers.HealthLevel;
                    break;
                case UpgradeDetailType.Damage:
                    level = _player.Modifiers.DamageLevel;
                    break;
                case UpgradeDetailType.Cargo:
                    level = _player.Modifiers.CargoLevel;
                    break;
                case UpgradeDetailType.Speed:
                    level = _player.Modifiers.SpeedLevel;
                    break;
            }

            return level;
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
