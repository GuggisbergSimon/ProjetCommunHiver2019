using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
	[SerializeField] private string nameLevelToLoad = null;
	private bool _canBeInteractedWith;

	private void Update()
	{
		//todo move that part of the code in playerController
		if (_canBeInteractedWith && Input.GetAxis("Vertical") > 0)
		{
			_canBeInteractedWith = false;
			GameManager.Instance.CameraManager.CanToggleGlobalVcam = false;
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