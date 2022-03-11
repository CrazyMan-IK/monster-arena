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

        public static Vector3 AsXZ(this Vector2 position)
        {
            return new Vector3(position.x, 0, position.y);
        }
    }
}
