using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MonsterArena.Models;

namespace MonsterArena.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class Carousel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public event Action<MonsterMenuView> SelectionChanged = null;

        [SerializeField] private Pivot _pivotPrefab = null;
        [SerializeField] private Transform _content = null;
        [SerializeField] private Button _previousButton = null;
        [SerializeField] private Button _nextButton = null;
        [SerializeField] private float _speedMultiplier = 5;
        [SerializeField] private float _sensitivityMultiplier = 2;
        [SerializeField] private float _angle = 25;
        [SerializeField] private float _carouselRadius = 10;

        private readonly List<MonsterMenuView> _monsters = new List<MonsterMenuView>();
        private readonly List<MonsterInformation> _availableMonsters = new List<MonsterInformation>();
        private RectTransform _rectTransform = null;
        private Vector2 _startMousePosition = Vector2.zero;
        private int _currentIndex = 0;
        private float _currentAngle = 0;
        private float _lastPercentage = 0;

        public IReadOnlyList<MonsterMenuView> Monsters => _monsters;
        public MonsterMenuView CurrentMonster => _monsters[_currentIndex];

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            UpdateCurrentAngle();
        }

        private void OnEnable()
        {
            _previousButton.onClick.AddListener(Previous);
            _nextButton.onClick.AddListener(Next);
        }

        private void OnDisable()
        {
            _previousButton.onClick.RemoveListener(Previous);
            _nextButton.onClick.RemoveListener(Next);
        }

        private void Update()
        {
            _content.localRotation = Quaternion.Lerp(_content.localRotation, Quaternion.AngleAxis(_currentAngle, Vector3.up), _speedMultiplier * Time.deltaTime);
        }

        public void Initialize(List<MonsterInformation> availableMonsters)
        {
            _availableMonsters.Clear();
            _monsters.Clear();

            _availableMonsters.AddRange(availableMonsters);

            var newPosition = _content.transform.localPosition;
            newPosition.z = _carouselRadius - 5;
            _content.transform.localPosition = newPosition;

            foreach (Transform child in _content)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < availableMonsters.Count; i++)
            {
                var monsterInformation = availableMonsters[i];

                var pivot = Instantiate(_pivotPrefab, _content);
                var attachTransform = pivot.GetAttachTransform();
                var monster = Instantiate(monsterInformation.MonsterPrefab, attachTransform.position, attachTransform.rotation, attachTransform);
                monster.Initialize(monsterInformation, true);
                _monsters.Add(monster.gameObject.AddComponent<MonsterMenuView>());

                pivot.transform.localRotation = Quaternion.AngleAxis(_angle * i, Vector3.down);
                pivot.SetOffset(Vector3.back * _carouselRadius);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _startMousePosition = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            var delta = _startMousePosition.x - eventData.position.x - eventData.delta.x * _sensitivityMultiplier;
            _lastPercentage = delta / _rectTransform.rect.width * _sensitivityMultiplier;

            _currentAngle = _angle * (_currentIndex + _lastPercentage);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (Mathf.Abs(_lastPercentage) >= 0.5f)
            {
                _currentIndex = Convert.ToInt32(_currentAngle / _angle);
            }

            UpdateCurrentAngle();
            _lastPercentage = 0;
        }

        private void Previous()
        {
            _currentIndex -= 1;
            UpdateCurrentAngle();
        }

        private void Next()
        {
            _currentIndex += 1;
            UpdateCurrentAngle();
        }

        private void UpdateCurrentAngle()
        {
            _currentIndex = Mathf.Clamp(_currentIndex, 0, _availableMonsters.Count - 1);

            _currentAngle = _angle * _currentIndex;

            _previousButton.gameObject.SetActive(_currentIndex != 0);
            _nextButton.gameObject.SetActive(_currentIndex != _availableMonsters.Count - 1);

            SelectionChanged?.Invoke(_monsters[_currentIndex]);
        }
    }
}
