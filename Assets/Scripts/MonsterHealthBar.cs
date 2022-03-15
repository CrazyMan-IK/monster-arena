using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MonsterArena
{
    [RequireComponent(typeof(Slider))]
    [RequireComponent(typeof(CanvasGroup))]
    public class MonsterHealthBar : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _name = null;
        [SerializeField] private Vector3 _offset = Vector3.zero;
        [SerializeField] private float _lerpSpeedMultiplier = 5;

        private Slider _bar = null;
        private CanvasGroup _group = null;
        private Monster _monster = null;
        private float _targetValue = 0;

        private void Awake()
        {
            _bar = GetComponent<Slider>();
            _group = GetComponent<CanvasGroup>();
        }

        public void Initialize(Monster monster, string name)
        {
            _monster = monster;

            if (string.IsNullOrEmpty(name))
            {
                _name.gameObject.SetActive(false);
                return;
            }

            _name.text = name;
        }

        private void Update()
        {
            if (_monster == null)
            {
                return;
            }

            _targetValue = _monster.HP;
            _bar.value = Mathf.Lerp(_bar.value, _targetValue, _lerpSpeedMultiplier * Time.deltaTime);

            if (_monster.HP <= 0)
            {
                _group.alpha = Mathf.Lerp(_group.alpha, 0, _lerpSpeedMultiplier * Time.deltaTime);

                if (_group.alpha <= 0)
                {
                    Destroy(gameObject);
                }
            }
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
