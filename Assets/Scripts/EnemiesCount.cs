using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MonsterArena
{
    public class EnemiesCount : MonoBehaviour
    {
        [SerializeField] private Level _level = null;
        [SerializeField] private TextMeshProUGUI _text = null;

        private void Update()
        {
            _text.text = $"{_level.Enemies.Count(x => x.Monster.IsAlive)}/{_level.Enemies.Count}";
        }
    }
}
