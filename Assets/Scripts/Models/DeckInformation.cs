using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterArena.Models
{
    [CreateAssetMenu(fileName = "New DeckInformation", menuName = "Monster Arena/Deck Information", order = 50)]
    public class DeckInformation : ScriptableObject
    {
        [SerializeField] private List<MonsterInformation> _monsters = new List<MonsterInformation>();

        public List<MonsterInformation> Monsters => _monsters;
    }
}
