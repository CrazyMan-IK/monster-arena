using System.Collections;
using System.Collections.Generic;
using MonsterArena.Extensions;
using UnityEngine;

namespace MonsterArena
{
    [RequireComponent(typeof(HelicopterMovement))]
    public class HelicopterAnimation : MonoBehaviour
    {
        [SerializeField] private Transform _topRotor = null;
        [SerializeField] private Transform _backRotor = null;
        [SerializeField] private float _speed = 100;

        private HelicopterMovement _movement = null;

        private void Awake()
        {
            _movement = GetComponent<HelicopterMovement>();
        }

        private void Update()
        {
            _topRotor.Rotate(_speed * MathExtensions.CubicOut(_movement.AirMultiplier) * Time.deltaTime * Vector3.down);
            _backRotor.Rotate(_speed * MathExtensions.CubicOut(_movement.AirMultiplier) * Time.deltaTime * Vector3.left);
        }
    }
}
