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
	[SerializeField] private AudioClip crumblingBeforeSound = null;
	[SerializeField] private AudioClip crumblingAfterSound = null;
	private bool _canBeActivated = true;
	private Collider2D _myCollider;
	private SpriteRenderer _mySprite;
	private bool _isShaking;
	private float _timerShaking;
	private Vector2 _initSpritePos;
	private AudioSource _myAudioSource;
	private Animator _myAnimator;

	private void Start()
	{
		_initSpritePos = transform.position;
		_myCollider = GetComponent<Collider2D>();
		_mySprite = GetComponentInChildren<SpriteRenderer>();
		_myAudioSource = GetComponent<AudioSource>();
		_myAnimator = GetComponent<Animator>();
	}

	private void Update()
	{
		if (_isShaking)
		{
			_timerShaking += Time.deltaTime;
			_mySprite.transform.position = _initSpritePos +
			                               (Vector2) transform.right *
			                               (Mathf.Sin(_timerShaking * speedShaking) * amplitudeShaking);
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
		//crumbles
		_canBeActivated = false;
		_isShaking = true;
		_timerShaking = 0.0f;
		_myAudioSource.PlayOneShot(crumblingBeforeSound);
		yield return new WaitForSeconds(beforeDestructingTime);

		//is destroyed
		_myCollider.enabled = false;
		_myAnimator.SetTrigger("Destroy");
		_isShaking = false;
		_myAudioSource.PlayOneShot(crumblingAfterSound);
		if (destroyedTime >= 0)
		{
			if (destroyedTime.CompareTo(0) != 0)
			{
				yield return new WaitForSeconds(destroyedTime);
			}

			_myAnimator.SetTrigger("Respawn");
			yield return new WaitForSeconds(respawningTime);

			//respawns
			_myAnimator.SetTrigger("Respawned");
			_canBeActivated = true;
			_myCollider.enabled = true;
		}
	}
}