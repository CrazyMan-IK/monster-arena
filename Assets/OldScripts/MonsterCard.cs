using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using RootMotion.Dynamics;
using MonsterArena.Models;
using MonsterArena.Extensions;

namespace MonsterArena
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(MonsterAI))]
    [RequireComponent(typeof(MonsterAnimation))]
    public class MonsterCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private const float _SpeedMultiplier = 10;

        [SerializeField] private Transform _offsetPoint = null;
        [SerializeField] private ParticleSystem _dropEffect = null;
        [SerializeField] private PuppetMaster _ragdoll = null;
        [SerializeField] private Rigidbody _head = null;

        public event Action Used = null;

        private Camera _camera = null;
        private Rigidbody _rigidbody = null;
        private MonsterAI _ai = null;
        private MonsterAnimation _animation = null;

        private Vector3 _lastPosition = Vector3.zero;
        private Quaternion _lastRotation = Quaternion.identity;
        private Vector3 _currentPosition = Vector3.zero;
        private Quaternion _currentRotation = Quaternion.identity;
        //private Vector3 _currentOffset = Vector3.zero;

        private Transform _rotationTarget = null;
        private LayerMask _arenaLayerMask = default;

        private ArenaBody _arenaBody = null;
        private RaycastHit _lastHitInformation = default;

        private bool _isActive = true;

        public bool IsAlive => _ai.IsAlive;
        public bool IsActive => _isActive;

        private void Awake()
        {
            _camera = Camera.main;
            _rigidbody = GetComponent<Rigidbody>();
            _ai = GetComponent<MonsterAI>();
            _animation = GetComponent<MonsterAnimation>();

            _currentPosition = _head.transform.position;// - _offsetPoint.localPosition;
            _currentRotation = _head.transform.rotation;

            _head.isKinematic = true;
            //_rigidbody.constraints |= RigidbodyConstraints.FreezeRotationY;
        }

        private void Update()
        {
            if (!_isActive)
            {
                return;
            }

            /*transform.SetPositionAndRotation(
                Vector3.Lerp(transform.position, _currentPosition - _currentOffset, Time.deltaTime * _SpeedMultiplier), 
                Quaternion.Slerp(transform.rotation, _currentRotation, Time.deltaTime * _SpeedMultiplier));*/

            _head.transform.SetPositionAndRotation(
                Vector3.Lerp(_head.transform.position, _currentPosition, Time.deltaTime * _SpeedMultiplier),
                Quaternion.Slerp(_head.transform.rotation, _currentRotation, Time.deltaTime * _SpeedMultiplier));

            //var deltaPosition = _SpeedMultiplier * Time.deltaTime * (_rigidbody.position - _currentPosition).normalized;
            //_rigidbody.MovePosition(_rigidbody.position + deltaPosition);
            /*_rigidbody.MovePosition(_currentPosition);

            var deltaRotation = (_rigidbody.rotation * Quaternion.Inverse(_currentRotation)).normalized;
            _rigidbody.MoveRotation(_rigidbody.rotation * deltaRotation);*/
        }

        public void Initialize(Transform rotationTarget, LayerMask arenaLayerMask, LayerMask enemyLayerMask, MonsterInformation information)
        {
            _rotationTarget = rotationTarget;
            _arenaLayerMask = arenaLayerMask;

            _ai.Initialize(enemyLayerMask, information);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _lastPosition = _currentPosition;
            _lastRotation = _currentRotation;

            _ragdoll.state = PuppetMaster.State.Dead;
            //_ragdoll.Kill();
            _head.isKinematic = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            _arenaBody = null;

            var distance = Vector3.Distance(_lastPosition, _camera.transform.position) - 5f;

            _currentPosition = _camera.ScreenToWorldPoint((Vector3)eventData.position + Vector3.forward * distance);
            var direction = _currentPosition - _camera.transform.position;

            _currentRotation = Quaternion.identity;

            //_currentPosition -= transform.InverseTransformPoint(_offsetPoint.position);
            //_currentOffset = transform.InverseTransformPoint(_offsetPoint.position);
            if (Physics.Raycast(_camera.transform.position, direction, out _lastHitInformation, 1000, _arenaLayerMask) && _lastHitInformation.collider.TryGetComponent(out _arenaBody))
            {
                var upOffset = _lastHitInformation.transform.up * 4;
                _currentPosition = _lastHitInformation.point + upOffset;

                var angle = AngleByLookAt(_rotationTarget.position, _currentPosition);

                _currentRotation = Quaternion.AngleAxis(angle, Vector3.up);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _ragdoll.state = PuppetMaster.State.Alive;
            //_ragdoll.Resurrect();

            if (_arenaBody != null)
            {
                var seq = DOTween.Sequence();

                //_ragdoll.state = PuppetMaster.State.Frozen;

                transform.position = _lastHitInformation.point + _lastHitInformation.transform.up * 4;
                _ragdoll.DisableImmediately();
                //_ragdoll.SwitchToDisabledMode();

                //seq.AppendInterval(0.5f);
                //seq.Append(transform.DOMove(_lastHitInformation.point + _lastHitInformation.transform.up * 4, 0.5f).SetSpeedBased());
                seq.Append(transform.DOMove(_lastHitInformation.point, 0.5f).SetEase(Ease.InExpo));
                seq.Join(transform.DORotate(_currentRotation.eulerAngles, 0.5f));

                seq.OnComplete(() =>
                {
                    //_ragdoll.mode = PuppetMaster.Mode.Disabled;
                    //_ragdoll.DisableImmediately();

                    _rigidbody.isKinematic = false;
                    _ai.enabled = true;
                    _animation.enabled = true;
                    _dropEffect.Play();
                });

                _isActive = false;

                //_ragdoll.mode = PuppetMaster.Mode.Kinematic;

                Used?.Invoke();
            }
            else
            {
                //_ragdoll.state = PuppetMaster.State.Alive;
            }

            _currentPosition = _lastPosition;
            _currentRotation = _lastRotation;
        }

        public void ActivateWinAnimation()
        {
            _animation.ActivateWinAnimation();
            _ai.enabled = false;
            //_rigidbody.constraints |= RigidbodyConstraints.FreezeRotationY;
        }

        private float AngleByLookAt(Vector3 a, Vector3 b)
        {
            var diff = a.GetXZ() - b.GetXZ();

            float angle = Mathf.Atan2(diff.x, diff.y) * Mathf.Rad2Deg;

            return angle;
        }
    }
}