using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper;
using MonsterArena.TasksSystem.Interfaces;
using System.Linq;

namespace MonsterArena.TasksSystem
{
    public class LevelTasks : MonoBehaviour
    {
        private const string _CurrentTaskKey = "_currentTask";
        private const string _CurrentTaskIndexKey = "_currentTaskIndex";

        [SerializeField] private List<ILevelTaskModel> _tasks = new List<ILevelTaskModel>();

        public int TasksCount => _tasks.Count;
        public ILevelTaskModel CurrentTask
        {
            get => _tasks[CurrentTaskIndex];
            set => _tasks[CurrentTaskIndex] = value;
        }

        public int CurrentTaskIndex { get; private set; }

        private void Awake()
        {
            CurrentTaskIndex = PlayerPrefs.GetInt(_CurrentTaskIndexKey, 0);
            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(_CurrentTaskKey, "{}"), CurrentTask);

            CurrentTask.Enable();
        }

        private void OnEnable()
        {
            foreach (var task in _tasks)
            {
                task.Completed += OnTaskCompleted;
            }
        }

        private void OnDisable()
        {
            foreach (var task in _tasks)
            {
                task.Completed -= OnTaskCompleted;
            }
        }

        private void OnValidate()
        {
            var tasks = GetComponentsInChildren<ILevelTask>();

            _tasks = tasks.Select(x => x.ToModel()).ToList();
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
            PlayerPrefs.SetString(_CurrentTaskKey, JsonUtility.ToJson(CurrentTask));
        }

        private void OnTaskCompleted()
        {
            CurrentTaskIndex++;
            CurrentTask.Enable();

            PlayerPrefs.SetInt(_CurrentTaskIndexKey, CurrentTaskIndex);
        }
    }
}
