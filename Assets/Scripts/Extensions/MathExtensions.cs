using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterArena.Extensions
{
    public static class MathExtensions
    {
        public static float Remap(this float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            return toMin + (value - fromMin) * (toMax - toMin) / (fromMax - fromMin);
        }

        public static float ExpoIn(float t)
        {
            return t == 0 ? 0 : Mathf.Pow(2, 10 * (t - 1));
        }

        public static float ExpoOut(float t)
        {
            return t == 1 ? 1 : 1 - Mathf.Pow(2, -10 * t);
        }

        public static float CubicIn(float t)
        {
            return t == 0 ? 0 : Mathf.Pow(t, 3);
        }

        public static float CubicOut(float t)
        {
            return t == 1 ? 1 : 1 - Mathf.Pow(1 - t, 3);
        }

        public static Vector2 CubicBezier(float t, float x1, float y1, float x2, float y2)
        {
            t = Mathf.Clamp01(t);

            Vector2 p1 = new Vector2(x1, y1);
            Vector2 p2 = new Vector2(x2, y2);

            return 3 * t * Mathf.Pow(1 - t, 2) * p1 +
                   3 * Mathf.Pow(t, 2) * (1 - t) * p2 +
                   Mathf.Pow(t, 3) * Vector2.one;
        }
    }
}
