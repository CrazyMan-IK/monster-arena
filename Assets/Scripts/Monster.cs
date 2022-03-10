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
        [SerializeField] private MonsterAnimationEventsRepeater _repeater = null;

        private MonsterAnimation _animation = null;
        private MonsterInformation _information = null;

        public Renderer Renderer => _renderer;
        public MonsterInformation Information => _information;

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
