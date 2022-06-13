using System.Collections;
using System.Collections.Generic;
using MonsterArena.TasksSystem.Interfaces;
using TMPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MonsterArena
{
    [RequireComponent(typeof(CanvasGroup))]
    public class TaskView : MonoBehaviour
    {
        [SerializeField] private Image _icon = null;
        [SerializeField] private Slider _progressBar = null;
        [SerializeField] private TextMeshProUGUI _progressText = null;
        [SerializeField] private TextMeshProUGUI _description = null;

        private CanvasGroup _canvasGroup = null;
        private ILevelTaskModel _currentTask = null;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();

            OnTaskUpdated();
        }

        private void Update()
        {
            if (_currentTask == null)
            {
                return;
            }

            _progressBar.value = Mathf.Lerp(_progressBar.value, _currentTask.Progress, Time.deltaTime * 5);
        }

        public void SwapTask(ILevelTaskModel newTask)
        {
            if (_currentTask != null)
            {
                _currentTask.Updated -= OnTaskUpdated;
            }

            _currentTask = newTask;

            if (_currentTask != null)
            {
                _currentTask.Updated += OnTaskUpdated;
            }
            
            OnTaskUpdated();
        }

        private void OnTaskUpdated()
        {
            if (_currentTask == null || _currentTask.IsCompleted)
            {
                _canvasGroup.DOFade(0, 0.5f);

                return;
            }

            _canvasGroup.DOFade(1, 0.5f);
        
            _icon.sprite = _currentTask.Icon;
            _progressText.text = _currentTask.Status;
            _description.text = _currentTask.Description;
        }
    }
}
