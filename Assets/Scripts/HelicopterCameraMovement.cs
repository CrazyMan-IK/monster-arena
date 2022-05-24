using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterArena
{
    [RequireComponent(typeof(CinemachineCameraOffset))]
    public class HelicopterCameraMovement : MonoBehaviour
    {
        private const float _CameraOffsetSpeed = 5;

        [SerializeField] private HelicopterMovement _helicopter = null;

        private CinemachineCameraOffset _offset = null;
        private float _baseZ = 0;
        private float _addition = 0;

        private void Awake()
        {
            _offset = GetComponent<CinemachineCameraOffset>();

            _baseZ = _offset.m_Offset.z;
        }

        private void Update()
        {
            _addition = Mathf.Lerp(_addition, _helicopter.CargoVisual, _CameraOffsetSpeed * Time.deltaTime);

            var position = _offset.m_Offset;
            position.z = Mathf.Lerp(_baseZ, _baseZ * 3.3f, (_helicopter.AirMultiplier + _addition) / 1.3f);
            _offset.m_Offset = position;
        }
    }
}
