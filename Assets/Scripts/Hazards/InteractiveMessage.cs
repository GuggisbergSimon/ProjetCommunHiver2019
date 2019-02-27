using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveMessage : Interactive
{
	[SerializeField] private Message message = null;
	[SerializeField] private AudioClip openSound = null;
	[SerializeField] private AudioClip closeSound = null;
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
			_isInteracting = true;
			base.Interact();
			_myAudioSource.PlayOneShot(openSound);
			GameManager.Instance.UIManager.PrintMessage(message);
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (_isInteracting)
		{
			_myAudioSource.PlayOneShot(closeSound);
			GameManager.Instance.UIManager.CloseMessage();
			_isInteracting = false;
		}
	}

}