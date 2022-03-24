using UnityEngine;

namespace PixelPlay.OffScreenIndicator
{
    public class OffScreenIndicatorCore
    {
        /// <summary>
        /// Gets the position of the target mapped to screen cordinates.
        /// </summary>
        /// <param name="mainCamera">Refrence to the main camera</param>
        /// <param name="targetPosition">Target position</param>
        /// <returns></returns>
        public static Vector3 GetScreenPosition(Camera mainCamera, Vector3 targetPosition)
        {
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(targetPosition);
            return screenPosition;
        }

        /// <summary>
        /// Gets if the target is within the view frustrum.
        /// </summary>
        /// <param name="screenPosition">Position of the target mapped to screen cordinates</param>
        /// <returns></returns>
        public static bool IsTargetVisible(Vector3 screenPosition)
        {
            bool isTargetVisible = screenPosition.z > 0 && screenPosition.x > 0 && screenPosition.x < Screen.width && screenPosition.y > 0 && screenPosition.y < Screen.height;
            return isTargetVisible;
        }

        /// <summary>
        /// Gets the screen position and angle for the arrow indicator. 
        /// </summary>
        /// <param name="screenPosition">Position of the target mapped to screen cordinates</param>
        /// <param name="screenCentre">The screen centre</param>
        /// <param name="screenBounds">The screen bounds</param>
        /// <returns>Angle of indicator</returns>
        /*public static float GetArrowIndicatorPositionAndAngle(ref Vector3 screenPosition, Vector2 screenCentre, Vector2 screenBounds)
        {
            // Our screenPosition's origin is screen's bottom-left corner.
            // But we have to get the arrow's screenPosition and rotation with respect to screenCentre.
            screenPosition -= (Vector3)screenCentre;

            // When the targets are behind the camera their projections on the screen (WorldToScreenPoint) are inverted,
            // so just invert them.
            if(screenPosition.z < 0)
            {
                screenPosition *= -1;
            }

            // Angle between the x-axis (bottom of screen) and a vector starting at zero(bottom-left corner of screen) and terminating at screenPosition.
            var angle = Mathf.Atan2(screenPosition.y, screenPosition.x);
            // Slope of the line starting from zero and terminating at screenPosition.
            var slope = Mathf.Tan(angle);

            //float deltaD = 1.5 - (vector.x * Vector.x * vector.x * Vector.x + Vector.y * vector.y * Vector.y * vector.y) / 4.0;

            // Two point's line's form is (y2 - y1) = m (x2 - x1) + c, 
            // starting point (x1, y1) is screen botton-left (0, 0),
            // ending point (x2, y2) is one of the screenBounds,
            // m is the slope
            // c is y intercept which will be 0, as line is passing through origin.
            // Final equation will be y = mx.
            if(screenPosition.x > 0)
            {
                // Keep the x screen position to the maximum x bounds and
                // find the y screen position using y = mx.
                screenPosition = new Vector3(screenBounds.x, screenBounds.x * slope, 0);
            }
            else
            {
                screenPosition = new Vector3(-screenBounds.x, -screenBounds.x * slope, 0);
            }
            // Incase the y ScreenPosition exceeds the y screenBounds 
            if(screenPosition.y > screenBounds.y)
            {
                // Keep the y screen position to the maximum y bounds and
                // find the x screen position using x = y/m.
                screenPosition = new Vector3(screenBounds.y / slope, screenBounds.y, 0);
            }
            else if(screenPosition.y < -screenBounds.y)
            {
                screenPosition = new Vector3(-screenBounds.y / slope, -screenBounds.y, 0);
            }

            // Bring the ScreenPosition back to its original reference.
            screenPosition += (Vector3)screenCentre;

            return angle;
        }*/

