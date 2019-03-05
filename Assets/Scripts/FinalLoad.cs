using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalLoad : MonoBehaviour
{
	[SerializeField] private float fadeToBlackTimeFinal = 5.0f;

	private void OnTriggerEnter2D(Collider2D other)
	{
		GameManager.Instance.UIManager.FadingToBlackTime = fadeToBlackTimeFinal;
		GameManager.Instance.LoadLevelFadeInAndOut("End");
	}
}