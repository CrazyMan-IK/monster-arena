using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterArena.Models
{
    [CreateAssetMenu(fileName = "New MonsterInformation", menuName = "Monster Arena/Monster Information", order = 50)]
    public class MonsterInformation : ScriptableObject
    {
        [SerializeField] private MonsterCard _monsterPrefab = null;
        [SerializeField] private float _damage = 1;
        [SerializeField] private float _attackSpeed = 1;
        [SerializeField] private float _attackArea = 1;
        [SerializeField] private float _movementSpeed = 1;
        [SerializeField] private float _hp = 1;

        public MonsterCard MonsterPrefab => _monsterPrefab;
        public float Damage => _damage;
        public float AttackSpeed => _attackSpeed;
        public float AttackArea => _attackArea;
        public float MovementSpeed => _movementSpeed;
        public float HP => _hp;
    }
}
