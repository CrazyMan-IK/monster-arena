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

		public static Vector3 Multiply(this Vector3 a, Vector3 b)
        {
			return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

		public static Vector3 Divide(this Vector3 a, Vector3 b)
        {
			return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }

        public static Vector2 GetXZ(this Vector3 position)
        {
            return new Vector2(position.x, position.z);
		}

		public static Vector3 AsXZ(this Vector3 position)
		{
			return new Vector3(position.x, 0, position.y);
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

			Vector2 point2 = point - radius / 3.5f * Vector2.up.Rotate(Mathf.Deg2Rad * rotation);

			float mag = point.magnitude;

			var rot1 = Vector2.down.Rotate(Mathf.Deg2Rad * (angle / -2 + rotation));
			var rot2 = Vector2.down.Rotate(Mathf.Deg2Rad * (angle / 2 + rotation));

			/*Debug.DrawRay(position, rot1, Color.red, 0.2f);
			Debug.DrawRay(position, rot2, Color.blue, 0.2f);
			Debug.DrawRay(position, Vector3.forward, Color.black, 0.2f);
			Debug.DrawRay(point, Vector3.forward, Color.yellow, 0.2f);*/

			float ld = Cross(rot1, point2);
			float rd = Cross(rot2, point2);

			bool isInsideCircle = mag <= radius;

			bool c2 = ld <= 0;
			bool c3 = rd >= 0;

			return isInsideCircle && c2 && c3 && point2.Rotate(Mathf.Deg2Rad * rotation).y < radius / -3.5f;
		}
    }
}
