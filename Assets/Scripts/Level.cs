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

namespace MonsterArena
{
    public class Level : MonoBehaviour
    {
        private const string _TotalPassedLevelsKey = "_passedLevels";

        [SerializeField] private SceneReference _mainMenuScene = null;
        [SerializeField] private SceneTransition _sceneTransition = null;
        /*[SerializeField] private List<LevelInformation> _levels = new List<LevelInformation>();

        [Space]

        [SerializeField] private TextMeshProUGUI _playerName = null;
        [SerializeField] private TextMeshProUGUI _enemyName = null;

        [Space]

        [SerializeField] private StartPanel _startPanel = null;
        [SerializeField] private WinnerPanel _winnerPanel = null;*/

        private LevelInformation _level = null;
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
            Initialize(0);
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

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                LoadNextLevel();
            }
        }

        public void GameStarted()
        {
            //Initialize(TotalLevelsPassed % _levels.Count);
        }

        public void Initialize(int levelNum)
        {
            if (_inited)
            {
                return;
            }
            _inited = true;

            _levelNum = levelNum;
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
                level.Initialize(_levelNum);
            });
        }

        private void OnStartPanelClosed()
        {
            //_startPanel.Closed -= OnStartPanelClosed;
        }
    }
}
