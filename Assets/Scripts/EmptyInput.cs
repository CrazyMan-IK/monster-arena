using System;
using UnityEngine;
using URandom = UnityEngine.Random;
using MonsterArena.Interfaces;

namespace MonsterArena
{
    public class EmptyInput : MonoBehaviour, IInput
    {
        public event Action AbilityUsed = null;
        public event Action PropThrowed = null;

        public Vector2 Direction => Vector2.zero;

        private void Awake()
        {
            if (TryGetComponent(out Target target))
            {
                target.TargetColor = URandom.ColorHSV(0, 1, 0.8f, 1, 0.8f, 1, 1, 1);
            }
        }

        public void Lock()
        {
            
        }
    }
}
