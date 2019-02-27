using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetterGravity : MonoBehaviour
{
	[SerializeField] private float timeRespawn = 2.0f;
	[SerializeField] private float speedFloating = 1.0f;
	[SerializeField] private float amplitudeFloating = 1.0f;
	private bool _isActive = true;
	private SpriteRenderer _mySprite;
	private Vector2 _initPos;
	private ParticleSystem _myParticleSystem;
	private AudioSource _myAudioSource;

	private void Start()
	{
		_mySprite = GetComponentInChildren<SpriteRenderer>();
		_initPos = transform.position;
		_myParticleSystem = GetComponent<ParticleSystem>();
		_myAudioSource = GetComponent<AudioSource>();
	}

	private void Update()
	{
		transform.position =
			_initPos + (Vector2) transform.up * (Mathf.Sin(Time.time * speedFloating) * amplitudeFloating);
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player") && _isActive)
		{
			if (other.GetComponent<PlayerController>().RestoreGravityPower())
			{
				_isActive = false;
				_mySprite.enabled = false;
				_myParticleSystem.Stop();
				_myAudioSource.Stop();
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
		_myAudioSource.Play();
		_isActive = true;
		_mySprite.enabled = true;
		_myParticleSystem.Play();
	}
}