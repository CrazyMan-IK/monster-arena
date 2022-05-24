using System;
using UnityEngine;

namespace MonsterArena.Models
{
    [Serializable]
    public class Modifier<T, U>
    {
        [field: SerializeField] public int Price { get; set; }
        [field: SerializeField] public T Value { get; set; }
        [field: SerializeField] public float Experience { get; set; }
        [field: SerializeField] public U Visual { get; set; }
    }
}
