using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class DestructiblePlatform : MonoBehaviour
{
	[SerializeField] private float beforeDestructingTime = 1.0f;
	[SerializeField] private float destroyedTime = 1.0f;
	[SerializeField] private float respawningTime = 1.0f;
	[SerializeField] private AudioClip crumblingBeforeSound = null;
	[SerializeField] private AudioClip crumblingAfterSound = null;
	private bool _canBeActivated = true;
	private Collider2D _myCollider;
	private Vector2 _initSpritePos;
	private AudioSource _myAudioSource;
	private Animator _myAnimator;

	private void Start()
	{
		_myCollider = GetComponent<Collider2D>();
		_myAudioSource = GetComponent<AudioSource>();
		_myAnimator = GetComponent<Animator>();
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
		_myAudioSource.PlayOneShot(crumblingBeforeSound);
		_myAnimator.SetTrigger("Shake");
		yield return new WaitForSeconds(beforeDestructingTime);

		//is destroyed
		_myCollider.enabled = false;
		_myAnimator.SetTrigger("Destroy");
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