using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBehaviour : MonoBehaviour
{
	[SerializeField] private float warmUpTime = 0.5f;
	[SerializeField] private float activeTime = 2.0f;
	[SerializeField] private float inactiveTime = 2.0f;
	private bool _isActive = true;
	private SpriteRenderer _mySprite = null;

	private void Start()
	{
		//_mySprite = GetComponentInChildren<SpriteRenderer>();
		//StartCoroutine(SimpleRoutine());
	}

	private IEnumerator SimpleRoutine()
	{
		while (true)
		{
			//todo change _mySprite to warmingup
			_mySprite.color=Color.grey;
			yield return new WaitForSeconds(warmUpTime);
			//todo change _mySprite to active
			_mySprite.color = Color.white;
			_isActive = true;
			yield return new WaitForSeconds(activeTime);
			//todo change _mySprite to inactive
			_mySprite.color = Color.clear;
			_isActive = false;
			yield return new WaitForSeconds(inactiveTime);
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (_isActive && other.CompareTag("Player"))
		{
			other.GetComponent<PlayerController>().Die();
		}
	}
}