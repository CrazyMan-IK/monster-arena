using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonsterArena.Models;

namespace MonsterArena
{
    public class Deck : MonoBehaviour
    {
        const float _CooldownDuration = 5;

        [SerializeField] private DeckInformation _deck = null;
        [SerializeField] private Transform _arena = null;
        [SerializeField] private LayerMask _arenaLayerMask = default;
        [SerializeField] private LayerMask _enemyLayerMask = default;

        private readonly List<MonsterCard> _cards = new List<MonsterCard>();

        private float _cooldownTime = 0;

        public bool HasAliveMonsters
        {
            get
            {
                foreach (var card in _cards)
                {
                    if (card.IsAlive)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool HasActiveCards
        {
            get
            {
                foreach (var card in _cards)
                {
                    if (card.IsActive)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public float CooldownTime => _cooldownTime;

        private void Awake()
        {
            var step = -10.0f / (_deck.Monsters.Count + 1);

            for (int i = 0; i < _deck.Monsters.Count; i++)
            {
                var monsterInfo = _deck.Monsters[i];

                var position = transform.position + step * (_deck.Monsters.Count / 2.0f - i - 0.5f) * transform.right;// + step * Mathf.Abs(_deck.Monsters.Count / 2.0f - i - 0.5f) * transform.up;
                var rotation = monsterInfo.MonsterPrefab.transform.rotation * Quaternion.LookRotation(Camera.main.transform.position - position, Camera.main.transform.up);

                var card = Instantiate(monsterInfo.MonsterPrefab, position, rotation, transform);
                //SetLayerRecursively(card.gameObject, gameObject.layer);
                card.gameObject.layer = gameObject.layer;
                card.Initialize(_arena, _arenaLayerMask, _enemyLayerMask, monsterInfo);

                card.Used += OnCardUsed;

                _cards.Add(card);
            }
        }

        private void Update()
        {
            if (_cooldownTime > 0)
            {
                _cooldownTime -= Time.deltaTime;
            }

            if (_cooldownTime < 0)
            {
                _cooldownTime = 0;

                foreach (var card in _cards)
                {
                    card.enabled = true;
                }
            }
        }

        public void ActivateWinAnimation()
        {
            foreach (var card in _cards)
            {
                card.ActivateWinAnimation();
            }
        }

        public MonsterCard GetRandomCard()
        {
            /*MonsterCard result;

            do
            {
                result = _cards[Random.Range(0, _cards.Count)];
            }
            while (!result.IsAlive);

            return result;*/

            return _cards.Where(x => x.IsAlive && x.IsActive).OrderBy(x => Random.value).First();
        }

        private void SetLayerRecursively(GameObject go, int layerMask)
        {
            go.layer = layerMask;
            foreach (Transform child in go.transform)
            {
                SetLayerRecursively(child.gameObject, layerMask);
            }
        }

        private void OnCardUsed()
        {
            _cooldownTime = _CooldownDuration;

            foreach (var card in _cards)
            {
                card.enabled = false;
            }
        }
    }
}
