using MonsterArena.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterArena
{
    public class JoystickInput : MonoBehaviour, IInput
    {
        public event Action AbilityUsed = null;

        [SerializeField] private Joystick _joystick = null;

        public Vector2 Direction { get; private set; }

        private void Update()
        {
            Direction = _joystick.Direction;
        }
    }
}
