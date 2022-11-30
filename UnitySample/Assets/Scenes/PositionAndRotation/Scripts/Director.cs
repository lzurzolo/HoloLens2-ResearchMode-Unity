using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Director : MonoBehaviour
{
    private bool _isRecording;
    public IMUOutput imuOutput;
    public GameObject startButton;
    public GameObject endButton;
    public XsensSocket xsensSocket;

    private void Start()
    {
        _isRecording = false;    
    }

    private void Update()
    {
        if(_isRecording)
        {
            imuOutput.Log();
        }
    }

    public void StartXsensRecording()
    {
#if WINDOWS_UWP
        xsensSocket.StartXsensRecording();
#endif
        _isRecording = true;
        imuOutput.StartLogging();
        startButton.SetActive(false);
        endButton.SetActive(true);
    }

    public void StopXsensRecording()
    {
        _isRecording = false;
        imuOutput.EndLogging();
        endButton.SetActive(false);
        Application.Quit();
    }
}
