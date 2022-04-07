using System;
using UnityEngine;
using MonsterArena.Models;
using MonsterArena.Interfaces;
using MonsterArena.Extensions;
using DG.Tweening;
using System.Linq;

namespace MonsterArena.Abilities
{
    [RequireComponent(typeof(MonsterAnimation))]
    [RequireComponent(typeof(Collider))]
    public class JumpAbility : MonoBehaviour, IMonsterAbility
    {
        public event Action<Transform> Killed = null;

        [SerializeField] private AnimationCurve _curve = null;
        [SerializeField] private ParticleSystem _startParticles = null;
        [SerializeField] private ParticleSystem _endParticles = null;
        [SerializeField] private Shake _shake = null;
        [SerializeField] private float _cooldown = 2;
        [SerializeField] private float _animationDelay = 0.6f;
        [SerializeField] private float _animationDuration = 3.2f;
        [SerializeField] private float _animationSpeedMultiplier = 1.0f;

        private MonsterAnimation _animation = null;
        private Collider _collider = null;
        private MonsterInformation _information = null;
        private bool _isUse = false;
        private LayerMask _monstersLayerMask = default;

        public float Cooldown => _cooldown;
        public bool CanUse
        {
            get
            {
                var colliders = Physics.OverlapSphere(transform.position, _information.ViewArea * 2.0f, _monstersLayerMask);

                return colliders.Any(x => x != _collider && x.TryGetComponent(out Monster monster) && monster.IsAlive);
            }
        }

        private void Awake()
        {
            _animation = GetComponent<MonsterAnimation>();
            _collider = GetComponent<Collider>();
        }

        public void Initialize(MonsterInformation information, LayerMask monstersLayerMask)
        {
            _information = information;
            _monstersLayerMask = monstersLayerMask;
        }

        public void Use()
        {
            _isUse = true;

            var colliders = Physics.OverlapSphere(transform.position, _information.ViewArea * 2.0f, _monstersLayerMask);

            foreach (var collider in colliders)
            {
                if (collider != _collider && collider.TryGetComponent(out Monster monster) && monster.IsAlive)
                {
                    _animation.StartAbility(_animationSpeedMultiplier);

                    _startParticles.Play();

                    var lookAt = Quaternion.LookRotation(collider.transform.position - transform.position, transform.up);

                    var sequence = DOTween.Sequence().OnComplete(Stop);
                    sequence.SetDelay(_animationDelay);

                    sequence.Append(transform.DORotateQuaternion(lookAt, _animationDuration / _animationSpeedMultiplier).SetEase(_curve));
                    sequence.Join(transform.DOJump(collider.transform.position, Vector3.Distance(transform.position, collider.transform.position) / 2.0f, 1, _animationDuration / _animationSpeedMultiplier).SetEase(_curve));

                    return;
                }
            }
        }

        public float TransformSpeed(float speed)
        {
            return speed;
        }

        public float TransformReceivedDamage(float damage)
        {
            if (_isUse)
            {
                return 0;
            }

            return damage;
        }

        public float TransformRange(float range)
        {
            return range;
        }

        private void Stop()
        {
            _isUse = false;

            var colliders = Physics.OverlapSphere(transform.position, _information.AttackArea * 0.5f, _monstersLayerMask);

            foreach (var collider in colliders)
            {
                if (collider != _collider && collider.TryGetComponent(out Monster monster) && monster.IsAlive)
                {
                    monster.Die();

                    Killed?.Invoke(collider.transform);

                    break;
                }
            }

            _animation.StopAbility();

            _endParticles.Play();

            CameraShake.AddShake(_shake.Duration, _shake.Strength);
        }
    }
}
