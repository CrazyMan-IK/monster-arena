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
        [SerializeField] private List<SceneReference> _levels = new List<SceneReference>();
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

            var isUnlocked = monster.Information.IsUnlocked;
            _questions.gameObject.SetActive(!isUnlocked);
            _monsterName.gameObject.SetActive(isUnlocked);

            _monsterName.text = monster.Information.Name;
        }

        private void OnGameStarted()
        {
            var index = Level.TotalLevelsPassed % _levels.Count;

            _sceneTransition.Load(_levels[index], rootGOs =>
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
