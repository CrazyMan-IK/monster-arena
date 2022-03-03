using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterArena.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class WinnerPanel : MonoBehaviour
    {
        private CanvasGroup _group = null;

        private void Awake()
        {
            _group = GetComponent<CanvasGroup>();

            _group.alpha = 0;
            _group.interactable = false;
            _group.blocksRaycasts = false;
        }

        public void Show()
        {
            _group.DOFade(1, 0.5f);
            _group.interactable = true;
            _group.blocksRaycasts = true;
        }
    }
}
