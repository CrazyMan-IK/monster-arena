using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;
using MonsterArena.UI;
using MonsterArena.Models;
using UnityEngine.Animations;

namespace MonsterArena
{
    public class Level : MonoBehaviour
    {
        private const string _TotalPassedLevelsKey = "_passedLevels";

        [SerializeField] private SceneReference _mainMenuScene = null;
        [SerializeField] private SceneTransition _sceneTransition = null;
        [SerializeField] private MonsterMovement _playerMovement = null;
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

        private int _levelNum = 0;
        private bool _inited = false;
        private bool _isPlayerWin = false;

        public int TotalLevelsPassed
        {
            get => PlayerPrefs.GetInt(_TotalPassedLevelsKey, 0);
            set => PlayerPrefs.SetInt(_TotalPassedLevelsKey, value);
        }

        private void Awake()
        {
            //_startPanel.Closed += OnStartPanelClosed;
            //_winnerPanel.Clicked += OnWinnerPanelClicked;

            //_startPanel.Initialize(_playerDeck.Nickname, _enemyDeck.Nickname);
            //Initialize(0, _playerInformation);
        }

        /*private void Update()
        {
            if (!_playerDeck.HasAliveMonsters)
            {
                _enemyDeck.ActivateWinAnimation();
                _winnerPanel.Activate(false);

                _isPlayerWin = false;
                enabled = false;

                return;
            }

            if (!_enemyDeck.HasAliveMonsters)
            {
                _playerDeck.ActivateWinAnimation();
                _winnerPanel.Activate(true);

                _isPlayerWin = true;
                enabled = false;

                return;
            }
        }*/

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

            var monster = Instantiate(playerInformation.MonsterPrefab, _playerMovement.transform);
            monster.Initialize(playerInformation);

            _playerMovement.Initialize(monster);
            _cameraConstraint.SetSource(0, new ConstraintSource() { sourceTransform = monster.transform, weight = 1 });

            _playerHealthBar.Initialize(monster);

            //_level = _levels[levelNum];

            //_playerName.text = _level.PlayerName;
            //_enemyName.text = _level.EnemyName;
        }

        public void LoadNextLevel()
        {
            var nextLevelNum = _levelNum + 1;
            //if (nextLevelNum >= _levels.Count)
            {
                nextLevelNum = 0;
            }

            TotalLevelsPassed++;

            _sceneTransition.Load(_mainMenuScene);
        }

        private void OnWinnerPanelClicked()
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
        }

        private void OnStartPanelClosed()
        {
            //_startPanel.Closed -= OnStartPanelClosed;
        }
    }
}
