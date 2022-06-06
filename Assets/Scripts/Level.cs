using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations;
using AYellowpaper;
using TMPro;
using Cinemachine;
using MonsterArena.UI;
using MonsterArena.Interfaces;
using System.Collections;
using GameAnalyticsSDK;
using MonsterArena.Extensions;
using MonsterArena.TasksSystem;

namespace MonsterArena
{
    public class Level : MonoBehaviour
    {
        private const string _TotalPassedLevelsKey = "_passedLevels";

        public event Action<Monster> EnemyDied = null;

        [SerializeField] private int _killReward = 300;
        [SerializeField] private List<MonsterMovement> _enemies = new List<MonsterMovement>();

        [Header("Scene")]
        [SerializeField] private LevelTasks _levelTasks = null;
        [SerializeField] private SceneTransition _sceneTransition = null;
        [SerializeField] private TextMeshProUGUI _levelNumberText = null;
        [SerializeField] private Wallet _wallet = null;
        [SerializeField] private Button _abilityButton = null;

        [Header("Logic")]
        [SerializeField] private InterfaceReference<IInput> _input = null;
        [SerializeField] private Helicopter _player = null;
        [SerializeField] private MonsterHealthBar _healthBarPrefab = null;
        [SerializeField] private Transform _healthBarsRoot = null;

        [Header("Player")]
        [SerializeField] private CinemachineVirtualCamera _deathCam = null;
        //[SerializeField] private MonsterMovement _playerMovement = null;
        //[SerializeField] private MonsterInformation _playerInformation = null;
        //[SerializeField] private MonsterHealthBar _playerHealthBar = null;
        //[SerializeField] private PositionConstraint _cameraConstraint = null;
        /*[SerializeField] private List<LevelInformation> _levels = new List<LevelInformation>();

        [Space]

        [SerializeField] private TextMeshProUGUI _playerName = null;
        [SerializeField] private TextMeshProUGUI _enemyName = null;

        [Space]

        [SerializeField] private StartPanel _startPanel = null;
        [SerializeField] private WinnerPanel _winnerPanel = null;*/

        //private Monster _playerMonster = null;
        private int _levelNum = 0;
        private bool _inited = false;
        private bool _isPlayerWin = false;

        public static int TotalLevelsPassed
        {
            get => PlayerPrefs.GetInt(_TotalPassedLevelsKey, 0);
            set => PlayerPrefs.SetInt(_TotalPassedLevelsKey, value);
        }

        public IReadOnlyList<MonsterMovement> Enemies => _enemies;

        /*private void Update()
        {
            if (_playerMonster == null)
            {
                return;
            }

            Camera.main.transform.localPosition = Vector3.Lerp(Camera.main.transform.localPosition, Vector3.back * _playerMonster.CameraDistance, Time.deltaTime * _CameraDistanceChangingSpeedMultiplier);

            _abilityButton.gameObject.SetActive(_playerMonster.CanUseAbility);
        }*/

        private void OnEnable()
        {
            _player.Died += OnPlayerDied;
            
            foreach (var enemy in _enemies)
            {
                enemy.Monster.Died += OnEnemyDied;
                enemy.Monster.Killed += OnEnemyKilledByEnemy;
            }
        }

        private void OnDisable()
        {
            /*_playerMonster.LevelChanged -= OnLevelChanged;
            _playerMonster.Killed -= OnEnemyKilled;*/
            _player.Died -= OnPlayerDied;

            foreach (var enemy in _enemies)
            {
                enemy.Monster.Died -= OnEnemyDied;
                enemy.Monster.Killed -= OnEnemyKilledByEnemy;
            }
        }

        public void GameStarted()
        {
            Initialize(TotalLevelsPassed);
        }

        public void Initialize(int levelNum)
        {
            if (_inited)
            {
                return;
            }
            _inited = true;

            _levelNum = levelNum;

            _levelNumberText.text = (TotalLevelsPassed + 1).ToString();

            /*_playerMonster = Instantiate(playerInformation.MonsterPrefab, _playerSpawn);
            _playerMonster.InitializeAsPlayer(playerInformation, _enemies.Count);

            _playerMonster.LevelChanged += OnLevelChanged;
            _playerMonster.Killed += OnEnemyKilled;
            _playerMonster.Died += OnPlayerDied;

            _playerHealthBar.Initialize(_playerMonster, null);*/

            //var movement = _playerMonster.gameObject.AddComponent<MonsterMovement>();
            //movement.Initialize(_input.Value);

            //_cameraConstraint.SetSource(0, new ConstraintSource() { sourceTransform = _playerMonster.transform, weight = 1 });

            for (int i = 0; i < _enemies.Count; i++)
            {
                var enemy = _enemies[i];

                var healthBar = Instantiate(_healthBarPrefab, _healthBarsRoot);
                healthBar.Initialize(enemy.Monster);
            }

            //_level = _levels[levelNum];

            //_playerName.text = _level.PlayerName;
            //_enemyName.text = _level.EnemyName;

            AnalyticsExtensions.SendLevelStartEvent(_levelNum);
        }

        public void BackToMainMenu()
        {
            //_sceneTransition.Load(_mainMenuScene);
        }

        public void Restart()
        {
            _sceneTransition.ReloadCurrent(rootGOs =>
            {
                var level = rootGOs.Select(x => x.GetComponent<Level>()).First(x => x != null);
                level.Initialize(_levelNum);
            });
        }

        private IEnumerator RevivePlayer(float seconds)
        {
            yield return new WaitForSeconds(seconds);

            _deathCam.Priority = 0;

            _player.Revive();
        }

        private void OnLevelChanged(int level)
        {
            ActivateAbilityButton();
        }
        
        private void OnEnemyKilled(Monster monster, Transform enemy)
        {
            _wallet.Add(enemy, _killReward);

            /*if (!_enemies.Any(x => x.Monster.IsAlive))
            {
                _input.Value.Lock();

                _isPlayerWin = true;

                TotalLevelsPassed++;
                _winnerUI.Activate(true, _results);
                _playerMonster.RunWinningAnimation();
            }*/
        }

        private void OnEnemyKilledByEnemy(Monster monster, Transform enemy)
        {
            
        }

        private void OnEnemyDied(Monster monster, DamageSource source)
        {
            EnemyDied?.Invoke(monster);

            /*if (!_enemies.Any(x => x.Monster.IsAlive))// && _playerMonster.IsAlive)
            {
                _input.Value.Lock();

                _isPlayerWin = true;

                TotalLevelsPassed++;
                //_playerMonster.RunWinningAnimation();
            }*/
        }

        private void OnPlayerDied()
        {
            _deathCam.Priority = 10;

            StartCoroutine(RevivePlayer(2));
        }

        private void OnWinnerUIClicked()
        {
            if (_isPlayerWin)
            {
                BackToMainMenu();
                return;
            }

            BackToMainMenu();
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
