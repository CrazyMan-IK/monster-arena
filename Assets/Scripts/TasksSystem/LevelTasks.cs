using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;
using AYellowpaper;
using MonsterArena.TasksSystem.Interfaces;

namespace MonsterArena.TasksSystem
{
    public class LevelTasks : MonoBehaviour
    {
        public event Action Completed = null;

        [SerializeField] private CanvasGroup _chooseLevelButton = null;
        [SerializeField] private TaskView _taskView = null;
        [SerializeField] private Slider _totalProgressBar = null;
        [SerializeField] private TextMeshProUGUI _totalProgressText = null;
        [SerializeField] private List<InterfaceReference<ILevelTask>> _tasks = new List<InterfaceReference<ILevelTask>>();

        private readonly List<ILevelTaskModel> _taskModels = new List<ILevelTaskModel>();

        public ILevelTaskModel CurrentTask
        {
            get => _taskModels.ElementAtOrDefault(CurrentTaskIndex);
            set => _taskModels[CurrentTaskIndex] = value;
        }

        public int CurrentTaskIndex { get; private set; }
        public bool IsCompleted => CurrentTaskIndex >= _tasks.Count;

        private void Awake()
        {
            _taskModels.AddRange(_tasks.Select(x => x.Value.ToModel()));

            CurrentTaskIndex = PlayerPrefs.GetInt(Constants.CurrentTaskIndexKey, 0);
            UpdateProgressBarText();
            if (IsCompleted)
            {
                _chooseLevelButton.DOFade(1, 0.5f).OnComplete(() => {
                    _chooseLevelButton.interactable = true;
                    _chooseLevelButton.blocksRaycasts = true;
                });

                return;
            }
            
            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(Constants.CurrentTaskKey, "{}"), CurrentTask);
        }

        private void Start()
        {
            if (IsCompleted)
            {
                return;
            }
            
            CurrentTask.Enable();

            _taskView.SwapTask(CurrentTask);
        }

        private void Update()
        {
            _totalProgressBar.value = Mathf.Lerp(_totalProgressBar.value, CurrentTaskIndex * 1.0f / _tasks.Count, Time.deltaTime * 5);
        }

        private void OnEnable()
        {
            foreach (var task in _taskModels)
            {
                task.Completed += OnTaskCompleted;
            }
        }

        private void OnDisable()
        {
            foreach (var task in _taskModels)
            {
                task.Completed -= OnTaskCompleted;
            }
        }

        private void OnValidate()
        {
            _tasks = GetComponentsInChildren<ILevelTask>().Select(x => new InterfaceReference<ILevelTask>() { Value = x } ).ToList();
        }

        private void OnApplicationPause(bool pause)
        {
            SaveCurrentState();
        }

        private void OnApplicationQuit()
        {
            SaveCurrentState();
        }

        private void SaveCurrentState()
        {
            if (IsCompleted)
            {
                PlayerPrefs.SetString(Constants.CurrentTaskKey, "{}");

                return;
            }
        
            PlayerPrefs.SetString(Constants.CurrentTaskKey, JsonUtility.ToJson(CurrentTask));
        }

        private void OnTaskCompleted()
        {
            CurrentTaskIndex++;
            UpdateProgressBarText();

            PlayerPrefs.SetInt(Constants.CurrentTaskIndexKey, CurrentTaskIndex);

            if (IsCompleted)
            {
                _chooseLevelButton.DOFade(1, 0.5f).OnComplete(() => {
                    _chooseLevelButton.interactable = true;
                    _chooseLevelButton.blocksRaycasts = true;
                });

                Completed?.Invoke();
                return;
            }
            if (CurrentTask.IsCompleted)
            {
                OnTaskCompleted();
                return;
            }
            CurrentTask.Enable();

            _taskView.SwapTask(CurrentTask);
        }

        private void UpdateProgressBarText()
        {
            _totalProgressText.text = $"{CurrentTaskIndex} / {_tasks.Count}";
        }
    }
}
