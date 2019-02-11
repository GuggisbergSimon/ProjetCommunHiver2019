using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
	[SerializeField] private string nameLevelToLoad = null;
	private bool _canBeInteractedWith;

	private void Update()
	{
		if (_canBeInteractedWith && Input.GetAxis("Vertical") > 0)
		{
			GameManager.Instance.LoadLevel(nameLevelToLoad, true, true);
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player"))
		{
			_canBeInteractedWith = true;
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.CompareTag("Player"))
		{
			_canBeInteractedWith = false;
		}
	}
}