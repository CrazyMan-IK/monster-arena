using MonsterArena.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MonsterArena
{
    public class JoystickInput : MonoBehaviour, IInput
    {
        public event Action AbilityUsed = null;

        [SerializeField] private Joystick _joystick = null;

        private bool _isLocked = false;

        public Vector2 Direction { get; private set; }

        private void Update()
        {
            if (_isLocked)
            {
                Direction = Vector2.zero;
                return;
            }

            Direction = _joystick.Direction;
        }

        public void Lock()
        {
            _isLocked = true;

            var data = new PointerEventData(EventSystem.current);
            data.position = Input.mousePosition;

            _joystick.OnPointerUp(data);
        }
    }
}
