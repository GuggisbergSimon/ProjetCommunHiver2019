using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class DestructiblePlatform : MonoBehaviour
{
	[SerializeField] private float beforeDestructingTime = 1.0f;
	[SerializeField] private float destroyedTime = 1.0f;
	[SerializeField] private float respawningTime = 1.0f;
	[SerializeField] private float speedShaking = 1.0f;
	[SerializeField] private float amplitudeShaking = 1.0f;
	private bool _canBeActivated = true;
	private Collider2D _myCollider;
	private SpriteRenderer _mySprite;
	private bool _isShaking;
	private float _timerShaking;
	private Vector2 _initSpritePos;

	private void Start()
	{
		_initSpritePos = transform.position;
		_myCollider = GetComponent<Collider2D>();
		_mySprite = GetComponentInChildren<SpriteRenderer>();
	}

	private void Update()
	{
		if (_isShaking)
		{
			_timerShaking += Time.deltaTime;
			_mySprite.transform.position = _initSpritePos +
			                               Vector2.right * (Mathf.Sin(_timerShaking * speedShaking) * amplitudeShaking);
		}
	}

	private void OnCollisionEnter2D(Collision2D other)
	{
		if (_canBeActivated && other.gameObject.CompareTag("Player"))
		{
			StartCoroutine(Destroy());
		}
	}

	IEnumerator Destroy()
	{
		_isShaking = true;
		_timerShaking = 0.0f;
		yield return new WaitForSeconds(beforeDestructingTime);
		_myCollider.enabled = false;
		_mySprite.enabled = false;
		_isShaking = false;
		if (destroyedTime >= 0)
		{
			if (destroyedTime.CompareTo(0) != 0)
			{
				yield return new WaitForSeconds(destroyedTime);
			}

			//todo set respawning sprite
			yield return new WaitForSeconds(respawningTime);
			_canBeActivated = true;
			_myCollider.enabled = true;
			_mySprite.enabled = true;
		}
	}
}