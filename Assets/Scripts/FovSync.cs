using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterArena
{
    [RequireComponent(typeof(Camera))]
    [ExecuteAlways]
    public class FovSync : MonoBehaviour
    {
        [SerializeField] private Camera _parent = null;

        private Camera _camera = null;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void Update()
        {
            if (_parent == null)
            {
                return;
            }

            _camera.fieldOfView = _parent.fieldOfView;
        }
    }
}
