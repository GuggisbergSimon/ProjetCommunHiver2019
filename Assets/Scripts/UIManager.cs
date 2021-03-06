﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	[SerializeField] private float fadingToBlackTime = 0.5f;
	[SerializeField] private Image blackPanel = null;
	[SerializeField] private GameObject dialoguePanel = null;
	[SerializeField] private TextMeshProUGUI textDisplayed = null;
	[SerializeField] private AudioClip openMessageSound = null;
	[SerializeField] private AudioClip closeMessageSound = null;
	private Coroutine _currentDialogue;
	private Message _currentMessage;
	private bool _isFadingToBlack;
	private AudioSource _myAudioSource;
	public bool IsFadingToBlack => _isFadingToBlack;

	public float FadingToBlackTime
	{
		get => fadingToBlackTime;
		set => fadingToBlackTime = value;
	}
	
	private void Start()
	{
		_myAudioSource = GetComponent<AudioSource>();
	}

	public void FadeToBlack(bool value)
	{
		StartCoroutine(FadingToBlack(value, fadingToBlackTime));
	}

	private IEnumerator FadingToBlack(bool value, float time)
	{
		_isFadingToBlack = true;
		blackPanel.gameObject.SetActive(true);
		float timer = 0.0f;
		Color tempColor = blackPanel.color;
		while (timer < time)
		{
			timer += Time.unscaledDeltaTime;
			tempColor.a = Mathf.Lerp(value ? 0.0f : 1.0f, value ? 1.0f : 0.0f, timer / time);
			blackPanel.color = tempColor;
			yield return null;
		}

		blackPanel.gameObject.SetActive(value);
		_isFadingToBlack = false;
	}

	public void PrintMessage(Message message)
	{
		_myAudioSource.PlayOneShot(openMessageSound);
		dialoguePanel.SetActive(true);
		if (_currentDialogue != null)
		{
			StopCoroutine(_currentDialogue);
		}

		_currentMessage = message;

		textDisplayed.color = _currentMessage.color;
		if (message.timeBetweenLetters.CompareTo(0) != 0)
		{
			_currentDialogue = StartCoroutine(PrintLetterByLetter());
		}
		else
		{
			PrintAll();
		}
	}

	public void CloseMessage()
	{
		_myAudioSource.PlayOneShot(closeMessageSound);
		dialoguePanel.SetActive(false);
	}

	private IEnumerator PrintLetterByLetter()
	{
		textDisplayed.text = "";
		for (int i = 0; i < _currentMessage.text.Length; i++)
		{
			textDisplayed.text += _currentMessage.text[i];
			yield return new WaitForSeconds(_currentMessage.timeBetweenLetters);
		}
	}

	private void PrintAll()
	{
		textDisplayed.text = _currentMessage.text;
	}
}