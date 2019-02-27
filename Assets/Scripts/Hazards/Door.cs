using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Interactive
{
	[SerializeField] private string nameLevelToLoad = null;
	private bool _isInteracting;
	private AudioSource _myAudioSource;

	private void Start()
	{
		_myAudioSource = GetComponent<AudioSource>();
	}

	public override void Interact()
	{
		if (!_isInteracting)
		{
			_myAudioSource.Play();
			_isInteracting = true;
			base.Interact();
			GameManager.Instance.LoadLevel(nameLevelToLoad, true, true);
		}
	}
}