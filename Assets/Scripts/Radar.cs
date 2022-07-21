using System;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;

namespace MonsterArena
{
    public class Radar : MonoBehaviour
    {
        [SerializeField] private Material _cloudsMaterial = null;
        [SerializeField] private Transform _cloudsCollider = null;
        [SerializeField] private CanvasGroup _worldUI = null;
        [SerializeField] private CinemachineVirtualCamera _animationCamera = null;
        [SerializeField] private float _defaultOffset = 70;

        private float _currentOffset = 0;
        private float _targetOffset = 0;

        private void Awake()
        {
            _currentOffset = _targetOffset;
            SetMaterialOffset();
        }

        private void Update()
        {
            if (Mathf.Abs(_currentOffset - _targetOffset) <= 0.0001f)
            {
                return;
            }

            _currentOffset = Mathf.Lerp(_currentOffset, _targetOffset, Time.deltaTime * 1.5f);

            SetMaterialOffset();
            _cloudsCollider.transform.localScale = _currentOffset / 210.0f * Vector3.one;
        }

        public void SetOffset(float newOffset, bool animated = false)
        {
            if (newOffset <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newOffset));
            }
            
            if (animated)
            {
                _animationCamera.Priority = 20;

                var sequence = DOTween.Sequence();

                //sequence.AppendInterval(0.5f);
                sequence.Append(_worldUI.DOFade(0, 1.0f));
                sequence.AppendCallback(() => _targetOffset = newOffset);
                sequence.AppendInterval(1.5f);
                sequence.AppendCallback(ResetAnimation);
                sequence.Append(_worldUI.DOFade(1, 1.0f));
            }
            else
            {
                _targetOffset = newOffset;
            }
        }

        private void SetMaterialOffset()
        {
            _cloudsMaterial.SetFloat(Constants.Offset, _currentOffset);
        }

        private void ResetAnimation()
        {
            _animationCamera.Priority = 0;
        }
    }
}
