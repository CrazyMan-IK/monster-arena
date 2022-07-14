using System;
using UnityEngine;
using URandom = UnityEngine.Random;
using MonsterArena.Interfaces;

namespace MonsterArena
{
    [RequireComponent(typeof(Monster))]
    public class BoatAI : MonoBehaviour, IInput
    {
        public event Action AbilityUsed = null;
        public event Action PropThrowed = null;

        [SerializeField] private float _radius;

        private Monster _monster = null;
        private float time;

        public Vector2 Direction { get; private set; }

        private void Awake()
        {
            _monster = GetComponent<Monster>();
        }

        private void OnEnable()
        {
            _monster.Died += OnDied;
        }

        private void OnDisable()
        {
            _monster.Died -= OnDied;
        }

        private void Update()
        {
            if (_monster.HelicopterInRange(out Helicopter _) || _monster.Attacking || !_monster.IsAlive)
            {
                Direction = Vector2.zero;

                return;
            }

            time += Time.deltaTime;
            float scaledTime = time / _radius;
            Direction = new Vector2(Mathf.Cos(scaledTime), Mathf.Sin(scaledTime));
        }

        public void Lock()
        {
        }

        private void OnDied(Monster monster, DamageSource source)
        {
            time = 0;
        }
    }
}