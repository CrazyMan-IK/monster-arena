using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterArena
{
    public class Level : MonoBehaviour
    {
        [SerializeField] private Deck _playerDeck = null;
        [SerializeField] private Deck _enemyDeck = null;

        private void Update()
        {
            if (!_playerDeck.HasAliveMonsters)
            {
                _enemyDeck.ActivateWinAnimation();
                enabled = false;

                return;
            }

            if (!_enemyDeck.HasAliveMonsters)
            {
                _playerDeck.ActivateWinAnimation();
                enabled = false;

                return;
            }
        }
    }
}
