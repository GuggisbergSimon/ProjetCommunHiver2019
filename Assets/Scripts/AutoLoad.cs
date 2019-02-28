using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoLoad : MonoBehaviour
{
	private void Start()
	{
		GameManager.Instance.LoadLevel("MainMenu", false, true);
	}
}
