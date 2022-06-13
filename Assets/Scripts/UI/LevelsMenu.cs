using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MonsterArena
{
    [RequireComponent(typeof(SceneTransition))]
    public class LevelsMenu : MonoBehaviour
    {
        [SerializeField] private RectTransform _mainPanel = null;
        [SerializeField] private Camera _temporaryCamera = null;
        [SerializeField] private List<LevelButton> _buttons = new List<LevelButton>();

        private SceneTransition _sceneTransition = null;
        private Level _currentLevel = null;

        private void Awake()
        {
            _sceneTransition = GetComponent<SceneTransition>();

            _sceneTransition.Load(PlayerPrefs.GetString(Constants.ActiveLevelKey, _buttons[0].Scene), false, OnSceneLoaded);
        }

        private void OnEnable()
        {
            foreach (var button in _buttons)
            {
                button.Clicked += OnLevelButtonClicked;
            }
        }

        private void OnDisable()
        {
            foreach (var button in _buttons)
            {
                button.Clicked -= OnLevelButtonClicked;
            }
        }

        public void Open()
        {
            _mainPanel.DOMoveY(_mainPanel.sizeDelta.y, 1.0f);
        }

        private void ActivateButton(string scenePath)
        {
            for (int i = 1; i < _buttons.Count; i++)
            {
                var previousButton = _buttons[i - 1];
                var currentButton = _buttons[i];

                if (previousButton.Scene.ScenePath == scenePath)
                {
                    currentButton.Activate();
                    return;
                }
            }
        }

        private void OnLevelCompleted()
        {
            var currentScenePath = SceneManager.GetActiveScene().path;

            ActivateButton(currentScenePath);
        }

        private void OnLevelButtonClicked(SceneReference scene)
        {
            PlayerPrefs.DeleteKey(Constants.CurrentTaskIndexKey);
            PlayerPrefs.DeleteKey(Constants.CurrentTaskKey);
            PlayerPrefs.DeleteKey(Constants.CargoKey);
            PlayerPrefs.DeleteKey(Constants.WalletCoinsKey);
            PlayerPrefs.DeleteKey(Constants.HealthLevelKey);
            PlayerPrefs.DeleteKey(Constants.DamageLevelKey);
            PlayerPrefs.DeleteKey(Constants.CargoLevelKey);
            PlayerPrefs.DeleteKey(Constants.SpeedLevelKey);

            PlayerPrefs.SetString(Constants.ActiveLevelKey, scene);

            PlayerPrefs.Save();

            _sceneTransition.Load(scene, true, OnSceneLoaded);
            
            _mainPanel.DOMoveY(0, 1.0f);
        }
        
        private void OnSceneLoaded(Scene scene, GameObject[] rootObjects)
        {
            if (_currentLevel != null)
            {
                _currentLevel.Completed -= OnLevelCompleted;
                _currentLevel.LevelsMapRequested -= Open;
            }

            _currentLevel = rootObjects.Select(x => x.GetComponent<Level>()).FirstOrDefault(x => x != null);
            if (_currentLevel == null)
            {
                throw new InvalidOperationException();
            }

            _currentLevel.Completed += OnLevelCompleted;
            _currentLevel.LevelsMapRequested += Open;

            if (_currentLevel.IsCompleted)
            {
                ActivateButton(scene.path);
            }

            var eventSystem = rootObjects.Select(x => x.GetComponent<EventSystem>()).FirstOrDefault(x => x != null);
            Destroy(eventSystem.gameObject);

            _currentLevel.Initialize();
        }
    }
}
