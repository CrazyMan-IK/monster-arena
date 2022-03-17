using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using URandom = UnityEngine.Random;
using DG.Tweening;
using TMPro;

namespace MonsterArena.UI
{
    public class Wallet : MonoBehaviour
    {
        private const string _WalletCoinsKey = "_walletCoins";

        [SerializeField] private Transform _icon = null;
        [SerializeField] private TextMeshProUGUI _text = null;
        [SerializeField] private Transform _coinPrefab = null;
        [SerializeField] private Transform _spawnedCoinsContainer = null;
        [SerializeField] private int _maxSpawnedCoins = 10;
        [SerializeField] private Vector2 _spawnSpreadMultiplier = Vector2.one;
        [SerializeField, Range(0.5f, 0.9f)] private float _minDuration = 0.5f;
        [SerializeField, Range(0.9f, 2.0f)] private float _maxDuration = 0.9f;

        private readonly Queue<Transform> _availableCoins = new Queue<Transform>();
        private int _currentValue = 0;
        private int _targetValue = 0;

        public int Value => _targetValue;

        private void Awake()
        {
            InitCoinsPool();
            _targetValue = PlayerPrefs.GetInt(_WalletCoinsKey, 0);
            //_targetValue = 3000;
            if (_targetValue > 0)
            {
                UpdateValue(_targetValue);
            }
        }

        public YieldInstruction Add(Transform from, int count)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            return AnimateCoin(from, _icon, count);
        }

        public YieldInstruction Take(Transform to, int count)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            return AnimateCoin(_icon, to, -count);
        }

        public YieldInstruction Take(int count)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (_targetValue + count < 0)
            {
                return null;
            }

            _targetValue -= count;
            PlayerPrefs.SetInt(_WalletCoinsKey, _targetValue);
            UpdateValue(_targetValue);

            return null;
        }

        public bool HaveCoins(int count)
        {
            return _currentValue >= count;
        }
        
        private YieldInstruction AnimateCoin(Transform from, Transform to, int count)
        {
            if (count == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (_targetValue + count < 0)
            {
                return null;
            }

            var coinsToSpawn = _maxSpawnedCoins / 4;
            _targetValue += count;
            PlayerPrefs.SetInt(_WalletCoinsKey, _targetValue);
            var rootAnimation = DOTween.Sequence().OnComplete(() =>
            {
                UpdateValue(_targetValue);
            });

            for (int i = 0; i < coinsToSpawn; i++)
            {
                if (_availableCoins.Count <= 0)
                {
                    break;
                }

                var coin = _availableCoins.Dequeue();

                var targetPosition = from.position;
                if (!from.TryGetComponent(out RectTransform _))
                {
                    targetPosition = Camera.main.WorldToScreenPoint(targetPosition);
                }
                coin.position = targetPosition;

                coin.gameObject.SetActive(true);

                var spreadOffset = (Vector3)(URandom.insideUnitCircle * _spawnSpreadMultiplier);

                var duration = URandom.Range(_minDuration, _maxDuration);
                var animation = DOTween.Sequence();
                
                if (count > 0)
                {
                    animation.OnComplete(() =>
                    {
                        coin.gameObject.SetActive(false);
                        _availableCoins.Enqueue(coin);

                        UpdateValue(_currentValue + count / coinsToSpawn);
                    });
                }
                else
                {
                    animation.OnStart(() =>
                    {
                        UpdateValue(_currentValue + count / coinsToSpawn);
                    }).OnComplete(() =>
                    {
                        coin.gameObject.SetActive(false);
                        _availableCoins.Enqueue(coin);
                    });
                }

                animation.Append(coin.DOMove(spreadOffset, duration / 4).SetEase(Ease.InCubic).SetRelative());

                animation.Append(coin.DOMove(to.position, duration / 4 * 3).SetEase(Ease.InOutBack));

                rootAnimation.Join(animation);
            }

            return rootAnimation.WaitForCompletion();
        }

        private void UpdateValue(int newValue)
        {
            if (newValue <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newValue));
            }

            _currentValue = newValue;
            _text.text = _currentValue.ToString();
        }

        private void InitCoinsPool()
        {
            for (int i = 0; i < _maxSpawnedCoins; i++)
            {
                var coin = Instantiate(_coinPrefab, Vector3.zero, Quaternion.identity, _spawnedCoinsContainer);
                coin.gameObject.SetActive(false);
                _availableCoins.Enqueue(coin);
            }
        }
    }
}
