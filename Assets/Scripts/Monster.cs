using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using URandom = UnityEngine.Random;
using MonsterArena.Models;
using MonsterArena.Extensions;

namespace MonsterArena
{
    [RequireComponent(typeof(MonsterAnimation))]
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
        private float _maxHP = 0;
        private float _hp = 0;

        public Renderer Renderer => _renderer;
        public Collider Collider => _collider;
        public Rigidbody Rigidbody => _animation.Rigidbody;
        public MonsterInformation Information => _information;
        public float HP => _hp / _maxHP;
        public bool IsAlive => _hp > 0;

        private void Awake()
        {
            _animation = GetComponent<MonsterAnimation>();

            //var oldMaterial = _shadow.material;
            //_shadow.material = new Material(_shadow.material.shader);
            //_shadow.material.CopyPropertiesFromMaterial(oldMaterial);

            _shadow.material = new Material(_shadow.material);

            if (_information != null)
            {
                Initialize(_information, false);
            }
        }

        public void Initialize(MonsterInformation information, bool isPlayer)
        {
            _maxHP = information.HP / (isPlayer ? 1 : 1.75f);
            _hp = _maxHP;

            _information = information;

            _shadow.material.SetFloat(_Radius, information.AttackArea);
            _shadow.material.SetFloat(_Angle, information.AttackAngle);

            _animation.Initialize(information);
        }

        private void Update()
        {
            _animation.IsAttacking = false;
            if (_information == null || !IsAlive)
            {
                return;
            }

            /*URandom.InitState(Convert.ToInt32(transform.position.magnitude));
            for (int i = 0; i < 50000; i++)
            {
                var point = _information.AttackArea * URandom.insideUnitCircle + transform.position.GetXZ();

                var isInside = point.IsInsideCircleSector(transform.position.GetXZ(), transform.localEulerAngles.y, _information.AttackArea, _information.AttackAngle);

                Debug.DrawRay(point.AsXZ(), Vector3.up, isInside ? Color.blue : Color.red);
            }*/

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

        public void TakeDamage(float damage)
        {
            if (damage <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(damage));
            }

            _hp -= damage;

            if (_hp <= 0)
            {
                _hp = 0;
                _animation.Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                _animation.Rigidbody.isKinematic = true;
                _collider.enabled = false;

                Died?.Invoke();
            }

            _animation.IsAlive = IsAlive;
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
                    monster.TakeDamage(_information.Damage);

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
