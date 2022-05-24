using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterArena
{
    public class Wobble : MonoBehaviour
    {
        [SerializeField] private Vector3 _offset = Vector3.up;
        [SerializeField] private float _speed = 1;

        private Vector3 _basePosition = Vector3.zero;
        private float _timeOffset = 0;

        private void Awake()
        {
            _basePosition = transform.localPosition;
            _timeOffset = (Random.value - 0.5f) * Mathf.PI;
        }
        
        private void Update()
        {
            transform.localPosition = _basePosition + _offset * (0.5f + Mathf.Sin(Time.time * _speed + _timeOffset) * 0.5f);
        }
    }
}
