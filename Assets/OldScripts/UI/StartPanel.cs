using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace MonsterArena.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class StartPanel : MonoBehaviour
    {
        public event Action Closed = null;

        [SerializeField] private RectTransform _topSlice = null;
        [SerializeField] private RectTransform _text = null;
        [SerializeField] private RectTransform _bottomSlice = null;
        [SerializeField] private ParticleSystem _effect = null;
        [SerializeField] private float _duration = 0.5f;

        private CanvasGroup _group = null;

        private void Awake()
        {
            _group = GetComponent<CanvasGroup>();

            _group.interactable = true;
            _group.blocksRaycasts = true;
        }

        private void Start()
        {
            var seq = DOTween.Sequence();

            //var topSlicePos = _topSlice.anchoredPosition.x;
            //var textPos = _text.anchoredPosition.x;
            //var bottomSlicePos = _bottomSlice.anchoredPosition.x;

            seq.Append(_topSlice.DOAnchorPosX(4000, _duration).SetEase(Ease.InCubic).From());
            seq.Join(_text.DOAnchorPosX(2000, _duration * 1.25f).SetDelay(_duration / 4).SetEase(Ease.InCubic).From().OnComplete(() =>
            {
                _effect.Play();

                Closed?.Invoke();
            }));
            //seq.Join(_bottomSlice.DOAnchorPosX(-4000, _duration).SetDelay(_duration / 12).SetEase(Ease.InCubic).From());
            seq.Join(_bottomSlice.DOAnchorPosX(-4000, _duration).SetDelay(_duration / -2f).SetEase(Ease.InCubic).From());

            seq.Append(_topSlice.DOAnchorPosX(-4000, _duration).SetEase(Ease.InCubic));
            seq.Join(_text.DOAnchorPosX(-2000, _duration * 1.25f).SetEase(Ease.InCubic));
            seq.Join(_bottomSlice.DOAnchorPosX(4000, _duration).SetEase(Ease.InCubic));

            seq.Join(_group.DOFade(0, _duration).SetDelay(_duration / 2).From(1));

            seq.OnComplete(() =>
            {
                _group.interactable = false;
                _group.blocksRaycasts = false;
            });
        }

        /*public void Initialize(string playerNickname, string enemyNickname)
        {
            _topText.text = enemyNickname;
            _bottomText.text = playerNickname;
        }*/
    }
}
