using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterArena
{
    [RequireComponent(typeof(Camera))]
    [ExecuteInEditMode]
    public class CameraDepthEnabler : MonoBehaviour
    {
        private void Awake()
        {
            var camera = GetComponent<Camera>();

            camera.depthTextureMode = DepthTextureMode.Depth;

#if UNITY_EDITOR
            camera.forceIntoRenderTexture = true;
#endif
        }
    }
}
