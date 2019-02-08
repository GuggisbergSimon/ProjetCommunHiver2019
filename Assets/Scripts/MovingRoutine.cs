using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingRoutine : MonoBehaviour
{
	[SerializeField] private MovingMode movingMode = 0;
	[SerializeField] private Point[] points;
	private int indexPoints;

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
		BackToBeginning,
		OnlyOnce
	}

	private void Start()
	{
		StartCoroutine(Moving(true));
	}

	private IEnumerator Moving(bool ascendantOrder)
	{
		while (indexPoints < points.Length)
		{
			Vector2 initPos = transform.position;
			float timer = 0.0f;
			Point actualPoint = points[indexPoints];
			while (timer < actualPoint.timeToReach)
			{
				timer += Time.deltaTime;
				transform.position = Vector2.Lerp(initPos, actualPoint.position, timer / actualPoint.timeToReach);
				yield return null;
			}
			yield return new WaitForSeconds(actualPoint.timeToStop);
			if (ascendantOrder)
			{
				indexPoints++;
			}
			else
			{
				indexPoints--;
			}
		}

		if (movingMode == MovingMode.BackToBeginning)
		{
			StartCoroutine(Moving(true));
		}
		else if (movingMode == MovingMode.PingPong)
		{
			StartCoroutine(Moving(!ascendantOrder));
		}
		//else : only once : do nothing
	}

	//todo start coroutin, move from actualpos to point i in a given time, then wait then iterate, i%points.length
}