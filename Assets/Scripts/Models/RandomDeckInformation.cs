using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonsterArena.Interfaces;

namespace MonsterArena.Models
{
    [CreateAssetMenu(fileName = "New DeckInformation", menuName = "Monster Arena/Deck Information", order = 50)]
    public class RandomDeckInformation : ScriptableObject, IDeck
    {
        [SerializeField] private List<MonsterInformation> _monsters = new List<MonsterInformation>();
        [SerializeField] private int _count = 0;

        private List<MonsterInformation> _currentMonsters = null;

        public IReadOnlyList<MonsterInformation> Monsters
        {
            get
            {
                if (_currentMonsters == null)
                {
                    _currentMonsters = new List<MonsterInformation>();

                    for (int i = 0; i < _count; i++)
                    {
                        _currentMonsters.Add(_monsters[Random.Range(0, _monsters.Count)]);
                    }
                }

                return _monsters;
            }
        }
    }
}
