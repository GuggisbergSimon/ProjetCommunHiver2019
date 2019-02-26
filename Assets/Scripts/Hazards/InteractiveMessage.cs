using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveMessage : Interactive
{
	[SerializeField] private Message message = null;
	private bool _isInteracting;

	public override void Interact()
	{
		if (!_isInteracting)
		{
			_isInteracting = true;
			base.Interact();
			GameManager.Instance.UIManager.PrintMessage(message);
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (_isInteracting)
		{
			GameManager.Instance.UIManager.CloseMessage();
			_isInteracting = false;
		}
	}

}