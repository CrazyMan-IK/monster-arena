using System.Collections;
using DG.Tweening;
using Source.EnemyView;
using UnityEngine;

namespace MonsterArena
{
    [RequireComponent(typeof(Monster))]
    public class BoatAttack : MonoBehaviour, IMonsterAttack
    {
        private const float AimingDelay = 2f;
        
        [SerializeField] private MonsterAnimation _animation;
        [SerializeField] private Transform _attackCenter;
        [SerializeField] private Transform _gun;
        [SerializeField] private float _hitRadius;
        [SerializeField] private LayerMask _helicopterLayerMask;
        [SerializeField] private Transform _missilePrefab;
        [SerializeField] private float _missileSpeed;
        [SerializeField] private ParticleSystem _explosion;

        private Monster _monster;

        private void Awake()
        {
            _monster = GetComponent<Monster>();
        }

        public void StartAttack()
        {
            SetAttacking(true);
            Vector3 helicopterPosition = _monster.Helicopter.transform.position;
            _attackCenter.position = new Vector3(helicopterPosition.x, _attackCenter.position.y, helicopterPosition.z);

            StartCoroutine(Attacking(helicopterPosition));
        }

        private IEnumerator Attacking(Vector3 helicopterPosition)
        {
            _gun.DORotate(Quaternion.LookRotation(helicopterPosition - _gun.position).eulerAngles, AimingDelay);
            yield return new WaitForSeconds(AimingDelay);
            Transform missile = Instantiate(_missilePrefab, _gun.position, _gun.rotation);

            while (missile.position.y < helicopterPosition.y - 1)
            {
                missile.Translate(Vector3.forward * _missileSpeed * Time.deltaTime);
                yield return null;
            }
            
            Destroy(missile.gameObject);
            _explosion.Play();
            _monster.OnAttacked();
        }

        public void Reset()
        {
            SetAttacking(false);
        }

        public void Hit(float damage)
        {
            if(HelicopterInAttackRange(out Helicopter helicopter))
                helicopter.TakeDamage(damage);
                
            SetAttacking(false);
        }

        private bool HelicopterInAttackRange(out Helicopter helicopter)
        {
            RaycastHit[] _lastHits = new RaycastHit[16];
            helicopter = null;
            
            var count = Physics.SphereCastNonAlloc(_attackCenter.position, _hitRadius, Vector3.up, _lastHits, 10,
                _helicopterLayerMask);

            for (int i = 0; i < count; i++)
            {
                if (_lastHits[i].collider.TryGetComponent(out Helicopter target) && target.IsAlive)
                {
                    helicopter = target;

                    return true;
                }
            }

            return false; 
        }

        private void SetAttacking(bool active)
        {
            _animation.IsThrowing = active;
            _attackCenter.gameObject.SetActive(active);
        }
    }
}