using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	[SerializeField] private float fadingToBlackTime = 0.5f;
	[SerializeField] private Image blackPanel = null;
	private bool _isFadingToBlack = false;
	public bool IsFadingToBlack => _isFadingToBlack;

	public void FadeToBlack(bool value)
	{
		StartCoroutine(FadingToBlack(value, fadingToBlackTime));
	}

	private IEnumerator FadingToBlack(bool value, float time)
	{
		_isFadingToBlack = true;
		float timer = 0.0f;

		Color tempColor = blackPanel.color;
		while (timer < time)
		{
			tempColor.a = Mathf.Lerp(value ? 0.0f : 1.0f, value ? 1.0f : 0.0f, timer / time);
			blackPanel.color = tempColor;
			timer += Time.unscaledDeltaTime;
			yield return null;
		}

		_isFadingToBlack = false;
	}
}