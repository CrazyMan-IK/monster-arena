using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MonsterArena.UI
{
    public class MainButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _playText = null;
        [SerializeField] private TextMeshProUGUI _unlockText = null;

        public void Lock()
        {
            _playText.gameObject.SetActive(false);
            _unlockText.gameObject.SetActive(true);
        }

        public void Unlock()
        {
            _playText.gameObject.SetActive(true);
            _unlockText.gameObject.SetActive(false);
        }
    }
}
