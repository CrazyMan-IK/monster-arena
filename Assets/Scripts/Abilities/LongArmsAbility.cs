using System;
using System.Collections.Generic;
using UnityEngine;
using MonsterArena.Models;
using MonsterArena.Interfaces;
using MonsterArena.Extensions;
using DG.Tweening;

namespace MonsterArena.Abilities
{
    public class LongArmsAbility : MonoBehaviour, IMonsterAbility
    {
        public event Action<Transform> Killed = null;

        [SerializeField] private List<Transform> _arms = new List<Transform>();
        [SerializeField] private float _animationDuration = 0.5f;
        [SerializeField] private float _cooldown = 2;
        [SerializeField] private float _duration = 3;
        [SerializeField] private float _rangeMultiplier = 1.5f;
        [SerializeField] private Vector3 _scaleMultiplier = Vector3.one;

        private bool _isUse = false;

        public float Cooldown => _cooldown + _duration;
        public bool CanUse => true;

        public void Initialize(MonsterInformation information, LayerMask monstersLayerMask)
        {

        }

        public void Use()
        {
            _isUse = true;

            var sequence = DOTween.Sequence();
            foreach (var arm in _arms)
            {
                sequence.Join(arm.DOScale(arm.localScale.Multiply(_scaleMultiplier), _animationDuration));
            }
            //sequence.Join(Camera.main.transform.DOBlendableLocalMoveBy(Vector3.back * _rangeMultiplier * 2, _animationDuration));
            sequence.AppendInterval(_duration);
            sequence.AppendCallback(Stop);
            //sequence.Join(Camera.main.transform.DOBlendableLocalMoveBy(Vector3.forward * _rangeMultiplier * 2, _animationDuration));
            foreach (var arm in _arms)
            {
                sequence.Join(arm.DOScale(arm.localScale, _animationDuration));
            }
        }

        public float TransformSpeed(float speed)
        {
            return speed;
        }

        public float TransformReceivedDamage(float damage)
        {
            return damage;
        }

        public float TransformRange(float range)
        {
            if (_isUse)
            {
                return range * _rangeMultiplier;
            }

            return range;
        }

        private void Stop()
        {
            _isUse = false;
        }
    }
}
