using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace MonsterArena
{
    [RequireComponent(typeof(BoxCollider))]
    public class Spikes : MonoBehaviour
    {
        [SerializeField] private Transform _root = null;
        [SerializeField] private LayerMask _monsterLayerMask = default;
        [SerializeField] private float _damage = 1;
        [SerializeField] private float _disabledVerticalPosition = 5;
        [SerializeField] private float _animationDuration = 0.4f;

        private BoxCollider _collider = null;
        private Sequence _lastSequence = null;

        private void Awake()
        {
            _collider = GetComponent<BoxCollider>();
        }

        private void OnTriggerStay(Collider other)
        {
            if (_lastSequence == null || !_lastSequence.IsActive() || _lastSequence.IsComplete())
            {
                _lastSequence = DOTween.Sequence();

                _lastSequence.Append(_root.DOLocalMoveY(0, _animationDuration).SetEase(Ease.InExpo).OnComplete(() =>
                {
                    HitStayedMonsters();
                }));
                _lastSequence.AppendInterval(_animationDuration);
                _lastSequence.Append(_root.DOLocalMoveY(_disabledVerticalPosition, _animationDuration).SetEase(Ease.Linear));
                _lastSequence.AppendInterval(_animationDuration);
            }
        }

        private void HitStayedMonsters()
        {
            var position = transform.position + Vector3.Scale(transform.lossyScale, _collider.center);
            var scale = Vector3.Scale(transform.lossyScale, _collider.size);

            var colliders = Physics.OverlapBox(position, scale / 2, transform.rotation, _monsterLayerMask);

            foreach (var collider in colliders)
            {
                if (collider.TryGetComponent(out Monster monster))
                {
                    monster.TakeDamage(_damage, DamageSource.Other);
                }
            }
        }
    }
}
