using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalFreezeDezoom : MonoBehaviour
{
	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player"))
		{
			GameManager.Instance.Player.CanDezoom = false;
		}
	}
}