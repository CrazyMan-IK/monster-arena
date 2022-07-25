using System.Collections;
using Source.EnemyView;
using UnityEngine;

namespace MonsterArena
{
    [RequireComponent(typeof(Monster))]
    public class MonsterAttackThrowing : MonoBehaviour, IMonsterAttack
    {
        [SerializeField] private FieldOfView _fieldOfView;
        [SerializeField] private MonsterAnimation _animation;
        [SerializeField] private Transform _throwPoint;
        [SerializeField] private Rigidbody _projectilePrefab;
        [SerializeField] private float _force;

        private Monster _monster;
        private Rigidbody _projectile;

        private void Awake()
        {
            _monster = GetComponent<Monster>();
            SpawnProjectile();
        }

        public void StartAttack()
        {
            SetAttacking(true);
            StartCoroutine(Attacking());
        }

        private IEnumerator Attacking()
        {
            yield return new WaitUntil(() => _projectile == null);
            SpawnProjectile();

            yield return new WaitForSeconds(1);
        }

        private void SpawnProjectile()
        {
            _projectile = Instantiate(_projectilePrefab, _throwPoint);
            _projectile.useGravity = false;
            _projectile.isKinematic = true;
        }

        public void Reset()
        {
            SetAttacking(false);
            Invoke(nameof(ReleaseProjectile), 2f);
        }

        public void Hit(float damage)
        {
            ReleaseProjectile();
            _projectile.AddForce(transform.forward * _force);
            
            StartCoroutine(DelayHit(damage));
        }

        private void ReleaseProjectile()
        {
            _projectile.transform.SetParent(null);
            _projectile.useGravity = true;
            _projectile.isKinematic = false;
        }

        private IEnumerator DelayHit(float damage)
        {
            yield return new WaitForSeconds(0.3f);
            
            if (_fieldOfView.TryFindVisibleTarget(out Helicopter helicopter))
                helicopter.TakeDamage(damage);

            SetAttacking(false);
            _projectile = null;
            SpawnProjectile();
        }

        private void SetAttacking(bool active)
        {
            _animation.IsThrowing = active;
            _fieldOfView.gameObject.SetActive(active);
        }
    }
}