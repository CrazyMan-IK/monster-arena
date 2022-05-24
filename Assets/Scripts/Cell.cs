using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterArena
{
    [RequireComponent(typeof(Renderer))]
    [RequireComponent(typeof(Collider))]
    public class Cell : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _consumeEffect = null;

        private Renderer _renderer = null;
        private Collider _collider = null;
        
        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _collider = GetComponent<Collider>();
        }

        public void Consume()
        {
            _renderer.enabled = false;
            _collider.enabled = false;

            _consumeEffect.Play();
        }
    }
}
