using AYellowpaper;
using MonsterArena.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterArena.Models
{
    [CreateAssetMenu(fileName = "New LevelInformation", menuName = "Monster Arena/Level Information", order = 40)]
    public class LevelInformation : ScriptableObject
    {
        [SerializeField] private InterfaceReference<IDeck> _playerDeck = null;
        [SerializeField] private string _playerName = "Player";
        [Space]
        [SerializeField] private InterfaceReference<IDeck> _enemyDeck = null;
        [SerializeField] private string _enemyName = "Enemy";

        public IDeck PlayerDeck => _playerDeck.Value;
        public string PlayerName => _playerName;
        public IDeck EnemyDeck => _enemyDeck.Value;
        public string EnemyName => _enemyName;
    }
}
