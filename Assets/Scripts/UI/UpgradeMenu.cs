using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MonsterArena.Extensions;
using MonsterArena.Interfaces;
using MonsterArena.Models;

namespace MonsterArena.UI
{
    public class UpgradeMenu : MonoBehaviour, IZoneMenu
    {
        [SerializeField] private HelicopterModifiers _modifiers = null;
        [SerializeField] private Wallet _wallet = null;
        [SerializeField] private Button _closeButton = null;

        [SerializeField] private Button _healthUpgradeButton = null;
        [SerializeField] private TextMeshProUGUI _healthUpgradeCurrentBonus = null;
        [SerializeField] private TextMeshProUGUI _healthUpgradeCost = null;
        
        [SerializeField] private Button _damageUpgradeButton = null;
        [SerializeField] private TextMeshProUGUI _damageUpgradeCurrentBonus = null;
        [SerializeField] private TextMeshProUGUI _damageUpgradeCost = null;
        
        [SerializeField] private Button _cargoUpgradeButton = null;
        [SerializeField] private TextMeshProUGUI _cargoUpgradeCurrentBonus = null;
        [SerializeField] private TextMeshProUGUI _cargoUpgradeCost = null;

        [SerializeField] private Button _speedUpgradeButton = null;
        [SerializeField] private TextMeshProUGUI _speedUpgradeCurrentBonus = null;
        [SerializeField] private TextMeshProUGUI _speedUpgradeCost = null;

        private void Start()
        {
            UpdateButtons();
        }

        private void OnEnable()
        {
            _closeButton.onClick.AddListener(CloseMenu);

            _healthUpgradeButton.onClick.AddListener(UpgradeHealth);
            _damageUpgradeButton.onClick.AddListener(UpgradeDamage);
            _cargoUpgradeButton.onClick.AddListener(UpgradeCargo);
            _speedUpgradeButton.onClick.AddListener(UpgradeSpeed);

            UpdateButtons();
        }

        private void OnDisable()
        {
            _closeButton.onClick.RemoveListener(CloseMenu);

            _healthUpgradeButton.onClick.RemoveListener(UpgradeHealth);
            _damageUpgradeButton.onClick.RemoveListener(UpgradeDamage);
            _cargoUpgradeButton.onClick.RemoveListener(UpgradeCargo);
            _speedUpgradeButton.onClick.RemoveListener(UpgradeSpeed);
        }

        public void Activate()
        {
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }

        private void CloseMenu()
        {
            gameObject.SetActive(false);
        }

        private void UpgradeHealth()
        {
            _wallet.Take(_healthUpgradeButton.transform, _modifiers.HealthPrice);

            AnalyticsExtensions.SendSoftSpentEvent("Health", _modifiers.HealthPrice);

            _modifiers.HealthLevel++;
            UpdateButtons();
        }

        private void UpgradeDamage()
        {
            _wallet.Take(_damageUpgradeButton.transform, _modifiers.DamagePrice);

            AnalyticsExtensions.SendSoftSpentEvent("Damage", _modifiers.DamagePrice);

            _modifiers.DamageLevel++;
            UpdateButtons();
        }

        private void UpgradeCargo()
        {
            _wallet.Take(_cargoUpgradeButton.transform, _modifiers.CargoPrice);

            AnalyticsExtensions.SendSoftSpentEvent("Cargo", _modifiers.CargoPrice);

            _modifiers.CargoLevel++;
            UpdateButtons();
        }

        private void UpgradeSpeed()
        {
            _wallet.Take(_speedUpgradeButton.transform, _modifiers.SpeedPrice);

            AnalyticsExtensions.SendSoftSpentEvent("Speed", _modifiers.SpeedPrice);

            _modifiers.SpeedLevel++;
            UpdateButtons();
        }
        
        private void UpdateButtons()
        {
            bool isMax = _modifiers.HealthPrice < 0;
            _healthUpgradeButton.interactable = !isMax && _wallet.HaveCoins(_modifiers.HealthPrice);
            _healthUpgradeCurrentBonus.text = "+" + _modifiers.TransformHealth(0).ToString();
            _healthUpgradeCost.text = isMax ? "Max" : _modifiers.HealthPrice.ToString();

            isMax = _modifiers.DamagePrice < 0;
            _damageUpgradeButton.interactable = !isMax && _wallet.HaveCoins(_modifiers.DamagePrice);
            _damageUpgradeCurrentBonus.text = "+" + _modifiers.TransformDamage(0).ToString();
            _damageUpgradeCost.text = isMax ? "Max" : _modifiers.DamagePrice.ToString();

            isMax = _modifiers.CargoPrice < 0;
            _cargoUpgradeButton.interactable = !isMax && _wallet.HaveCoins(_modifiers.CargoPrice);
            _cargoUpgradeCurrentBonus.text = "+" + _modifiers.TransformCargo(0).ToString();
            _cargoUpgradeCost.text = isMax ? "Max" : _modifiers.CargoPrice.ToString();

            isMax = _modifiers.SpeedPrice < 0;
            _speedUpgradeButton.interactable = !isMax && _wallet.HaveCoins(_modifiers.SpeedPrice);
            _speedUpgradeCurrentBonus.text = "+" + _modifiers.TransformSpeed(0).ToString();
            _speedUpgradeCost.text = isMax ? "Max" : _modifiers.SpeedPrice.ToString();
        }
    }
}
