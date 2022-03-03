using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MonsterArena
{
    public class EnemyAI : MonoBehaviour
    {
        [SerializeField] private Deck _deck = null;
        [SerializeField] private Transform _targetPoint = null;

        private Camera _camera = null;

        private void Awake()
        {
            _camera = Camera.main;
        }

        public void StartAI()
        {
            StartCoroutine(CustomUpdate());
        }

        private IEnumerator CustomUpdate()
        {
            while (_deck.HasActiveCards)
            {
                if (_deck.CooldownTime > 0)
                {
                    yield return null;
                    continue;
                }

                yield return new WaitForSeconds(1 + Random.value * 2);

                var card = _deck.GetRandomCard();
                var eventData = new PointerEventData(EventSystem.current);

                eventData.position = _camera.WorldToScreenPoint(card.transform.position);

                card.OnBeginDrag(eventData);

                var targetPos = _camera.WorldToScreenPoint(_targetPoint.position);
                while (Vector2.Distance(eventData.position, targetPos) > 0.01)
                {
                    yield return null;

                    eventData.position = Vector2.Lerp(eventData.position, targetPos, 10 * Time.deltaTime);
                    card.OnDrag(eventData);
                }

                eventData.position = targetPos;
                card.OnEndDrag(eventData);
            }
        }
    }
}
