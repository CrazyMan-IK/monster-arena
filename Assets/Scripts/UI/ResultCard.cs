using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MonsterArena.UI
{
    public class ResultCard : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _name = null;
        [SerializeField] private TextMeshProUGUI _killCount = null;

        public void Initialize(string name, int kills)
        {
            _name.text = name;
            _killCount.text = $"x {kills}";
        }
    }
}
