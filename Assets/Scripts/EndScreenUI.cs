using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndScreenUI : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI deathsNumber;
	[SerializeField] private TextMeshProUGUI timeCounter;

	private void Start()
	{
		deathsNumber.text = GameManager.Instance.DeathsCounter.ToString()+ " deaths";
		deathsNumber.text = timeCounter.ToString()+" seconds";
	}
}
