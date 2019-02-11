using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingRoutine : MonoBehaviour
{
	[SerializeField] private MovingMode movingMode = 0;
	[SerializeField] private Point[] points;
	private int _indexPoints = 0;
	private bool _ascendantOrder = true;

	[Serializable]
	private struct Point
	{
		public Vector2 position;
		public float timeToReach;
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
		StartCoroutine(Moving());
	}

	private IEnumerator Moving()
	{
		while (true)
		{
			while (_indexPoints < points.Length && _indexPoints >= 0)
			{
				Vector2 initPos = transform.position;
				float timer = 0.0f;
				Point actualPoint = points[_indexPoints];
				while (timer < actualPoint.timeToReach)
				{
					timer += Time.deltaTime;
					transform.position = Vector2.Lerp(initPos, actualPoint.position, timer / actualPoint.timeToReach);
					yield return null;
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
			else
			{
				break;
			}
		}
	}
}