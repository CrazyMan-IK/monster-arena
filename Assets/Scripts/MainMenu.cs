using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MonsterArena.Models;
using MonsterArena.UI;

namespace MonsterArena
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private List<MonsterInformation> _availableMonsters = new List<MonsterInformation>();
        [SerializeField] private ParticleSystem _unlockParticles = null;
        [SerializeField] private Carousel _carousel = null;
        [SerializeField] private TextMeshProUGUI _monsterName = null;
        [SerializeField] private MainButton _mainButton = null;

        private void Awake()
        {
            _carousel.Initialize(_availableMonsters);
        }

        private void OnEnable()
        {
            _carousel.SelectionChanged += OnCarouselSelectionChanged;
            _mainButton.Unlocked += OnMonsterUnlocked;
        }

        private void OnDisable()
        {
            _carousel.SelectionChanged -= OnCarouselSelectionChanged;
            _mainButton.Unlocked -= OnMonsterUnlocked;
        }

        private void OnCarouselSelectionChanged(MonsterMenuView monster)
        {
            _mainButton.ChangeInformation(monster.Information);
            _monsterName.text = monster.Information.Name;
        }

        private void OnMonsterUnlocked()
        {
            _carousel.CurrentMonster.Unlock();
            _unlockParticles.Play();
        }
    }
}
