using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MonsterArena.UI
{
    public class DoubleClickPanel : MonoBehaviour, IPointerClickHandler
    {
        public event Action Clicked = null;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount == 2)
            {
                Clicked?.Invoke();
            }
        }
    }
}
