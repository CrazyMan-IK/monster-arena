using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MonsterArena
{
    [RequireComponent(typeof(Button))]
    public class LevelButton : MonoBehaviour
    {
        public event Action<SceneReference> Clicked = null;

        [SerializeField] private SceneReference _scene = null;
        [SerializeField] private bool _isForceActived = false;

        private Button _button = null;

        public SceneReference Scene => _scene;
        public bool IsActive { get; private set; }

        private void Awake()
        {
            _button = GetComponent<Button>();

            IsActive = _isForceActived || PlayerPrefs.GetInt($"_levelEnabled {_scene.ScenePath}", 0) == 1;
            _button.enabled = IsActive;
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnButtonClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnButtonClicked);
        }

        public void Activate()
        {
            PlayerPrefs.SetInt($"_levelEnabled {_scene.ScenePath}", 1);
        
            IsActive = true;
            _button.enabled = true;
        }

        private void OnButtonClicked()
        {
            Clicked?.Invoke(_scene);
        }
    }
}
