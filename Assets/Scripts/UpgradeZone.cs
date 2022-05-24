using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonsterArena.UI;
using MonsterArena.Interfaces;

namespace MonsterArena
{
    public class UpgradeZone : MonoBehaviour, IZone
    {
        [SerializeField] private UpgradeMenu _menu = null;

        private bool _isInZone = false;

        public void Enter()
        {
            if (_isInZone)
            {
                return;
            }

            _isInZone = true;
            _menu.gameObject.SetActive(true);
        }

        public void Exit()
        {
            if (!_isInZone)
            {
                return;
            }

            _isInZone = false;
            _menu.gameObject.SetActive(false);
        }
    }
}
