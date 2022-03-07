using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;

namespace MonsterArena.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class WinnerPanel : MonoBehaviour, IPointerClickHandler
    {
        public event Action Clicked = null;

        [SerializeField] private GameObject _winPanel = null;
        [SerializeField] private GameObject _failPanel = null;

        private CanvasGroup _group = null;

        private void Awake()
        {
            _group = GetComponent<CanvasGroup>();

            _group.alpha = 0;
            _group.interactable = false;
            _group.blocksRaycasts = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Clicked?.Invoke();
        }

        public void Activate(bool isPlayerWins)
        {
            _group.DOFade(1, 0.5f);
            _group.interactable = true;
            _group.blocksRaycasts = true;

            _winPanel.SetActive(isPlayerWins);
            _failPanel.SetActive(!isPlayerWins);
        }
    }
}
