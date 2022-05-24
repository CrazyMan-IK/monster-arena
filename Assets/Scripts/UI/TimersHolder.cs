using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace MonsterArena.UI
{
    public class TimersHolder : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _timerPrefab = null;
        [SerializeField] private List<MonsterSpawner> _spawners = null;

        private void Awake()
        {
            foreach (var spawner in _spawners)
            {
                var timer = Instantiate(_timerPrefab, transform);
                spawner.Initialize(timer);
            }
        }
    }
}
