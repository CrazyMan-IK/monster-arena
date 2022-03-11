using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cinemachine;
using MonsterArena.Models;
using MonsterArena.UI;

namespace MonsterArena
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private SceneReference _levelScene = null;
        [SerializeField] private SceneTransition _sceneTransition = null;

        [Space]

        [SerializeField] private List<MonsterInformation> _availableMonsters = new List<MonsterInformation>();
        [SerializeField] private ParticleSystem _unlockParticles = null;
        [SerializeField] private Carousel _carousel = null;
        [SerializeField] private TextMeshProUGUI _monsterName = null;
        [SerializeField] private RectTransform _questions = null;
        [SerializeField] private MainButton _mainButton = null;
        [SerializeField] private CinemachineVirtualCamera _unlockCamera = null;

        private void Awake()
        {
            _carousel.Initialize(_availableMonsters);
        }

        private void OnEnable()
        {
            _carousel.SelectionChanged += OnCarouselSelectionChanged;
            _mainButton.GameStarted += OnGameStarted;
            _mainButton.Unlocked += OnMonsterUnlocked;

            foreach (var monster in _carousel.Monsters)
            {
                monster.WinAnimationCompleted += OnWinAnimationCompleted;
            }
        }

        private void OnDisable()
        {
            _carousel.SelectionChanged -= OnCarouselSelectionChanged;
            _mainButton.GameStarted -= OnGameStarted;
            _mainButton.Unlocked -= OnMonsterUnlocked;

            foreach (var monster in _carousel.Monsters)
            {
                monster.WinAnimationCompleted -= OnWinAnimationCompleted;
            }
        }

        private void OnCarouselSelectionChanged(MonsterMenuView monster)
        {
            _mainButton.ChangeInformation(monster.Information);

            if (monster.Information.IsUnlocked)
            {
                _questions.gameObject.SetActive(false);
                _monsterName.gameObject.SetActive(true);
            }
            else
            {
                _questions.gameObject.SetActive(true);
                _monsterName.gameObject.SetActive(false);
            }

            _monsterName.text = monster.Information.Name;
        }

        private void OnGameStarted()
        {
            _sceneTransition.Load(_levelScene, rootGOs =>
            {
                var level = rootGOs.Select(x => x.GetComponent<Level>()).First(x => x != null);
                level.Initialize(0, _carousel.CurrentMonster.Information);
            });
        }

        private void OnMonsterUnlocked()
        {
            _unlockParticles.Play();
            _carousel.CurrentMonster.Unlock();

            _unlockCamera.Priority = 10;
        }

        private void OnWinAnimationCompleted()
        {
            _questions.gameObject.SetActive(false);
            _monsterName.gameObject.SetActive(true);

            _mainButton.CompleteAnimation();

            _unlockCamera.Priority = -10;
        }
    }
}
