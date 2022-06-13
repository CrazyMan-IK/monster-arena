using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonsterArena.UI;
using MonsterArena.Interfaces;
using AYellowpaper;

namespace MonsterArena
{
    public class MenuZone : MonoBehaviour, IZone
    {
        [SerializeField] private InterfaceReference<IZoneMenu> _menu = null;

        private bool _isInZone = false;

        public void Enter()
        {
            if (_isInZone)
            {
                return;
            }

            _isInZone = true;
            _menu.Value.Activate();
        }

        public void Exit()
        {
            if (!_isInZone)
            {
                return;
            }

            _isInZone = false;
            _menu.Value.Deactivate();
        }
    }
}
