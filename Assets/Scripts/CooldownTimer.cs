using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace MonsterArena
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class CooldownTimer : MonoBehaviour
    {
        [SerializeField] private Deck _deck = null;

        private TextMeshProUGUI _text = null;

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
            _text.text = _deck.CooldownTime.ToString("0.00");
        }
    }
}
