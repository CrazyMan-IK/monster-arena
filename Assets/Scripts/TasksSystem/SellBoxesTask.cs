using System;
using UnityEngine;
using MonsterArena.TasksSystem.Interfaces;

namespace MonsterArena.TasksSystem
{
    public class SellBoxesTask : MonoBehaviour, ILevelTask
    {
        [SerializeField] private Helicopter _player = null;
        [SerializeField] private int _boxesCount = 3;
        
        public ILevelTaskModel ToModel()
        {
            return new SellBoxesTaskModel(_player, _boxesCount);
        }
    }

    [Serializable]
    public class SellBoxesTaskModel : ILevelTaskModel
    {
        public event Action Completed = null;

        //[SerializeField] private int _lastCargo = 0;
        [SerializeField] private int _collectedBoxesCount = 0;

        private readonly Helicopter _player = null;
        private readonly int _boxesCount = 3;

        public SellBoxesTaskModel(Helicopter player, int boxesCount)
        {
            _player = player;
            _boxesCount = boxesCount;

            //_lastCargo = _player.Cargo;
        }

        public bool IsCompleted => _collectedBoxesCount >= _boxesCount;

        public void Enable()
        {
            _player.CargoChanged += OnPlayerCargoChanged;
        }

        public string GetTaskTitle()
        {
            return "Sell {X} boxes";
        }

        public string GetTaskStatus()
        {
            return "X/X";
        }

        private void OnPlayerCargoChanged(int difference, CauseType cause)
        {
            if (cause == CauseType.Sell)
            {
                _collectedBoxesCount -= difference;
            }

            if (IsCompleted)
            {
                _player.CargoChanged -= OnPlayerCargoChanged;
                
                Completed?.Invoke();
            }
        }
    }
}
