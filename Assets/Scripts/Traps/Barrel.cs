using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace MonsterArena.Traps
{
    [RequireComponent(typeof(Renderer))]
    public class Barrel : MonoBehaviour
    {
        private const float _AnimationDuration = 0.05f;
        private const string _ColorKey = "_Color";
        private const string _ColorDimKey = "_ColorDim";

        [SerializeField] private float _range = 1;
        [SerializeField] private float _damage = 1;
        [SerializeField] private float _explodeDelay = 2;
        [SerializeField] private int _ticks = 4;
        [SerializeField] private LayerMask _monsterLayerMask = default;
        [SerializeField] private ParticleSystem _explosionParticlesPrefab = null;

        private Renderer _renderer = null;

        private void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
        }

        private void OnTriggerEnter(Collider other)
        {
            var sequence = DOTween.Sequence();

            var baseColor = _renderer.material.GetColor(_ColorKey);
            var shadowColor = _renderer.material.GetColor(_ColorDimKey);
            var delay = _explodeDelay / (_ticks * 2);

            for (int i = 0; i < _ticks; i++)
            {
                AppendColorAnimation(sequence, _renderer, Color.white, Color.white);
                sequence.AppendInterval(delay);
                AppendColorAnimation(sequence, _renderer, baseColor, shadowColor);
                sequence.AppendInterval(delay);
            }

            sequence.OnComplete(OnAnimationCompleted);

            static void AppendColorAnimation(Sequence sequence, Renderer renderer, Color baseColor, Color shadowColor)
            {
                sequence.Append(renderer.material.DOColor(baseColor, _ColorKey, _AnimationDuration));
                sequence.Join(renderer.material.DOColor(shadowColor, _ColorDimKey, _AnimationDuration));
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, _range);
        }

        private void OnAnimationCompleted()
        {
            var colliders = Physics.OverlapSphere(transform.position, _range, _monsterLayerMask);

            foreach (var collider in colliders)
            {
                if (collider.TryGetComponent(out Monster monster))
                {
                    monster.TakeDamage(_damage, DamageSource.Other);
                }
            }

            Instantiate(_explosionParticlesPrefab, transform.position, transform.rotation, null);

            Destroy(gameObject);
        }
    }
}
