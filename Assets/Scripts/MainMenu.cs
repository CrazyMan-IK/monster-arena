using MonsterArena.Models;
using MonsterArena.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterArena
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private List<MonsterInformation> _availableMonsters = new List<MonsterInformation>();
        [SerializeField] private Carousel _carousel = null;

        private void Awake()
        {
            _carousel.Initialize(_availableMonsters);
        }
    }
}
