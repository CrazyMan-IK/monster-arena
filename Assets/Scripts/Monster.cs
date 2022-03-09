using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonsterArena.Models;

namespace MonsterArena
{
    public class Monster : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer = null;

        private MonsterInformation _information = null;

        public Renderer Renderer => _renderer;
        public MonsterInformation Information => _information;

        public void Initialize(MonsterInformation information)
        {
            _information = information;
        }
    }
}
