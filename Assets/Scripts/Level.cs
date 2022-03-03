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
        [SerializeField] private WinnerPanel _winnerPanel = null;

        private void Update()
        {
            if (!_playerDeck.HasAliveMonsters)
            {
                _enemyDeck.ActivateWinAnimation();
                _winnerPanel.Show();
                enabled = false;

                return;
            }

            if (!_enemyDeck.HasAliveMonsters)
            {
                _playerDeck.ActivateWinAnimation();
                _winnerPanel.Show();
                enabled = false;

                return;
            }
        }
    }
}
