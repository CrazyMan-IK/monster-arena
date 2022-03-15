using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterArena.Extensions
{
    public static class VectorExtensions
    {
		public static float Cross(Vector2 a, Vector2 b)
        {
			return a.x * b.y - a.y * b.x;
        }

        public static Vector2 GetXZ(this Vector3 position)
        {
            return new Vector2(position.x, position.z);
        }

        public static Vector3 AsXZ(this Vector2 position)
        {
            return new Vector3(position.x, 0, position.y);
        }

		public static Vector2 Rotate(this Vector2 position, float angle)
        {
			var result = Vector2.zero;
			result.x = Mathf.Cos(angle) * position.x + Mathf.Sin(angle) * position.y;
			result.y = Mathf.Sin(angle) * position.x + Mathf.Cos(angle) * position.y;

			return result;
        }

        public static bool IsInsideCircleSector(this Vector2 point, Vector2 position, float rotation, float radius, float angle)
		{
			point = position - point;

			float mag = point.magnitude;

			var rot1 = Vector2.down.Rotate(Mathf.Deg2Rad * (angle / -2 + rotation));
			var rot2 = Vector2.down.Rotate(Mathf.Deg2Rad * (angle / 2 + rotation));

			/*Debug.DrawRay(position, rot1, Color.red, 0.2f);
			Debug.DrawRay(position, rot2, Color.blue, 0.2f);
			Debug.DrawRay(position, Vector3.forward, Color.black, 0.2f);
			Debug.DrawRay(point, Vector3.forward, Color.yellow, 0.2f);*/

			float ld = Cross(rot1, point);
			float rd = Cross(rot2, point);

			bool isInsideCircle = mag <= radius;

			bool c2 = ld <= 0;
			bool c3 = rd >= 0;

			return isInsideCircle && c2 && c3;
		}
    }
}
