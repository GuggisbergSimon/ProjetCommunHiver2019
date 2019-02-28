using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonInterface : MonoBehaviour
{
	public void LoadLevelFadeInAndOut(string nameLevel)
	{
		GameManager.Instance.LoadLevelFadeInAndOut(nameLevel);
	}

	public void QuitGame()
	{
		GameManager.Instance.QuitGame();
	}

	public void ChangeTimeScale(float timeScale)
	{
		GameManager.Instance.ChangeTimeScale(timeScale);
	}

	public void NoVomitMode(bool value)
	{
		GameManager.Instance.NoVomitMode(value);
	}

	public void ToggleTimer(bool value)
	{
		GameManager.Instance.ToggleTimer(value);
	}

	public void ResetDeathsCounterAndTimer()
	{
		GameManager.Instance.ResetDeathsCounterAndTimer();
	}
}
