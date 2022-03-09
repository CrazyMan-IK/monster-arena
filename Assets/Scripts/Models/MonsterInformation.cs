using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterArena.Models
{
    [CreateAssetMenu(fileName = "New MonsterInformation", menuName = "Monster Arena/Monster Information", order = 50)]
    public class MonsterInformation : ScriptableObjectWithID
    {
        [SerializeField] private Transform _monsterPrefab = null;
        [SerializeField] private float _hp = 1;
        [SerializeField] private float _damage = 1;
        //[SerializeField] private float _attackSpeed = 1;
        [SerializeField] private float _movementSpeed = 1;
        [SerializeField] private float _attackArea = 1;

        [Header("Enemy")]
        [SerializeField] private float _viewArea = 1;

        public Transform MonsterPrefab => _monsterPrefab;
        public float HP => _hp;
        public float Damage => _damage;
        //public float AttackSpeed => _attackSpeed;
        public float MovementSpeed => _movementSpeed;
        public float AttackArea => _attackArea;
    }
}
