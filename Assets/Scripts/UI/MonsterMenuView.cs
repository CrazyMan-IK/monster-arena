using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using MonsterArena.Models;

namespace MonsterArena.UI
{
    [RequireComponent(typeof(Monster))]
    public class MonsterMenuView : MonoBehaviour
    {
        public event Action WinAnimationCompleted = null;

        private const float _AnimationDuration = 0.5f;
        private const string _ColorKey = "_Color";
        private const string _ColorDimKey = "_ColorDim";

        private Renderer[] _renderers = null;
        private Monster _monster = null;
        private Color _baseColor = Color.white;
        private Color _baseShadowColor = Color.white;

        public MonsterInformation Information => _monster.Information;

        private void Awake()
        {
            _monster = GetComponent<Monster>();

            _renderers = GetComponentsInChildren<Renderer>();
        }

        private void OnEnable()
        {
            _monster.WinAnimationCompleted += OnWinAnimationCompleted;
        }

        private void OnDisable()
        {
            _monster.WinAnimationCompleted -= OnWinAnimationCompleted;
        }

        private void Start()
        {
            _baseColor = _monster.Renderer.material.GetColor(_ColorKey);
            _baseShadowColor = _monster.Renderer.material.GetColor(_ColorDimKey);

            if (!Information.IsUnlocked)
            {
                var targetColor = Color.black; //Color.white / 8;

                _monster.Renderer.material.DOColor(targetColor, _ColorKey, _AnimationDuration);
                _monster.Renderer.material.DOColor(targetColor, _ColorDimKey, _AnimationDuration);
            }

            _monster.DisableRadiusPreview();
        }

        private void Update()
        {
            var isActive = Vector3.Dot(transform.forward, Camera.main.transform.forward) < 0;

            foreach (var renderer in _renderers)
            {
                renderer.enabled = isActive;
            }
        }

        public void Unlock()
        {
            _monster.Renderer.material.DOColor(_baseColor, _ColorKey, _AnimationDuration);
            _monster.Renderer.material.DOColor(_baseShadowColor, _ColorDimKey, _AnimationDuration);

            _monster.RunWinningAnimationOnce();
        }
        
        private void OnWinAnimationCompleted()
        {
            WinAnimationCompleted?.Invoke();
        }
    }
}
