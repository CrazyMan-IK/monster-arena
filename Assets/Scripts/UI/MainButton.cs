using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using MonsterArena.Models;

namespace MonsterArena.UI
{
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Button))]
    public class MainButton : MonoBehaviour
    {
        public event Action GameStarted = null;
        public event Action Unlocked = null;

        [SerializeField] private Wallet _wallet = null;
        [SerializeField] private RectTransform _rootTransform = null;
        [SerializeField] private TextMeshProUGUI _playText = null;
        [SerializeField] private TextMeshProUGUI _unlockText = null;
        [SerializeField] private float _animationDuration = 0.5f;
        [SerializeField] private Color _unlockedColor = Color.green;
        [SerializeField] private Color _lockedColor = Color.yellow;

        private Image _image = null;
        private Button _button = null;
        private MonsterInformation _information = null;
        private string _unlockTextValue = "";
        private bool _unlockingAnimationRunning = false;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _button = GetComponent<Button>();

            _unlockTextValue = _unlockText.text;
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnClicked);
        }

        public void ChangeInformation(MonsterInformation information)
        {
            _information = information;

            _playText.gameObject.SetActive(information.IsUnlocked);
            _unlockText.gameObject.SetActive(!information.IsUnlocked);
            _unlockText.text = _unlockTextValue.Replace("${}", information.Price.ToString());

            _image.DOBlendableColor(information.IsUnlocked ? _unlockedColor : _lockedColor, _animationDuration);

            _button.interactable = _information.IsUnlocked || _wallet.HaveCoins(_information.Price);
        }

        public void CompleteAnimation()
        {
            _unlockingAnimationRunning = false;
        }

        private void OnClicked()
        {
            if (_information.IsUnlocked)
            {
                GameStarted?.Invoke();

                return;
            }

            StartCoroutine(Unlock());

            _information.Unlock();

            _playText.gameObject.SetActive(true);
            _unlockText.gameObject.SetActive(false);
            _image.DOBlendableColor(_unlockedColor, _animationDuration);
        }

        private IEnumerator Unlock()
        {
            //yield return _rootTransform.DOSizeDelta(Vector2.one * 1024, _animationDuration / 2).WaitForCompletion();
            _rootTransform.DOSizeDelta(Vector2.one * 1024, _animationDuration / 2);

            yield return _wallet.Take(_information.Price);

            Unlocked?.Invoke();

            _unlockingAnimationRunning = true;
            yield return new WaitWhile(() => _unlockingAnimationRunning);

            yield return _rootTransform.DOSizeDelta(Vector2.zero, _animationDuration / 2).WaitForCompletion();
        }
    }
}
