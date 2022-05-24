using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterArena
{
    [RequireComponent(typeof(LineRenderer))]
    public class LineMaterialMainTilingOffsetAnimation : MonoBehaviour
    {
        [SerializeField] private float _speed = 0.5f;

        private LineRenderer _renderer = null;

        private void Awake()
        {
            _renderer = GetComponent<LineRenderer>();
        }

        private void Update()
        {
            _renderer.material.mainTextureOffset = _speed * Time.time * Vector2.right;
        }
    }
}
