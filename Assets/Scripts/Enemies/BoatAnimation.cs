using UnityEngine;
using DG.Tweening;

namespace MonsterArena
{
    [RequireComponent(typeof(Rigidbody))]
    public class BoatAnimation : MonoBehaviour
    {
        [SerializeField] private Monster _monster;

        private Rigidbody _rigidbody;

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            _monster.Died += OnDied;
        }
        
        private void OnDisable()
        {
            _monster.Died -= OnDied;
        }

        private void OnDied(Monster monster, DamageSource source)
        {
            _rigidbody.isKinematic = true;
            _rigidbody.useGravity = false;
            transform.DOLocalMove(Vector3.down * 1f, 3.0f).SetRelative();
        }
    }
}
