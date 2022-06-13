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

        private int _currentLevel = 0;

        private void Awake()
        {
            _currentLevel = PlayerPrefs.GetInt(Constants.RadarLevelKey, 0);

            _radar.SetOffset(_levels[_currentLevel].offset);

            UpdateUI();
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
            if (_currentLevel >= _levels.Count - 1)
            {
                _upgradeButton.interactable = false;

                _level.text = "Max level";
                _price.text = "";
            }
            else
            {
                _level.text = $"Level {_currentLevel + 1}  ► Level {_currentLevel + 2}";
                _price.text = $"{_levels[_currentLevel + 1].cost}$";
            }
        }

        private void OnUpgrade()
        {
            if (_wallet.HaveCoins(_levels[_currentLevel + 1].cost))
            {
                _wallet.Take(_upgradeButton.transform, _levels[_currentLevel + 1].cost);
                
                _radar.SetOffset(_levels[_currentLevel + 1].offset, true);

                _currentLevel++;

                PlayerPrefs.SetInt(Constants.RadarLevelKey, _currentLevel);
            }

            UpdateUI();

            Deactivate();
        }
    }
}
