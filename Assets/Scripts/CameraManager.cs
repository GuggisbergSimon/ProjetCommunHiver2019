using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    private CinemachineVirtualCamera vcam;
    private CinemachineBasicMultiChannelPerlin noise;

    void Start()
    {
        vcam = GameObject.Find("CM vcam1").GetComponent<CinemachineVirtualCamera>();
        noise = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void ChangePriorityOfVCam1By(int value)
    {
        vcam.Priority += value;
    }

    public void Noise(float amplitudeGain, float frequencyGain)
    {
        noise.m_AmplitudeGain = amplitudeGain;
        noise.m_FrequencyGain = frequencyGain;
    }
}
