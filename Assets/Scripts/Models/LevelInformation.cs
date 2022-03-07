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
        [SerializeField] private InterfaceReference<IDeck> _enemyDeck = null;

        public IDeck PlayerDeck => _playerDeck.Value;
        public IDeck EnemyDeck => _enemyDeck.Value;
    }
}
