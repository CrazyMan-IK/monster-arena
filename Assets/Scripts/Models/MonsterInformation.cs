using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MonsterArena.Models
{
    [CreateAssetMenu(fileName = "New MonsterInformation", menuName = "Monster Arena/Monster Information", order = 50)]
    public class MonsterInformation : ScriptableObjectWithID
    {
        [SerializeField] private Monster _monsterPrefab = null;
        [SerializeField] private string _name = "<UNKNOWN>";
        [SerializeField] private float _hp = 1;
        [SerializeField] private float _damage = 1;
        //[SerializeField] private float _attackSpeed = 1;
        [SerializeField] private float _movementSpeed = 1;
        [SerializeField] private float _attackArea = 1;

        [Header("Player")]
        [SerializeField] private bool _forceUnlocked = false;
        [SerializeField] private int _price = 100;

        [Header("Enemy")]
        [SerializeField] private float _viewArea = 1;

        public Monster MonsterPrefab => _monsterPrefab;
        public string Name => _name;
        public float HP => _hp;
        public float Damage => _damage;
        //public float AttackSpeed => _attackSpeed;
        public float MovementSpeed => _movementSpeed;
        public float AttackArea => _attackArea;
        public bool IsUnlocked => _forceUnlocked || PlayerPrefs.GetInt($"{InternalID}_unlocked", 0) != 0;
        public int Price => _price;

        public void Unlock()
        {
            PlayerPrefs.SetInt($"{InternalID}_unlocked", 1);
        }
    }
}
