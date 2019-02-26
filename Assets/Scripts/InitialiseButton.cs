using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InitialiseButton : MonoBehaviour
{
	private GameObject _lastselect;

	void Start()
	{
		_lastselect = new GameObject();
	}

	// Update is called once per frame
	void Update()
	{
		if (!EventSystem.current.currentSelectedGameObject)
		{
			Debug.Log("nothing selected, reverting");
			EventSystem.current.SetSelectedGameObject(_lastselect);
		}
		else
		{
			_lastselect = EventSystem.current.currentSelectedGameObject;
		}
	}
}