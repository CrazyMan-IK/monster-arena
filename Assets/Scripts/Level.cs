using MonsterArena.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterArena
{
    public class Level : MonoBehaviour
    {
        [SerializeField] private Deck _playerDeck = null;
        [SerializeField] private Deck _enemyDeck = null;
        [SerializeField] private EnemyAI _enemyAI = null;
        [SerializeField] private StartPanel _startPanel = null;
        [SerializeField] private WinnerPanel _winnerPanel = null;

        private void Awake()
        {
            _startPanel.Closed += OnStartPanelClosed;
        }

        private void Update()
        {
            if (!_playerDeck.HasAliveMonsters)
            {
                _enemyDeck.ActivateWinAnimation();
                _winnerPanel.Activate(false);
                enabled = false;

                return;
            }

            if (!_enemyDeck.HasAliveMonsters)
            {
                _playerDeck.ActivateWinAnimation();
                _winnerPanel.Activate(true);
                enabled = false;

                return;
            }
        }

        private void OnStartPanelClosed()
        {
            _startPanel.Closed -= OnStartPanelClosed;

            _enemyAI.StartAI();
        }
    }
}