        /// <summary>
        /// Gets the screen position and angle for the arrow indicator. 
        /// </summary>
        /// <param name="screenPosition">Position of the target mapped to screen cordinates</param>
        /// <param name="screenCentre">The screen centre</param>
        /// <param name="screenBounds">The screen bounds</param>
        /// <returns>Angle of indicator</returns>
        public static float GetArrowIndicatorPositionAndAngle(ref Vector3 screenPosition, Vector2 screenCentre, Vector2 screenBounds, float radius)
        {
            // Our screenPosition's origin is screen's bottom-left corner.
            // But we have to get the arrow's screenPosition and rotation with respect to screenCentre.
            screenPosition -= (Vector3)screenCentre;

            // When the targets are behind the camera their projections on the screen (WorldToScreenPoint) are inverted,
            // so just invert them.
            if(screenPosition.z < 0)
            {
                screenPosition *= -1;
            }

            screenPosition.Normalize();

            // Angle between the x-axis (bottom of screen) and a vector starting at zero(bottom-left corner of screen) and terminating at screenPosition.
            var angle = Mathf.Atan2(screenPosition.y, screenPosition.x);
            var aspectRatio = screenBounds.y / screenBounds.x;

            radius *= 2;

            var px = Mathf.Pow(screenPosition.x, radius);
            var py = Mathf.Pow(screenPosition.y / aspectRatio, radius);

            var x = Mathf.Pow(px / (px + py), 1 / radius);

            //Debug.Log(x);

            var y = screenPosition.x == 0 ? aspectRatio * Mathf.Sign(screenPosition.y) : screenPosition.y / screenPosition.x * x;

            var sign = Mathf.Sign(screenPosition.x);

            screenPosition = new Vector2(x * screenBounds.x, y * screenBounds.x) * sign;

            // Bring the ScreenPosition back to its original reference.
            screenPosition += (Vector3)screenCentre;

            return angle;
        }

        public static Vector3 GetArrowIndicatorPositionByAngle(float angle, Vector2 screenCentre, Vector2 screenBounds, float radius)
        {
            var direction = Vector2.right;
            //direction.x = Mathf.Cos(angle) * x + Mathf.Sin(angle) * y;
            //direction.y = -Mathf.Sin(angle) * x + Mathf.Cos(angle) * y;
            direction.x = Mathf.Cos(angle);
            direction.y = Mathf.Sin(angle);

            // Angle between the x-axis (bottom of screen) and a vector starting at zero(bottom-left corner of screen) and terminating at screenPosition.
            var aspectRatio = screenBounds.y / screenBounds.x;
            direction.y *= aspectRatio / 3f;
            direction.Normalize();

            radius *= 2;

            var px = Mathf.Pow(direction.x, radius);
            var py = Mathf.Pow(direction.y, radius);

            var x = Mathf.Pow(px / (px + py), 1 / radius);

            //Debug.Log(x);

            var y = direction.x == 0 ? aspectRatio / 2 * Mathf.Sign(direction.y) : direction.y / direction.x * x;

            var sign = Mathf.Sign(direction.x);

            //return new Vector3(x * screenBounds.x * sign, y * screenBounds.x * sign, 0) + (Vector3)screenCentre;

            return new Vector3(x * screenBounds.x, y * screenBounds.y, 0) * sign + (Vector3)screenCentre;
        }

        /*public static Vector3 GetArrowIndicatorPositionByAngle(float angle, Vector2 screenCentre, Vector2 screenBounds, float radius)
        {
            var direction = Vector2.right;
            //direction.x = Mathf.Cos(angle) * x + Mathf.Sin(angle) * y;
            //direction.y = -Mathf.Sin(angle) * x + Mathf.Cos(angle) * y;
            direction.x = Mathf.Cos(angle);
            direction.y = Mathf.Sin(angle);

            // Angle between the x-axis (bottom of screen) and a vector starting at zero(bottom-left corner of screen) and terminating at screenPosition.
            var aspectRatio = screenBounds.y / screenBounds.x;

            radius *= 2;

            var px = Mathf.Pow(direction.x, radius);
            var py = Mathf.Pow(direction.y / aspectRatio, radius);

            var x = Mathf.Pow(px / (px + py), 1 / radius);

            //Debug.Log(x);

            var y = direction.x == 0 ? aspectRatio * Mathf.Sign(direction.y) : direction.y / direction.x * x;

            var sign = Mathf.Sign(direction.x);

            //return new Vector3(x * screenBounds.x * sign, y * screenBounds.x * sign, 0) + (Vector3)screenCentre;

            return new Vector3(x * screenBounds.x, y * screenBounds.x, 0) * sign + (Vector3)screenCentre;
        }*/
    }
}
