using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using MonsterArena.Models;

namespace MonsterArena.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class WinnerPanel : MonoBehaviour
    {
        public event Action Clicked = null;

        [SerializeField] private float _cardAnimationDuration = 0.3f;
        [SerializeField] private GameObject _winPanel = null;
        [SerializeField] private GameObject _failPanel = null;
        [SerializeField] private Button _returnButton = null;

        private CanvasGroup _group = null;

        private void Awake()
        {
            _group = GetComponent<CanvasGroup>();

            _group.alpha = 0;
            _group.interactable = false;
            _group.blocksRaycasts = false;
        }

        private void OnEnable()
        {
            _returnButton.onClick.AddListener(OnClicked);
        }

        private void OnDisable()
        {
            _returnButton.onClick.RemoveListener(OnClicked);
        }
        
        public void Activate(bool isPlayerWins)
        {
            _group.DOFade(1, 0.5f);
            _group.interactable = true;
            _group.blocksRaycasts = true;

            _winPanel.SetActive(isPlayerWins);
            _failPanel.SetActive(!isPlayerWins);
        }

        private void OnClicked()
        {
            Clicked?.Invoke();
        }
    }
}
