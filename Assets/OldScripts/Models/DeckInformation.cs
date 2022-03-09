using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonsterArena.Interfaces;

namespace MonsterArena.Models
{
    [CreateAssetMenu(fileName = "New DeckInformation", menuName = "Monster Arena/Deck Information", order = 50)]
    public class DeckInformation : ScriptableObject, IDeck
    {
        [SerializeField] private List<MonsterInformation> _monsters = new List<MonsterInformation>();

        public IReadOnlyList<MonsterInformation> Monsters => _monsters;
    }
}
