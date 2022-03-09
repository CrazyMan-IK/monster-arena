using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterArena.Extensions
{
    public static class VectorExtensions
    {
        public static Vector2 GetXZ(this Vector3 position)
        {
            return new Vector2(position.x, position.z);
        }
    }
}
