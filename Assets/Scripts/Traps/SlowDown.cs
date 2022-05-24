using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterArena.Traps
{
    public class SlowDown : MonoBehaviour
    {
        [SerializeField] private float _damage = 1;
        [SerializeField] private float _duration = 0.5f;
        [SerializeField] private float _speedFactor = 0.7f;

        private readonly List<MonsterMovement> _monsters = new List<MonsterMovement>();

        private Timer _timer = null;

        private void OnEnable()
        {
            _timer ??= new Timer(_duration);
            _timer.Ticked += OnTimerTicked;
        }

        private void OnDisable()
        {
            _timer.Ticked -= OnTimerTicked;
        }

        private void Update()
        {
            _timer.Update(Time.deltaTime);
        }

        private void LateUpdate()
        {
            foreach (var monster in _monsters)
            {
                monster.ApplySpeedFactor(_speedFactor);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out MonsterMovement movement))
            {
                _monsters.Add(movement);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out MonsterMovement movement))
            {
                _monsters.Remove(movement);
            }
        }

        private void OnTimerTicked()
        {
            foreach (var monster in _monsters)
            {
                monster.Monster.TakeDamage(_damage, DamageSource.Other);
            }
        }
    }
}
