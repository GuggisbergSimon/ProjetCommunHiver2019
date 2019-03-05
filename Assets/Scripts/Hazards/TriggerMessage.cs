using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerMessage : MonoBehaviour
{
	[SerializeField] private Message message = null;
	private bool _isRead;
	
	private void OnTriggerEnter2D(Collider2D other)
	{
		if (!_isRead)
		{
			GameManager.Instance.UIManager.PrintMessage(message);
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		GameManager.Instance.UIManager.CloseMessage();
		_isRead = true;
	}
}