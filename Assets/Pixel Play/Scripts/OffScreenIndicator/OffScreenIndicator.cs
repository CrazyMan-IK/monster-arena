using PixelPlay.OffScreenIndicator;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Attach the script to the off screen indicator panel.
/// </summary>
[DefaultExecutionOrder(-1)]
public class OffScreenIndicator : MonoBehaviour
{
    //[Range(0.5f, 0.9f)]
    [Tooltip("Distance offset of the indicators from the centre of the screen")]
    //[SerializeField] private float screenBoundOffset = 0.9f;
    [SerializeField] private Vector2 screenBoundOffset = Vector2.one * 32;
    [SerializeField] private float _minDistance = 192;
    [SerializeField] private float _minAngle = 15;
    [SerializeField] private float _radius = 3;

    private Camera mainCamera;
    private Vector2 screenCentre;
    private Vector2 screenBounds;

    private List<Target> targets = new List<Target>();

    public static Action<Target, bool> TargetStateChanged;

    void Awake()
    {
        mainCamera = Camera.main;
        screenCentre = new Vector2(Screen.width, Screen.height) / 2;
        screenBounds = screenCentre - screenBoundOffset; //* screenBoundOffset;
        TargetStateChanged += HandleTargetStateChanged;
    }

    void LateUpdate()
    {
        DrawIndicators();
    }

    /// <summary>
    /// Draw the indicators on the screen and set thier position and rotation and other properties.
    /// </summary>
    void DrawIndicators()
    {
        foreach(Target target in targets)
        {
            Vector3 screenPosition = OffScreenIndicatorCore.GetScreenPosition(mainCamera, target.transform.position);
            bool isTargetVisible = OffScreenIndicatorCore.IsTargetVisible(screenPosition);
            float distanceFromCamera = target.NeedDistanceText ? target.GetDistanceFromCamera(mainCamera.transform.position) : float.MinValue;// Gets the target distance from the camera.
            Indicator indicator = null;

            if(target.NeedBoxIndicator && isTargetVisible)
            {
                screenPosition.z = 0;
                indicator = GetIndicator(ref target.indicator, IndicatorType.BOX); // Gets the box indicator from the pool.
            }
            else if(target.NeedArrowIndicator && !isTargetVisible)
            {
                var angle = OffScreenIndicatorCore.GetArrowIndicatorPositionAndAngle(ref screenPosition, screenCentre, screenBounds, _radius);
                indicator = GetIndicator(ref target.indicator, IndicatorType.ARROW); // Gets the arrow indicator from the pool.
                indicator.transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg); // Sets the rotation for the arrow indicator.
            }
            if(indicator)
            {
                indicator.SetImageColor(target.TargetColor);// Sets the image color of the indicator.
                indicator.SetDistanceText(distanceFromCamera); //Set the distance text for the indicator.
                //indicator.transform.position = screenPosition; //Sets the position of the indicator on the screen.
                indicator.TargetPosition = screenPosition; //Sets the position of the indicator on the screen.
                indicator.SetTextRotation(Quaternion.identity); // Sets the rotation of the distance text of the indicator.
            }
            else if (target.indicator != null)
            {
                target.indicator.Activate(false);
            }
        }

        for (int i = 0; i < targets.Count; i++)
        {
            var target1 = targets[i];
            if (target1.indicator == null)
            {
                continue;
            }

            //Debug.DrawRay(target1.indicator.TargetPosition, Vector3.forward * 100, Color.red);

            //for (int j = i + 1; j < targets.Count; j++)
            for (int j = 0; j < targets.Count; j++)
            {
                if (j == i)
                {
                    continue;
                }

                var target2 = targets[j];
                if (target2.indicator == null)
                {
                    continue;
                }

                /*var pos1 = target1.indicator.TargetPosition;
                var pos2 = target2.indicator.TargetPosition;

                if (Vector3.Distance(pos1, pos2) <= _minDistance)
                {
                    var center = (pos1 + pos2) / 2;
                    var direction = pos2 - pos1;
                    direction.Normalize();
                    direction /= 2;

                    pos1 = center + direction * _minDistance;
                    pos2 = center - direction * _minDistance;

                    target1.indicator.TargetPosition = pos1;
                    target2.indicator.TargetPosition = pos2;

                    Debug.DrawRay(target1.indicator.TargetPosition, Vector3.forward * 90, Color.blue);
                }*/

                var a1 = target1.indicator.transform.eulerAngles.z;
                var a2 = target2.indicator.transform.eulerAngles.z;

                if (Mathf.Abs(a1 - a2) < _minAngle)
                {
                    var avg = (a1 + a2) / 2;
                    var sign = Mathf.Sign(a1 - a2) / 2;

                    a1 = avg + sign * _minAngle;
                    a2 = avg - sign * _minAngle;

                    target1.indicator.TargetPosition = OffScreenIndicatorCore.GetArrowIndicatorPositionByAngle(a1 * Mathf.Deg2Rad, screenCentre, screenBounds, _radius);
                    target2.indicator.TargetPosition = OffScreenIndicatorCore.GetArrowIndicatorPositionByAngle(a2 * Mathf.Deg2Rad, screenCentre, screenBounds, _radius);

                    target1.indicator.transform.rotation = Quaternion.Euler(0, 0, a1);
                    target2.indicator.transform.rotation = Quaternion.Euler(0, 0, a2);
                }
            }
        }
    }

    /// <summary>
    /// 1. Add the target to targets list if <paramref name="active"/> is true.
    /// 2. If <paramref name="active"/> is false deactivate the targets indicator, 
    ///     set its reference null and remove it from the targets list.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="active"></param>
    private void HandleTargetStateChanged(Target target, bool active)
    {
        if(active)
        {
            targets.Add(target);
        }
        else
        {
            target.indicator?.Activate(false);
            target.indicator = null;
            targets.Remove(target);
        }
    }

    /// <summary>
    /// Get the indicator for the target.
    /// 1. If its not null and of the same required <paramref name="type"/> 
    ///     then return the same indicator;
    /// 2. If its not null but is of different type from <paramref name="type"/> 
    ///     then deactivate the old reference so that it returns to the pool 
    ///     and request one of another type from pool.
    /// 3. If its null then request one from the pool of <paramref name="type"/>.
    /// </summary>
    /// <param name="indicator"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    private Indicator GetIndicator(ref Indicator indicator, IndicatorType type)
    {
        if(indicator != null)
        {
            if(indicator.Type != type)
            {
                indicator.Activate(false);
                indicator = type == IndicatorType.BOX ? BoxObjectPool.current.GetPooledObject() : ArrowObjectPool.current.GetPooledObject();
                indicator.Activate(true); // Sets the indicator as active.
            }
        }
        else
        {
            indicator = type == IndicatorType.BOX ? BoxObjectPool.current.GetPooledObject() : ArrowObjectPool.current.GetPooledObject();
            indicator.Activate(true); // Sets the indicator as active.
        }
        return indicator;
    }

    private void OnDestroy()
    {
        TargetStateChanged -= HandleTargetStateChanged;
    }
}
