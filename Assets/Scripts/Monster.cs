using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonsterArena.Models;

namespace MonsterArena
{
    [RequireComponent(typeof(MonsterAnimation))]
    public class Monster : MonoBehaviour
    {
        public event Action WinAnimationCompleted = null;

        [SerializeField] private Renderer _renderer = null;
        [SerializeField] private Collider _collider = null;
        [SerializeField] private MonsterAnimationEventsRepeater _repeater = null;

        private MonsterAnimation _animation = null;
        private MonsterInformation _information = null;
        private float _hp = 0;

        public Renderer Renderer => _renderer;
        public Collider Collider => _collider;
        public Rigidbody Rigidbody => _animation.Rigidbody;
        public MonsterInformation Information => _information;
        public float HP => _hp / _information.HP;

        private void Awake()
        {
            _animation = GetComponent<MonsterAnimation>();
        }

        private void OnEnable()
        {
            _repeater.Winned += OnWinAnimationCompleted;
        }

        private void OnDisable()
        {
            _repeater.Winned -= OnWinAnimationCompleted;
        }

        public void Initialize(MonsterInformation information)
        {
            _information = information;
            _hp = _information.HP;

            _animation.Initialize(information);
        }

        public void RunWinningAnimation()
        {
            _animation.ActivateWinAnimation(false);
        }

        public void RunWinningAnimationOnce()
        {
            _animation.ActivateWinAnimation(true);
        }

        private void OnWinAnimationCompleted()
        {
            WinAnimationCompleted?.Invoke();
        }
    }
}
