using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructiblePlatform : MonoBehaviour
{
	[SerializeField] private float beforeDestructingTime = 1.0f;
	[SerializeField] private float destroyedTime = 1.0f;
	[SerializeField] private float respawningTime = 1.0f;
	private bool _canBeActivated = true;
	private Collider2D _myCollider;
	private SpriteRenderer _mySprite;

	private void Start()
	{
		_myCollider = GetComponentInChildren<Collider2D>();
		_mySprite = GetComponentInChildren<SpriteRenderer>();
	}

	private void OnCollisionEnter2D(Collision2D other)
	{
		if (_canBeActivated)
		{
			StartCoroutine(Destroy());
		}
	}

	IEnumerator Destroy()
	{
		//todo change sprite
		yield return new WaitForSeconds(beforeDestructingTime);
		_myCollider.gameObject.SetActive(false);
		_mySprite.gameObject.SetActive(false);
		if (destroyedTime >= 0)
		{
			if (destroyedTime.CompareTo(0) != 0)
			{
				yield return new WaitForSeconds(destroyedTime);
			}

			//todo set respawning sprite
			yield return new WaitForSeconds(respawningTime);
			_canBeActivated = true;
			_myCollider.gameObject.SetActive(true);
			_mySprite.gameObject.SetActive(true);
		}
	}
}