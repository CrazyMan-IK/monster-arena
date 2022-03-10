using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterArena
{
    [RequireComponent(typeof(Animator))]
    public class MonsterAnimationEventsRepeater : MonoBehaviour
    {
        public event Action Attacked = null;
        public event Action Died = null;
        public event Action Winned = null;

        public void Attack()
        {
            Attacked?.Invoke();
        }

        public void HideBody()
        {
            Died?.Invoke();
        }

        public void WinAnimEnd()
        {
            Winned?.Invoke();
        }
    }
}
