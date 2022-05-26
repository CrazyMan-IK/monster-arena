using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterArena
{
    [ExecuteAlways]
    public class CustomPostProcess : MonoBehaviour
    {
        [SerializeField] private Material _material = null;

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (_material == null)
            {
                return;
            }
            
            Graphics.Blit(source, destination, _material);
        }
    }
}
