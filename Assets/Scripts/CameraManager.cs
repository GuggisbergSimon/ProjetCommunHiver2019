using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
	[SerializeField] private int defaultPriority = 9;
	[SerializeField] private int mainFocusPriority = 10;
	[SerializeField] private float maxHeightLookUp = 4.0f;
	[SerializeField] private float lookUpSpeed = 10.0f;
	[SerializeField] private CinemachineVirtualCamera[] gravityCams = null;
	[SerializeField] private CinemachineVirtualCamera globalCam = null;

	private Dictionary<PlayerController.CardinalDirection, CinemachineVirtualCamera> _vCams =
		new Dictionary<PlayerController.CardinalDirection, CinemachineVirtualCamera>();

	private Coroutine lookingUpCoroutine;
	private CinemachineVirtualCamera _vCam;
	private CinemachineBasicMultiChannelPerlin _noise;
	private bool _canToggleGlobalVcam = true;

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

		//setup the maincamera
		_vCam = _vCams[GameManager.Instance.Player.ActualGravityDirection];
		_vCam.Priority = mainFocusPriority;
		globalCam.Priority = defaultPriority;
		_noise = _vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
	}

	public void ChangeVCamByDirection(PlayerController.CardinalDirection direction)
	{
		_vCam.Priority = defaultPriority;
		_vCam = _vCams[direction];
		_vCam.Priority = mainFocusPriority;
	}

	public void ToggleGlobalCamera(bool value)
	{
		if (_canToggleGlobalVcam)
		{
			if (value)
			{
				globalCam.Priority = mainFocusPriority + 1;
			}
			else
			{
				globalCam.Priority = defaultPriority;
			}
		}
	}

	public void Noise(float amplitudeGain, float frequencyGain)
	{
		_noise.m_AmplitudeGain = amplitudeGain;
		_noise.m_FrequencyGain = frequencyGain;
	}
}