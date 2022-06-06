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
        [SerializeField] private Helicopter _player = null;
        [SerializeField] private UpgradeDetailType _detailType = UpgradeDetailType.Health;
        [SerializeField] private int _level = 2;

        public ILevelTaskModel ToModel()
        {
            return new UpgradePlayerTaskModel(_player, _detailType, _level);
        }
    }
    
    [Serializable]
    public class UpgradePlayerTaskModel : ILevelTaskModel
    {
        public event Action Completed = null;

        private readonly Helicopter _player = null;
        private readonly UpgradeDetailType _detailType = UpgradeDetailType.Health;
        private readonly int _level = 2;

        public UpgradePlayerTaskModel(Helicopter player, UpgradeDetailType detailType, int level)
        {
            _player = player;
            _detailType = detailType;
            _level = level;
        }

        public bool IsCompleted
        {
            get
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

                return level >= _level;
            }
        }
        
        public void Enable()
        {
            _player.ModifiersChanged += OnPlayerModifiersChanged;
        }
        
        public string GetTaskTitle()
        {
            return "Upgrade player {Y} to {X} level";
        }
        
        public string GetTaskStatus()
        {
            return "X/X";
        }

        private void OnPlayerModifiersChanged()
        {
            if (IsCompleted)
            {
                _player.ModifiersChanged -= OnPlayerModifiersChanged;
                Completed?.Invoke();
            }
        }
    }
}
