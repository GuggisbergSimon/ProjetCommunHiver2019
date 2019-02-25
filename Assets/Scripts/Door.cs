using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Interactive
{
	[SerializeField] private string nameLevelToLoad = null;
	private bool _isInteracting;

	public override void Interact()
	{
		if (!_isInteracting)
		{
			_isInteracting = true;
			base.Interact();
			GameManager.Instance.LoadLevel(nameLevelToLoad, true, true);
		}
	}
}