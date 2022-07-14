using Source.EnemyView;
using UnityEngine;

namespace MonsterArena
{
    [RequireComponent(typeof(Monster))]
    public class MonsterAttack : MonoBehaviour, IMonsterAttack
    {
        [SerializeField] private FieldOfView _fieldOfView;
        [SerializeField] private MonsterAnimation _animation;

        private Monster _monster;

        private void Awake()
        {
            _monster = GetComponent<Monster>();
        }

        public void StartAttack()
        {
            SetAttacking(true);
        }

        public void Reset()
        {
            SetAttacking(false);
        }

        public void Hit(float damage)
        {
            if(_fieldOfView.TryFindVisibleTarget(out Helicopter helicopter))
                helicopter.TakeDamage(damage);
            
            SetAttacking(false);
        }

        private void SetAttacking(bool active)
        {
            _animation.IsThrowing = active;
            _fieldOfView.gameObject.SetActive(active);
        }
    }
}