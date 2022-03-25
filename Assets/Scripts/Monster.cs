using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonsterArena.Models;
using MonsterArena.Extensions;
using MonsterArena.Interfaces;

namespace MonsterArena
{
    [RequireComponent(typeof(MonsterAnimation))]
    [RequireComponent(typeof(IMonsterAbility))]
    public class Monster : MonoBehaviour
    {
        private const string _Radius = "_Radius";
        private const string _Angle = "_Angle";

        public event Action WinAnimationCompleted = null;
        public event Action<Transform> Killed = null;
        public event Action Died = null;

        [SerializeField] private MonsterInformation _information = null;
        [SerializeField] private Renderer _renderer = null;
        [SerializeField] private Projector _shadow = null;
        [SerializeField] private Collider _collider = null;
        [SerializeField] private MonsterAnimationEventsRepeater _repeater = null;
        [SerializeField] private LayerMask _monstersLayerMask = default;

        private readonly Collider[] _lastColliders = new Collider[8];
        private MonsterAnimation _animation = null;
        private IMonsterAbility _ability = null;
        private float _maxHP = 0;
        private float _hp = 0;
        private bool _isPlayer = false;

        public Renderer Renderer => _renderer;
        public Collider Collider => _collider;
        public Rigidbody Rigidbody => _animation.Rigidbody;
        public MonsterInformation Information => _information;
        public float HP => _hp / _maxHP;
        public bool IsAlive => _hp > 0;
        public float AbilityCooldown => _ability.Cooldown;
        public float MovementSpeed => _ability.TransformSpeed(_information.MovementSpeed);

        private void Awake()
        {
            _animation = GetComponent<MonsterAnimation>();
            _ability = GetComponent<IMonsterAbility>();
            
            _shadow.material = new Material(_shadow.material);

            if (_information != null)
            {
                Initialize(_information);
            }
        }

        public void Initialize(MonsterInformation information)
        {
            _ability.Initialize(information, _monstersLayerMask);

            _maxHP = information.HP;
            _hp = _maxHP;

            _information = information;

            _shadow.material.SetFloat(_Radius, information.AttackArea);
            _shadow.material.SetFloat(_Angle, information.AttackAngle);

            _animation.Initialize(information);
        }

        public void InitializeAsPlayer(MonsterInformation information, int enemyCount)
        {
            Initialize(information);

            _maxHP = enemyCount + 1;
            _hp = _maxHP;

            _isPlayer = true;
        }

        private void Update()
        {
            _animation.IsAttacking = false;
            if (_information == null || !IsAlive)
            {
                return;
            }

            var count = Physics.OverlapSphereNonAlloc(transform.position, _information.AttackArea, _lastColliders, _monstersLayerMask);
            for (int i = 0; i < count; i++)
            {
                var collider = _lastColliders[i];
                    
                if (collider.TryGetComponent(out Monster monster) && monster != this && collider.transform.position.GetXZ().IsInsideCircleSector(transform.position.GetXZ(), transform.localEulerAngles.y, _information.AttackArea, _information.AttackAngle))
                {
                    _animation.IsAttacking = true;
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, _information.ViewArea);
        }

        private void OnEnable()
        {
            _repeater.Winned += OnWinAnimationCompleted;
            _repeater.Attacked += OnAttacked;
        }

        private void OnDisable()
        {
            _repeater.Winned -= OnWinAnimationCompleted;
            _repeater.Attacked -= OnAttacked;
        }

        public void UseAbility()
        {
            _ability.Use();
        }

        public void TakeDamage(float damage)
        {
            damage = _ability.TransformDamage(damage);

            if (damage < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(damage));
            }

            _hp -= damage;

            if (!IsAlive)
            {
                Die();

                return;
            }

            _animation.IsAlive = true;
        }

        public void Die()
        {
            if (!IsAlive || _ability.TransformDamage(100) <= 0)
            {
                return;
            }

            _hp = 0;
            _animation.Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            _animation.Rigidbody.isKinematic = true;
            _collider.enabled = false;

            Died?.Invoke();

            _animation.IsAlive = false;
        }

        public void RunWinningAnimation()
        {
            _animation.ActivateWinAnimation(false);
        }

        public void RunWinningAnimationOnce()
        {
            _animation.ActivateWinAnimation(true);
        }

        private void OnWinAnimationCompleted()
        {
            WinAnimationCompleted?.Invoke();
        }

        private void OnAttacked()
        {
            if (!IsAlive)
            {
                return;
            }

            var count = Physics.OverlapSphereNonAlloc(transform.position, _information.AttackArea, _lastColliders, _monstersLayerMask);
            for (int i = 0; i < count; i++)
            {
                var collider = _lastColliders[i];

                if (collider.TryGetComponent(out Monster monster) && monster != this && monster.IsAlive && collider.transform.position.GetXZ().IsInsideCircleSector(transform.position.GetXZ(), transform.localEulerAngles.y, _information.AttackArea, _information.AttackAngle))
                {
                    if (_isPlayer)
                    {
                        monster.Die();
                    }
                    else
                    {
                        monster.TakeDamage(_information.Damage);
                    }

                    if (monster.IsAlive)
                    {
                        continue;
                    }

                    Killed?.Invoke(monster.transform);
                }
            }
        }
    }
}
