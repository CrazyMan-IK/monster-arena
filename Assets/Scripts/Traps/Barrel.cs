using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterArena.Traps
{
    public class Barrel : MonoBehaviour
    {
        [SerializeField] private float _range = 1;
        [SerializeField] private float _damage = 1;
        [SerializeField] private LayerMask _monsterLayerMask = default;
        [SerializeField] private ParticleSystem _explosionParticlesPrefab = null;

        private void OnTriggerEnter(Collider other)
        {
            var colliders = Physics.OverlapSphere(transform.position, _range, _monsterLayerMask);

            foreach (var collider in colliders)
            {
                if (collider.TryGetComponent(out Monster monster))
                {
                    monster.TakeDamage(_damage);
                }
            }

            Instantiate(_explosionParticlesPrefab, transform.position, transform.rotation, null);

            Destroy(gameObject);
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, _range);
        }
    }
}
