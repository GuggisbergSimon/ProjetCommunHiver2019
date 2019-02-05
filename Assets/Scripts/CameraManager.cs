using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
	[SerializeField] private int defaultPriority = 9;
	[SerializeField] private int mainFocusPriority = 10;
	private Dictionary<PlayerController.CardinalDirection, CinemachineVirtualCamera> _vCams = new Dictionary<PlayerController.CardinalDirection, CinemachineVirtualCamera>();
	private CinemachineVirtualCamera _vCam;
	private CinemachineBasicMultiChannelPerlin _noise;

	void Start()
	{
		int i = 0;
		foreach (var cam in FindObjectsOfType<CinemachineVirtualCamera>())
		{
			_vCams.Add((PlayerController.CardinalDirection) i,cam);
			cam.transform.eulerAngles = Vector3.forward * 90 * i;
			Debug.Log(i + cam.name);
			++i;
		}

		;
		_vCam = _vCams[GameManager.Instance.Player.ActualGravityDirection];
		_vCam.Priority = mainFocusPriority;
		_noise = _vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
	}

	public void ChangeVCamByDirection(PlayerController.CardinalDirection direction)
	{
		_vCam.Priority = defaultPriority;
		_vCam = _vCams[direction];
		_vCam.Priority = mainFocusPriority;
	}

	public void Noise(float amplitudeGain, float frequencyGain)
	{
		_noise.m_AmplitudeGain = amplitudeGain;
		_noise.m_FrequencyGain = frequencyGain;
	}
}