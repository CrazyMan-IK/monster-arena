using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterArena.Models
{
    [CreateAssetMenu(fileName = "New HelicopterModifiers", menuName = "Monster Arena/Helicopter Modifiers", order = 70)]
    public class HelicopterModifiers : ScriptableObject
    {
        public event Action Changed = null;
        public event Action LevelChanged = null;

        private const string _HealthLevel = "_healthLevel";
        private const string _DamageLevel = "_damageLevel";
        private const string _CargoLevel = "_cargoLevel";
        private const string _SpeedLevel = "_speedLevel";

        private int _healthLevel = 0;
        private int _damageLevel = 0;
        private int _cargoLevel = 0;
        private int _speedLevel = 0;
        private int _lastLevel = 0;

        [SerializeField] private Material _baseHealthVisual = null;
        [SerializeField] private Material _baseDamageVisual = null;

        [SerializeField] private List<Modifier<float, Material>> _healthValues = new List<Modifier<float, Material>>();
        [SerializeField] private List<Modifier<float, Material>> _damageValues = new List<Modifier<float, Material>>();
        [SerializeField] private List<Modifier<int, float>> _cargoValues = new List<Modifier<int, float>>();
        [SerializeField] private List<Modifier<float, float>> _speedValues = new List<Modifier<float, float>>();
        [SerializeField] private List<float> _levels = new List<float>();

        public int HealthLevel
        {
            get => _healthLevel;
            set
            {
                if (_healthLevel != value)
                {
                    _healthLevel = value;
                    PlayerPrefs.SetInt(_HealthLevel, _healthLevel);

                    OnModifierChanged();
                }
            }
        }
        public int DamageLevel
        {
            get => _damageLevel;
            set
            {
                if (_damageLevel != value)
                {
                    _damageLevel = value;
                    PlayerPrefs.SetInt(_DamageLevel, _damageLevel);

                    OnModifierChanged();
                }
            }
        }
        public int CargoLevel
        {
            get => _cargoLevel;
            set
            {
                if (_cargoLevel != value)
                {
                    _cargoLevel = value;
                    PlayerPrefs.SetInt(_CargoLevel, _cargoLevel);

                    OnModifierChanged();
                }
            }
        }
        public int SpeedLevel
        {
            get => _speedLevel;
            set
            {
                if (_speedLevel != value)
                {
                    _speedLevel = value;
                    PlayerPrefs.SetInt(_SpeedLevel, _speedLevel);

                    OnModifierChanged();
                }
            }
        }

        public int HealthPrice => _healthValues.ElementAtOrDefault(_healthLevel)?.Price ?? -1;
        public int DamagePrice => _damageValues.ElementAtOrDefault(_damageLevel)?.Price ?? -1;
        public int CargoPrice => _cargoValues.ElementAtOrDefault(_cargoLevel)?.Price ?? -1;
        public int SpeedPrice => _speedValues.ElementAtOrDefault(_speedLevel)?.Price ?? -1;

        public Material HealthVisual => _healthValues.ElementAtOrDefault(_healthLevel - 1)?.Visual ?? _baseHealthVisual;
        public Material DamageVisual => _damageValues.ElementAtOrDefault(_damageLevel - 1)?.Visual ?? _baseDamageVisual;
        public float CargoVisual => _cargoValues.ElementAtOrDefault(_cargoLevel - 1)?.Visual ?? 0;
        public float SpeedVisual => _speedValues.ElementAtOrDefault(_speedLevel - 1)?.Visual ?? 0;

        public float CurrentExperience
        {
            get
            {
                var health = _healthValues.ElementAtOrDefault(_healthLevel - 1)?.Experience ?? 0;
                var damage = _damageValues.ElementAtOrDefault(_damageLevel - 1)?.Experience ?? 0;
                var cargo = _cargoValues.ElementAtOrDefault(_cargoLevel - 1)?.Experience ?? 0;
                var speed = _speedValues.ElementAtOrDefault(_speedLevel - 1)?.Experience ?? 0;

                return health + damage + cargo + speed;
            }
        }
        
        public float CurrentExperienceMapped
        {
            get
            {
                var total = CurrentExperience;

                var level = _levels.FindLastIndex(x => x <= total);

                if (level < 0)
                {
                    return total;
                }

                return total - _levels[level];
            }
        }

        public int CurrentLevel
        {
            get
            {
                var total = CurrentExperience;

                return _levels.FindLastIndex(x => x <= total) + 1;
            }
        }

        private void OnEnable()
        {
            _healthLevel = PlayerPrefs.GetInt(_HealthLevel, 0);
            _damageLevel = PlayerPrefs.GetInt(_DamageLevel, 0);
            _cargoLevel = PlayerPrefs.GetInt(_CargoLevel, 0);
            _speedLevel = PlayerPrefs.GetInt(_SpeedLevel, 0);
        }

        public float TransformHealth(float health)
        {
            if (_healthLevel <= 0)
            {
                return health;
            }
            
            return health + _healthValues[_healthLevel - 1].Value;
        }

        public float TransformDamage(float damage)
        {
            if (_damageLevel <= 0)
            {
                return damage;
            }

            return damage + _damageValues[_damageLevel - 1].Value;
        }

        public int TransformCargo(int cargo)
        {
            if (_cargoLevel <= 0)
            {
                return cargo;
            }

            return cargo + _cargoValues[_cargoLevel - 1].Value;
        }

        public float TransformSpeed(float speed)
        {
            if (_speedLevel <= 0)
            {
                return speed;
            }

            return speed + _speedValues[_speedLevel - 1].Value;
        }

        private void OnModifierChanged()
        {
            if (CurrentLevel > _lastLevel)
            {
                LevelChanged?.Invoke();

                _lastLevel = CurrentLevel;
            }
            
            Changed?.Invoke();
        }
    }
}
