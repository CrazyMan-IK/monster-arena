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
        public event Action<Monster> EnemyDied = null;
        public event Action LevelsMapRequested = null;
        public event Action Completed = null;

        [SerializeField] private List<MonsterMovement> _enemies = new List<MonsterMovement>();

        [Header("Scene")]
        [SerializeField] private int _levelNum = 0;
        [SerializeField] private Button _chooseLevelButton = null;
        [SerializeField] private LevelTasks _tasks = null;

        [Header("Logic")]
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
        private bool _inited = false;

        public IReadOnlyList<MonsterMovement> Enemies => _enemies;
        public bool IsCompleted => _tasks.IsCompleted;

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
            _chooseLevelButton.onClick.AddListener(OnChooseLevelButtonClicked);
            _tasks.Completed += OnAllTasksCompleted;
            _player.Died += OnPlayerDied;
            
            foreach (var enemy in _enemies)
            {
                enemy.Monster.Died += OnEnemyDied;
            }
        }

        private void OnDisable()
        {
            _chooseLevelButton.onClick.RemoveListener(OnChooseLevelButtonClicked);
            _tasks.Completed -= OnAllTasksCompleted;
            _player.Died -= OnPlayerDied;

            foreach (var enemy in _enemies)
            {
                enemy.Monster.Died -= OnEnemyDied;
            }
        }

        public void GameStarted()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (_inited)
            {
                return;
            }
            _inited = true;

            //_levelNumberText.text = (TotalLevelsPassed + 1).ToString();

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

        private IEnumerator RevivePlayer(float seconds)
        {
            yield return new WaitForSeconds(seconds);

            _deathCam.Priority = 0;

            _player.Revive();
        }

        private void OnChooseLevelButtonClicked()
        {
            LevelsMapRequested?.Invoke();
        }
        
        private void OnAllTasksCompleted()
        {
            Completed?.Invoke();
        }

        private void OnEnemyDied(Monster monster, DamageSource source)
        {
            EnemyDied?.Invoke(monster);
        }

        private void OnPlayerDied()
        {
            _deathCam.Priority = 10;

            StartCoroutine(RevivePlayer(2));
        }
    }
}
