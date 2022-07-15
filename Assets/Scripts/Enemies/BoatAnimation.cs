using UnityEngine;
using DG.Tweening;

namespace MonsterArena
{
    [RequireComponent(typeof(Rigidbody))]
    public class BoatAnimation : MonoBehaviour
    {
        [SerializeField] private Monster _monster;
        [SerializeField] private Animator _monsterAnimator;

        private Rigidbody _rigidbody;

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            _monster.Died += OnDied;
            _monster.Revived += OnRevived;
        }
        
        private void OnDisable()
        {
            _monster.Died -= OnDied;
            _monster.Revived -= OnRevived;
        }

        private void OnRevived(Monster monster)
        {
            _monsterAnimator.SetBool(Constants.Alive, true);
        }

        private void OnDied(Monster monster, DamageSource source)
        {
            _rigidbody.isKinematic = true;
            _rigidbody.useGravity = false;
            transform.DOLocalMove(Vector3.down * 1f, 3.0f).SetRelative();
            _monsterAnimator.SetBool(Constants.Alive, false);
        }
    }
}
