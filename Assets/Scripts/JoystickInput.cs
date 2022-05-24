using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MonsterArena.Interfaces;

namespace MonsterArena
{
    public class JoystickInput : MonoBehaviour, IInput, IPointerClickHandler
    {
        public event Action AbilityUsed = null;
        public event Action PropThrowed = null;

        [SerializeField] private Joystick _joystick = null;
        [SerializeField] private Button _abilityButton = null;
        //[SerializeField] private DoubleClickPanel _doubleClickPanel = null;

        private bool _isLocked = false;

        public Vector2 Direction { get; private set; }

        private void OnEnable()
        {
            _abilityButton.onClick.AddListener(OnAbilityUsed);
            //_doubleClickPanel.Clicked += OnDoubleClick;
        }

        private void OnDisable()
        {
            _abilityButton.onClick.RemoveListener(OnAbilityUsed);
            //_doubleClickPanel.Clicked -= OnDoubleClick;
        }

        private void Update()
        {
            if (_isLocked)
            {
                Direction = Vector2.zero;
                return;
            }

            Direction = _joystick.Direction;

            if (Input.GetKeyDown(KeyCode.Q) && _abilityButton.gameObject.activeSelf && _abilityButton.interactable)
            {
                OnAbilityUsed();
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                //PropThrowed?.Invoke();
            }
        }

        public void Lock()
        {
            _isLocked = true;

            var data = new PointerEventData(EventSystem.current);
            data.position = Input.mousePosition;

            _joystick.OnPointerUp(data);
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount == 2)
            {
                //PropThrowed?.Invoke();
            }
        }

        private void OnAbilityUsed()
        {
            AbilityUsed?.Invoke();

            _abilityButton.interactable = false;
        }

        /*private void OnDoubleClick()
        {
            PropThrowed?.Invoke();
        }*/
    }
}
