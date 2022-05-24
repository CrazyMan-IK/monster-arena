using System;
using UnityEngine;
using MonsterArena.Models;
using MonsterArena.Interfaces;

namespace MonsterArena.Abilities
{
    [RequireComponent(typeof(MonsterAnimation))]
    [RequireComponent(typeof(Collider))]
    public class TornadoAbility : MonoBehaviour, IMonsterAbility
    {
        public event Action<Transform> Killed = null;

        [SerializeField] private GameObject _particles = null;
        [SerializeField] private float _cooldown = 2;
        [SerializeField] private float _duration = 3;
        [SerializeField] private float _speedMultiplier = 1.5f;

        private MonsterAnimation _animation = null;
        private Collider _collider = null;
        private MonsterInformation _information = null;
        private bool _isUse = false;
        private readonly Collider[] _lastColliders = new Collider[8];
        private LayerMask _monstersLayerMask = default;

        public float Cooldown => _cooldown + _duration;
        public bool CanUse => true;

        private void Awake()
        {
            _animation = GetComponent<MonsterAnimation>();
            _collider = GetComponent<Collider>();
        }

        private void Update()
        {
            if (_isUse)
            {
                var count = Physics.OverlapSphereNonAlloc(transform.position, _information.AttackArea, _lastColliders, _monstersLayerMask);
                for (int i = 0; i < count; i++)
                {
                    var collider = _lastColliders[i];

                    if (collider != _collider && collider.TryGetComponent(out Monster monster) && monster.IsAlive)
                    {
                        monster.Die(DamageSource.Player);

                        Killed?.Invoke(collider.transform);
                    }
                }
            }
        }

        public void Initialize(MonsterInformation information, LayerMask monstersLayerMask)
        {
            _information = information;
            _monstersLayerMask = monstersLayerMask;
        }

        public void Use()
        {
            _isUse = true;
            _particles.SetActive(true);
            _animation.StartAbility();

            Invoke(nameof(Stop), _duration);
        }

        public float TransformSpeed(float speed)
        {
            if (_isUse)
            {
                return speed * _speedMultiplier;
            }

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
            _particles.SetActive(false);
            _animation.StopAbility();
        }
    }
}
