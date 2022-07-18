using System;
using UnityEngine;
using MonsterArena.TasksSystem.Interfaces;

namespace MonsterArena.TasksSystem
{
    public class SellBoxesTask : MonoBehaviour, ILevelTask
    {
        [SerializeField] private Sprite _icon = null;
        [SerializeField] private Helicopter _player = null;
        [SerializeField] private int _boxesCount = 3;
        
        public ILevelTaskModel ToModel()
        {
            return new SellBoxesTaskModel(_icon, _player, _boxesCount);
        }
    }

    [Serializable]
    public class SellBoxesTaskModel : ILevelTaskModel
    {
        public event Action Completed = null;
        public event Action Updated = null;

        //[SerializeField] private int _lastCargo = 0;
        [SerializeField] private int _selledBoxesCount = 0;

        private readonly Sprite _icon = null;
        private readonly Helicopter _player = null;
        private readonly int _boxesCount = 3;

        public SellBoxesTaskModel(Sprite icon, Helicopter player, int boxesCount)
        {
            _icon = icon;
            _player = player;
            _boxesCount = boxesCount;

            //_lastCargo = _player.Cargo;
        }

        public bool IsCompleted => _selledBoxesCount >= _boxesCount;
        public Sprite Icon => _icon;
        public float Progress => _selledBoxesCount * 1.0f / _boxesCount;
        public string Description => $"Sell {_boxesCount} barrels";
        public string Status => $"{_selledBoxesCount} / {_boxesCount}";

        public void Enable()
        {
            if (IsCompleted)
            {
                Completed?.Invoke();
                return;
            }

            _player.CargoChanged += OnPlayerCargoChanged;
        }

        private void OnPlayerCargoChanged(int difference, CauseType cause)
        {
            if (cause == CauseType.Sell)
            {
                _selledBoxesCount -= difference;
            }

            if (IsCompleted)
            {
                _player.CargoChanged -= OnPlayerCargoChanged;
                
                Completed?.Invoke();
            }

            Updated?.Invoke();
        }
    }
}
