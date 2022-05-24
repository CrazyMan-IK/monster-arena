using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MonsterArena.Extensions;

namespace MonsterArena
{
    public class MonsterSpawner : MonoBehaviour
    {
        [SerializeField] private float _duration = 60;
        [SerializeField] private Monster _monster = null;

        private TextMeshProUGUI _timeText = null;
        private Timer _timer = null;

        public void Initialize(TextMeshProUGUI timeText)
        {
            _timeText = timeText;
        }

        private void Start()
        {
            _timeText.gameObject.SetActive(false);

            _timer.Stop();
        }

        private void OnEnable()
        {
            _timer ??= new Timer(_duration);
            _timer.Ticked += SpawnMonster;

            _monster.Died += OnMonsterDied;
        }

        private void OnDisable()
        {
            _timer.Ticked -= SpawnMonster;

            _monster.Died -= OnMonsterDied;
        }

        private void Update()
        {
            if (_timeText == null)
            {
                return;
            }
            
            _timeText.text = _timer.TimeLeft.ToCustomString();

            _timer.Update(Time.deltaTime);
        }

        private void LateUpdate()
        {
            if (_timeText == null)
            {
                return;
            }
            
            _timeText.transform.position = Camera.main.WorldToScreenPoint(transform.position);
        }

        private void SpawnMonster()
        {
            _monster.Revive();
            _monster.transform.localPosition = Vector3.zero;

            _timeText.gameObject.SetActive(false);

            _timer.Stop();
        }
        
        private void OnMonsterDied(Monster self, DamageSource source)
        {
            _timeText.gameObject.SetActive(true);

            _timer.Start();
        }
    }
}
