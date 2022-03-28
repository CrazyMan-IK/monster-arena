using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations;
using AYellowpaper;
using TMPro;
using MonsterArena.UI;
using MonsterArena.Models;
using MonsterArena.Interfaces;

namespace MonsterArena
{
    public class Level : MonoBehaviour
    {
        private const string _TotalPassedLevelsKey = "_passedLevels";

        [SerializeField] private int _killReward = 300;
        [SerializeField] private List<MonsterMovement> _enemies = new List<MonsterMovement>();

        [Header("Scene")]
        [SerializeField] private SceneReference _mainMenuScene = null;
        [SerializeField] private SceneTransition _sceneTransition = null;
        [SerializeField] private TextMeshProUGUI _levelNumberText = null;
        [SerializeField] private WinnerPanel _winnerUI = null;
        [SerializeField] private Wallet _wallet = null;
        [SerializeField] private Button _abilityButton = null;

        [Header("Logic")]
        [SerializeField] private InterfaceReference<IInput> _input = null;
        [SerializeField] private Transform _playerSpawn = null;
        [SerializeField] private MonsterHealthBar _healthBarPrefab = null;
        [SerializeField] private Transform _healthBarsRoot = null;

        [Header("Player")]
        //[SerializeField] private MonsterMovement _playerMovement = null;
        [SerializeField] private MonsterInformation _playerInformation = null;
        [SerializeField] private MonsterHealthBar _playerHealthBar = null;
        [SerializeField] private PositionConstraint _cameraConstraint = null;
        /*[SerializeField] private List<LevelInformation> _levels = new List<LevelInformation>();

        [Space]

        [SerializeField] private TextMeshProUGUI _playerName = null;
        [SerializeField] private TextMeshProUGUI _enemyName = null;

        [Space]

        [SerializeField] private StartPanel _startPanel = null;
        [SerializeField] private WinnerPanel _winnerPanel = null;*/

        private Monster _playerMonster = null;
        private int _levelNum = 0;
        private bool _inited = false;
        private bool _isPlayerWin = false;

        public int TotalLevelsPassed
        {
            get => PlayerPrefs.GetInt(_TotalPassedLevelsKey, 0);
            set => PlayerPrefs.SetInt(_TotalPassedLevelsKey, value);
        }

        public IReadOnlyList<MonsterMovement> Enemies => _enemies;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F) && _abilityButton.interactable)
            {
                UsePlayerAbility();
            }
        }

        private void OnEnable()
        {
            _winnerUI.Clicked += OnWinnerUIClicked;

            _abilityButton.onClick.AddListener(UsePlayerAbility);
        }

        private void OnDisable()
        {
            _playerMonster.Killed -= OnEnemyKilled;
            _playerMonster.Died -= OnPlayerDied;

            _winnerUI.Clicked -= OnWinnerUIClicked;

            _abilityButton.onClick.RemoveListener(UsePlayerAbility);
        }

        public void GameStarted()
        {
            Initialize(TotalLevelsPassed, _playerInformation);
        }

        public void Initialize(int levelNum, MonsterInformation playerInformation)
        {
            if (_inited)
            {
                return;
            }
            _inited = true;

            _levelNum = levelNum;
            _playerInformation = playerInformation;

            _levelNumberText.text = (TotalLevelsPassed + 1).ToString();

            _playerMonster = Instantiate(playerInformation.MonsterPrefab, _playerSpawn);
            _playerMonster.InitializeAsPlayer(playerInformation, _enemies.Count);

            _playerMonster.Killed += OnEnemyKilled;
            _playerMonster.Died += OnPlayerDied;

            _playerHealthBar.Initialize(_playerMonster, null);

            var movement = _playerMonster.gameObject.AddComponent<MonsterMovement>();
            movement.Initialize(_input.Value);

            _cameraConstraint.SetSource(0, new ConstraintSource() { sourceTransform = _playerMonster.transform, weight = 1 });

            var names = AINamesGenerator.Utils.GetRandomNames(_enemies.Count);
            for (int i = 0; i < _enemies.Count; i++)
            {
                var enemy = _enemies[i];
                var name = names[i];

                var healthBar = Instantiate(_healthBarPrefab, _healthBarsRoot);
                healthBar.Initialize(enemy.Monster, name);
            }

            //_level = _levels[levelNum];

            //_playerName.text = _level.PlayerName;
            //_enemyName.text = _level.EnemyName;
        }

        public void LoadNextLevel()
        {
            TotalLevelsPassed++;

            _sceneTransition.Load(_mainMenuScene);
        }

        public void Restart()
        {
            _sceneTransition.ReloadCurrent(rootGOs =>
            {
                var level = rootGOs.Select(x => x.GetComponent<Level>()).First(x => x != null);
                level.Initialize(_levelNum, _playerInformation);
            });
        }

        private void OnEnemyKilled(Transform enemy)
        {
            _isPlayerWin = true;

            _wallet.Add(enemy, _killReward);

            if (!_enemies.Any(x => x.Monster.IsAlive))
            {
                TotalLevelsPassed++;
                _winnerUI.Activate(true);
            }
        }

        private void OnPlayerDied()
        {
            _isPlayerWin = false;

            _winnerUI.Activate(false);
        }

        private void OnWinnerUIClicked()
        {
            if (_isPlayerWin)
            {
                LoadNextLevel();
                return;
            }

            LoadNextLevel();
        }

        private void UsePlayerAbility()
        {
            _playerMonster.UseAbility();

            _abilityButton.interactable = false;

            Invoke(nameof(ActivateAbilityButton), _playerMonster.AbilityCooldown);
        }

        private void ActivateAbilityButton()
        {
            _abilityButton.interactable = true;
        }

        /*private void OnWinnerPanelClicked()
        {
            //_winnerPanel.Clicked -= OnWinnerPanelClicked;

            if (_isPlayerWin)
            {
                LoadNextLevel();
                return;
            }

            _sceneTransition.ReloadCurrent(rootGOs =>
            {
                var level = rootGOs.Select(x => x.GetComponent<Level>()).First(x => x != null);
                level.Initialize(_levelNum, _playerInformation);
            });
        }*/

        /*private void OnStartPanelClosed()
        {
            //_startPanel.Closed -= OnStartPanelClosed;
        }*/
    }
}
