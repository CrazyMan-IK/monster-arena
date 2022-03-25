using AYellowpaper;
using MonsterArena.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MonsterArena.Models
{
    [CreateAssetMenu(fileName = "New MonsterInformation", menuName = "Monster Arena/Monster Information", order = 50)]
    public class MonsterInformation : ScriptableObjectWithID
    {
        [field: SerializeField] public Monster MonsterPrefab { get; private set; } = null;
        [field: SerializeField] public string Name { get; private set; } = "<UNKNOWN>";
        [field: SerializeField] public float HP { get; private set; } = 1;
        [field: SerializeField] public float Damage { get; private set; } = 1;
        [field: SerializeField] public float MovementSpeed { get; private set; } = 1;
        [field: SerializeField] public float AttackArea { get; private set; } = 1;
        [field: SerializeField] public float AttackAngle { get; private set; } = 25;

        [field: Header("Player")]
        [field: SerializeField] private bool ForceUnlocked { get; set; } = false;
        [field: SerializeField] public int Price { get; private set; } = 100;

        [field: Header("Enemy")]
        [field: SerializeField] public float ViewArea { get; private set; } = 1;

        public bool IsUnlocked => ForceUnlocked || PlayerPrefs.GetInt($"{InternalID}_unlocked", 0) != 0;

        public void Unlock()
        {
            PlayerPrefs.SetInt($"{InternalID}_unlocked", 1);
        }
    }
}
