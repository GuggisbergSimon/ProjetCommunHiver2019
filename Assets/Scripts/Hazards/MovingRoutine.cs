using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingRoutine : MonoBehaviour
{
	[SerializeField] private MovingMode movingMode = 0;
	[SerializeField] private Point[] points = null;
	[SerializeField] private bool isActive = true;
	private int _indexPoints = 0;
	private bool _ascendantOrder = true;
	private Coroutine _movingCoroutine;
	private AudioSource _myAudioSource;

	[Serializable]
	private struct Point
	{
		public Vector2 position;
		public float angle;
		public float speedToReach;
		public float timeToStop;
	}

	private enum MovingMode
	{
		PingPong,
		LoopToBeginning,
		OnlyOnce
	}

	private void Start()
	{
		_myAudioSource = GetComponent<AudioSource>();
		if (isActive)
		{
			Activate();
		}
	}

	public void Activate()
	{
		isActive = true;
		if (_movingCoroutine != null)
		{
			StopCoroutine(_movingCoroutine);
		}

		_movingCoroutine = StartCoroutine(Moving());
	}

	private IEnumerator Moving()
	{
		_myAudioSource.Play();
		while (true)
		{
			while (_indexPoints < points.Length && _indexPoints >= 0)
			{
				if (!_myAudioSource.isPlaying)
				{
					_myAudioSource.Play();
				}

				Vector2 initPos = transform.position;
				Vector3 eulerAngles = transform.eulerAngles;
				float timer = 0.0f;
				Point actualPoint = points[_indexPoints];
				float timeToReach = Vector2.Distance(initPos, actualPoint.position) / actualPoint.speedToReach;
				while (timer < timeToReach)
				{
					timer += Time.deltaTime;
					transform.position = Vector2.Lerp(initPos, actualPoint.position, timer / timeToReach);
					transform.eulerAngles = Vector3.up * eulerAngles.y + Vector3.right * eulerAngles.x +
											Vector3.forward * Mathf.LerpAngle(eulerAngles.z, actualPoint.angle,
												timer / timeToReach);
					yield return null;
				}

				if (actualPoint.timeToStop > 0)
				{
					_myAudioSource.Stop();
				}

				yield return new WaitForSeconds(actualPoint.timeToStop);

				if (_ascendantOrder)
				{
					_indexPoints++;
				}
				else
				{
					_indexPoints--;
				}
			}

			if (movingMode == MovingMode.LoopToBeginning)
			{
				_indexPoints = 0;
			}
			else if (movingMode == MovingMode.PingPong)
			{
				_indexPoints += _ascendantOrder ? -1 : 1;
				_ascendantOrder = !_ascendantOrder;
			}
			else if (movingMode == MovingMode.OnlyOnce)
			{
				_myAudioSource.Stop();
				isActive = false;
				break;
			}
		}
	}
}