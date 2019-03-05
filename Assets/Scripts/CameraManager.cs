using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
	[SerializeField] private int defaultPriority = 9;
	[SerializeField] private int mainFocusPriority = 10;
	[SerializeField] private CinemachineVirtualCamera[] gravityCams = null;
	[SerializeField] CinemachineVirtualCamera[] globalCams = null;
	[SerializeField] private AudioClip zoomInSound = null;
	[SerializeField] private AudioClip zoomOutSound = null;

	private Dictionary<PlayerController.CardinalDirection, CinemachineVirtualCamera> _vCams =
		new Dictionary<PlayerController.CardinalDirection, CinemachineVirtualCamera>();

	private Coroutine _lookingUpCoroutine;
	private Coroutine _shakingCoroutine;
	private CinemachineVirtualCamera _vCam;
	private CinemachineVirtualCamera _globalCam;
	private CinemachineBasicMultiChannelPerlin _noise;
	private bool _canToggleGlobalVcam = true;
	private AudioSource _myAudioSource;

	public bool CanToggleGlobalVcam
	{
		get => _canToggleGlobalVcam;
		set => _canToggleGlobalVcam = value;
	}

	void Start()
	{
		//setup the rotation of each camera correctly
		int i = 0;
		foreach (var cam in gravityCams)
		{
			_vCams.Add((PlayerController.CardinalDirection) i, cam);
			cam.transform.eulerAngles = Vector3.forward * 90 * i;
			cam.Priority = defaultPriority;
			cam.Follow = GameManager.Instance.Player.transform;
			++i;
		}

		foreach (var globalCam in globalCams)
		{
			globalCam.Priority = defaultPriority;
		}

		//setup the maincamera
		_vCam = _vCams[GameManager.Instance.Player.ActualGravityDirection];
		_globalCam = globalCams[0];
		_vCam.Priority = mainFocusPriority;
		_globalCam.Priority = defaultPriority;
		_myAudioSource = GetComponent<AudioSource>();
	}

	public void ChangeVCamByDirection(PlayerController.CardinalDirection direction)
	{
		_vCam.Priority = defaultPriority;
		_globalCam.Priority = defaultPriority;
		_vCam = _vCams[direction];
		_globalCam = globalCams[(int) direction];
		_vCam.Priority = mainFocusPriority;
	}

	public void ToggleGlobalCamera(bool value)
	{
		if (_canToggleGlobalVcam)
		{
			if (value)
			{
				_myAudioSource.PlayOneShot(zoomOutSound);
				_globalCam.Priority = mainFocusPriority + 1;
			}
			else
			{
				_myAudioSource.PlayOneShot(zoomInSound);
				_globalCam.Priority = defaultPriority;
			}
		}
	}

	public void Shake(float amplitudeGain, float frequencyGain, float time)
	{
		if (_shakingCoroutine != null)
		{
			StopCoroutine(_shakingCoroutine);
		}

		_shakingCoroutine = StartCoroutine(Shaking(amplitudeGain, frequencyGain, time));
	}

	private IEnumerator Shaking(float amplitudeGain, float frequencyGain, float time)
	{
		_noise = _vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
		Noise(amplitudeGain, frequencyGain);
		yield return new WaitForSeconds(time);
		Noise(0.0f, 0.0f);
	}

	private void Noise(float amplitudeGain, float frequencyGain)
	{
		_noise.m_AmplitudeGain = amplitudeGain;
		_noise.m_FrequencyGain = frequencyGain;
	}
}