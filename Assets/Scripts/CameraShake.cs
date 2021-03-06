using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using URandom = UnityEngine.Random;

namespace MonsterArena
{
	[Serializable]
	public class Shake
	{
		[field: SerializeField] public float Duration { get; set; } = 1;
		[field: SerializeField] public float Strength { get; set; } = 1;
	}

	[RequireComponent(typeof(Camera))]
	public class CameraShake : MonoBehaviour
	{
		[SerializeField] private float _shakeAmount = 0.7f;
		[SerializeField] private float _shakeSpeed = 7.0f;
		[SerializeField] private float _decreaseFactor = 1.0f;

		private static CameraShake _instance = null;
		private bool _initialized = false;
		private Vector3 _originalPos = Vector3.zero;
		private readonly List<Shake> _shakes = new List<Shake>();

		private void Awake()
		{
			if (_instance == null)
			{
				_instance = this;
			}
			else
            {
				Destroy(this);
            }

			_originalPos = transform.localPosition;
		}

		private void LateUpdate()
		{
			/*if (!_initialized)
            {
				Destroy(this);
				return;
            }*/

			if (_shakes.Count < 1)
			{
				transform.localPosition = Vector3.Slerp(transform.localPosition, _originalPos, _shakeSpeed * Time.deltaTime);
				return;
			}

			var resultShake = Vector3.zero;
			for (int i = 0; i < _shakes.Count; i++)
			{
				var shake = _shakes[i];
				if (shake.Duration > 0)
				{
					resultShake += URandom.insideUnitSphere * (_shakeAmount * shake.Duration * shake.Strength);

					shake.Duration -= Time.deltaTime * _decreaseFactor;
				}
				else
				{
					_shakes.RemoveAt(i);
					i--;
				}
			}

			transform.localPosition = Vector3.Slerp(transform.localPosition, _originalPos + resultShake, _shakeSpeed * Time.deltaTime);
		}

		public static void AddShake(float duration, float strength)
		{
			if (_instance == null)
            {
				_instance = Camera.main.gameObject.AddComponent<CameraShake>();

				//_instance._originalPos = _instance.transform.localPosition;
				//_instance._initialized = true;
            }

			_instance._shakes.Add(new Shake() { Duration = duration, Strength = strength });
		}
	}
}
