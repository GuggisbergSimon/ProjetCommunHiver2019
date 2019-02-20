using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetterGravity : MonoBehaviour
{
	[SerializeField] private float timeRespawn = 2.0f;
	private bool _isActive = true;
	private SpriteRenderer _mySprite;

	private void Start()
	{
		_mySprite = GetComponentInChildren<SpriteRenderer>();
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player") && _isActive)
		{
			if (other.GetComponent<PlayerController>().RestoreGravityPower())
			{
				_isActive = false;
				_mySprite.enabled = false;
				if (timeRespawn < 0)
				{
					Destroy(gameObject);
				}
				else
				{
					StartCoroutine(Respawn());
				}
			}
		}
	}

	private IEnumerator Respawn()
	{
		yield return new WaitForSeconds(timeRespawn);
		_isActive = true;
		_mySprite.enabled = true;
	}
}