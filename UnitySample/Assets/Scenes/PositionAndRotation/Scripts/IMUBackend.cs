using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_WINMD_SUPPORT
using HL2UnityPlugin;
#endif

public class IMUBackend : MonoBehaviour
{
#if ENABLE_WINMD_SUPPORT
    private HL2ResearchMode _researchMode;
#endif

    private float[] _accelerometerSample = null;
    [HideInInspector]
    public Vector3 accelerometer;

    private float[] _gyroSample = null;
    [HideInInspector]
    public Vector3 gyro;

    private float[] _magnetometerSample = null;
    [HideInInspector]
    public Vector3 magnetometer;

    private void Start()
    {
#if WINDOWS_UWP
        _researchMode = new HL2ResearchMode();
        _researchMode.InitializeAccelSensor();
        _researchMode.InitializeGyroSensor();
        _researchMode.InitializeMagSensor();

        _researchMode.StartAccelSensorLoop();
        _researchMode.StartGyroSensorLoop();
        _researchMode.StartMagSensorLoop();
#endif
    }

    private void LateUpdate()
    {
#if WINDOWS_UWP
        if(_researchMode.AccelSampleUpdated())
        {
            _accelerometerSample = _researchMode.GetAccelSample();
        }

        if(_researchMode.GyroSampleUpdated())
        {
            _gyroSample = _researchMode.GetGyroSample();
        }

        if(_researchMode.MagSampleUpdated())
        {
            _magnetometerSample = _researchMode.GetMagSample();
        }
#endif

        accelerometer = ProcessAccelerometerSample(_accelerometerSample);
        magnetometer = ProcessMagnetometerSample(_magnetometerSample);
        gyro = ProcessGyroSampleIntoEulerAngles(_gyroSample);
    }

    private Vector3 ProcessAccelerometerSample(float[] sample)
    {
        Vector3 vec = Vector3.zero;
        if(sample != null && sample.Length == 3)
        {
            vec = new Vector3(
                sample[2],
                -1.0f * sample[0],
                -1.0f * sample[1]);
        }

        return vec;
    }

    private Vector3 ProcessGyroSampleIntoEulerAngles(float[] sample)
    {
        Vector3 vec = Vector3.zero;
        if(sample != null && sample.Length == 3)
        {
            vec = new Vector3(
                sample[2],
                sample[0],
                sample[1]);
        }

        return vec;
    }

    private Vector3 ProcessMagnetometerSample(float[] sample)
    {
        Vector3 vec = Vector3.zero;
        if (sample != null && sample.Length == 3)
        {
            vec = new Vector3(
                sample[2],
                sample[0],
                sample[1]);
        }

        return vec;
    }

    private void StopIMU()
    {
#if ENABLE_WINMD_SUPPORT
        _researchMode.StopAllSensorDevice();
#endif
    }
}
