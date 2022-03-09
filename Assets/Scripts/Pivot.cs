using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterArena
{
    public class Pivot : MonoBehaviour
    {
        [SerializeField] private Transform _offset = null;

        public Transform GetAttachTransform()
        {
            return _offset;
        }

        public void SetOffset(Vector3 offset)
        {
            _offset.localPosition = offset;
        }
    }
}
