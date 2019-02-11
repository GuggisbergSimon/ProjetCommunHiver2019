using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetterGravity : MonoBehaviour
{
	private bool _isActive = true;

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player") && _isActive)
		{
			other.GetComponent<PlayerController>().RestoreGravityPower();
			_isActive = false;
			Destroy(gameObject);
		}
	}
}