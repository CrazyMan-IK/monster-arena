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
                    PlayerPrefs.SetInt(Constants.HealthLevelKey, _healthLevel);

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
                    PlayerPrefs.SetInt(Constants.DamageLevelKey, _damageLevel);

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
                    PlayerPrefs.SetInt(Constants.CargoLevelKey, _cargoLevel);

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
                    PlayerPrefs.SetInt(Constants.SpeedLevelKey, _speedLevel);

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
            _healthLevel = PlayerPrefs.GetInt(Constants.HealthLevelKey, 0);
            _damageLevel = PlayerPrefs.GetInt(Constants.DamageLevelKey, 0);
            _cargoLevel = PlayerPrefs.GetInt(Constants.CargoLevelKey, 0);
            _speedLevel = PlayerPrefs.GetInt(Constants.SpeedLevelKey, 0);
        }

        public float TransformHealth(float health)
        {
            return TransformHealth(health, _healthLevel - 1);
        }

        public float TransformDamage(float damage)
        {
            return TransformDamage(damage, _damageLevel - 1);
        }

        public int TransformCargo(int cargo)
        {
            return TransformCargo(cargo, _cargoLevel - 1);
        }

        public float TransformSpeed(float speed)
        {
            return TransformSpeed(speed, _speedLevel - 1);
        }

        public float TransformNextLevelHealth(int health)
        {
            return TransformHealth(health, _healthLevel);
        }

        public float TransformNextLevelDamage(float damage)
        {
            return TransformDamage(damage, _damageLevel);
        }

        public int TransformNextLevelCargo(int cargo)
        {
            return TransformCargo(cargo, _cargoLevel);
        }

        public float TransformNextLevelSpeed(float speed)
        {
            return TransformSpeed(speed, _speedLevel);
        }

        private float TransformHealth(float health, int healthLevel)
        {
            if (healthLevel < 0)
            {
                return health;
            }

            if (healthLevel >= _healthValues.Count)
                healthLevel = _healthValues.Count - 1;

            return health + _healthValues[healthLevel].Value;
        }

        private float TransformDamage(float damage, int damageLevel)
        {
            if (damageLevel < 0)
            {
                return damage;
            }

            if (damageLevel >= _damageValues.Count)
                damageLevel = _damageValues.Count - 1;

            return damage + _damageValues[damageLevel].Value;
        }

        private int TransformCargo(int cargo, int cargoLevel)
        {
            if (cargoLevel < 0)
            {
                return cargo;
            }

            if (cargoLevel >= _cargoValues.Count)
                cargoLevel = _cargoValues.Count - 1;

            return cargo + _cargoValues[cargoLevel].Value;
        }

        private float TransformSpeed(float speed, int speedLevel)
        {
            if (speedLevel < 0)
            {
                return speed;
            }

            if (speedLevel >= _speedValues.Count)
                speedLevel = _speedValues.Count - 1;

            return speed + _speedValues[speedLevel].Value;
        }

        private void OnModifierChanged()
        {
            LevelChanged?.Invoke();

            _lastLevel = CurrentLevel;

            Changed?.Invoke();
        }
    }
}