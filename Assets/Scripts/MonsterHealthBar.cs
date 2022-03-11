using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MonsterArena
{
    [RequireComponent(typeof(Slider))]
    public class MonsterHealthBar : MonoBehaviour
    {
        [SerializeField] private Monster _monster = null;
        [SerializeField] private Vector3 _offset = Vector3.zero;
        [SerializeField] private float _lerpSpeedMultiplier = 5;

        private Slider _bar = null;
        //private Monster _monster = null;
        private float _targetValue = 0;

        private void Awake()
        {
            _bar = GetComponent<Slider>();
        }

        public void Initialize(Monster monster)
        {
            _monster = monster;
        }

        private void Update()
        {
            if (_monster == null)
            {
                return;
            }

            _targetValue = _monster.HP;
            _bar.value = Mathf.Lerp(_bar.value, _targetValue, _lerpSpeedMultiplier * Time.deltaTime);
        }

        private void LateUpdate()
        {
            if (_monster == null)
            {
                return;
            }

            transform.position = (Vector2)Camera.main.WorldToScreenPoint(_monster.transform.position + _offset + _monster.Collider.bounds.size.y * Vector3.up);
        }
    }
}
