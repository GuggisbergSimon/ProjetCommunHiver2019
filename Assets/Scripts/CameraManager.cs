using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
	private Dictionary<CinemachineVirtualCamera, PlayerController.CardinalDirection> vcams;
	//private CinemachineVirtualCamera[] _vcams;
	private CinemachineVirtualCamera _vcam;
	private CinemachineBasicMultiChannelPerlin _noise;

	void Start()
	{
		GameObject.FindObjectOfType<CinemachineVirtualCamera>();
		_vcam = GameObject.Find("CM vcam1").GetComponent<CinemachineVirtualCamera>();
		_noise = _vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
	}

	public void ChangePriorityofVCam(PlayerController.CardinalDirection direction)
	{
		
	}

	public void Noise(float amplitudeGain, float frequencyGain)
	{
		_noise.m_AmplitudeGain = amplitudeGain;
		_noise.m_FrequencyGain = frequencyGain;
	}
}