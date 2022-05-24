using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MonsterArena
{
    [RequireComponent(typeof(Slider))]
    [RequireComponent(typeof(CanvasGroup))]
    public class HelicopterHealthBar : MonoBehaviour
    {
        [SerializeField] private Helicopter _player = null;
        [SerializeField] private TextMeshProUGUI _level = null;
        [SerializeField] private TextMeshProUGUI _cargo = null;
        [SerializeField] private Vector3 _offset = Vector3.zero;
        [SerializeField] private float _lerpSpeedMultiplier = 5;

        private Slider _bar = null;
        private CanvasGroup _group = null;
        private float _targetValue = 0;

        private void Awake()
        {
            _bar = GetComponent<Slider>();
            _group = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            //OnPlayerCargoChanged();
            OnPlayerLevelChanged();
        }

        private void OnEnable()
        {
            _player.CargoChanged += OnPlayerCargoChanged;
            _player.LevelChanged += OnPlayerLevelChanged;
        }

        private void OnDisable()
        {
            _player.CargoChanged -= OnPlayerCargoChanged;
            _player.LevelChanged -= OnPlayerLevelChanged;
        }

        private void Update()
        {
            _targetValue = _player.HP;
            _bar.value = Mathf.Lerp(_bar.value, _targetValue, _lerpSpeedMultiplier * Time.deltaTime);

            _group.alpha = Mathf.Lerp(_group.alpha, _player.IsAlive ? 1 : 0, _lerpSpeedMultiplier * Time.deltaTime);
        }

        private void FixedUpdate()
        {
            transform.position = (Vector2)Camera.main.WorldToScreenPoint(_player.transform.position + _offset);
        }

        private void OnPlayerCargoChanged()
        {
            _cargo.text = $"{_player.Cargo}/{_player.MaxCargo}";
        }

        private void OnPlayerLevelChanged()
        {
            _level.text = _player.Level.ToString();
        }
    }
}
