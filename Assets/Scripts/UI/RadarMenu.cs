using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MonsterArena.Extensions;
using MonsterArena.Interfaces;
using MonsterArena.Models;

namespace MonsterArena.UI
{
    public class RadarMenu : MonoBehaviour, IZoneMenu
    {
        [Serializable]
        struct RadarLevel
        {
            public int cost;
            public float offset;
        }

        [SerializeField] private Button _closeButton = null;
        [SerializeField] private Button _upgradeButton = null;
        [SerializeField] private TextMeshProUGUI _level = null;
        [SerializeField] private TextMeshProUGUI _price = null;
        [SerializeField] private Wallet _wallet = null;
        [SerializeField] private Radar _radar = null;
        [SerializeField] private List<RadarLevel> _levels = new List<RadarLevel>();

        public event Action Upgraded;
        
        public int CurrentLevel { get; private set; } = 0;

        private void Start()
        {
            CurrentLevel = PlayerPrefs.GetInt(Constants.RadarLevelKey, 0);
            _radar.SetOffset(_levels[CurrentLevel].offset);

            UpdateUI();

            Deactivate();
        }

        private void OnEnable()
        {
            _closeButton.onClick.AddListener(Deactivate);
            _upgradeButton.onClick.AddListener(OnUpgrade);
        }

        private void OnDisable()
        {
            _closeButton.onClick.RemoveListener(Deactivate);
            _upgradeButton.onClick.RemoveListener(OnUpgrade);
        }

        public void Activate()
        {
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }

        private void UpdateUI()
        {
            if (CurrentLevel >= _levels.Count - 1)
            {
                _upgradeButton.interactable = false;

                _level.text = "Max level";
                _price.text = "";
            }
            else
            {
                _level.text = $"Level {CurrentLevel + 1}  ► Level {CurrentLevel + 2}";
                _price.text = $"{_levels[CurrentLevel + 1].cost} <sprite=0>";
            }
        }

        private void OnUpgrade()
        {
            if (_wallet.HaveCoins(_levels[CurrentLevel + 1].cost))
            {
                _wallet.Take(_upgradeButton.transform, _levels[CurrentLevel + 1].cost);
                
                _radar.SetOffset(_levels[CurrentLevel + 1].offset, true);

                CurrentLevel++;

                PlayerPrefs.SetInt(Constants.RadarLevelKey, CurrentLevel);
                Upgraded?.Invoke();
            }

            UpdateUI();

            Deactivate();
        }
    }
}
