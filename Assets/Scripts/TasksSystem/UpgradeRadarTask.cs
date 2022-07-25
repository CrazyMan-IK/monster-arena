using System;
using UnityEngine;
using MonsterArena.TasksSystem.Interfaces;
using MonsterArena.UI;

namespace MonsterArena.TasksSystem
{
    public class UpgradeRadarTask : MonoBehaviour, ILevelTask
    {
        [SerializeField] private Sprite _icon = null;
        [SerializeField] private RadarMenu _radar;
        [SerializeField] private int _level = 2;

        public ILevelTaskModel ToModel()
        {
            return new UpgradeRadarTaskModel(_icon, _radar, _level);
        }
    }
    
    [Serializable]
    public class UpgradeRadarTaskModel : ILevelTaskModel
    {
        public event Action Completed = null;
        public event Action Updated = null;

        private readonly Sprite _icon = null;
        private readonly RadarMenu _radar = null;
        private readonly int _level = 2;
        private int _radarCurrentLevel => _radar.CurrentLevel + 1;

        public UpgradeRadarTaskModel(Sprite icon, RadarMenu radar, int level)
        {
            _icon = icon;
            _radar = radar;
            _level = level;
        }

        public bool IsCompleted => _radarCurrentLevel == _level;
        public Sprite Icon => _icon;
        public float Progress => (float) _radarCurrentLevel / _level;
        public string Description => $"Upgrade radar to {_level} level";
        public string Status => $"{_radarCurrentLevel} / {_level}";

        public void Enable()
        {
            if (IsCompleted)
            {
                Completed?.Invoke();
                return;
            }

            _radar.Upgraded += OnUpgraded;
        }
        
        private void OnUpgraded()
        {
            if (IsCompleted)
            {
                _radar.Upgraded -= OnUpgraded;
                Completed?.Invoke();
            }

            Updated?.Invoke();
        }
    }
}
