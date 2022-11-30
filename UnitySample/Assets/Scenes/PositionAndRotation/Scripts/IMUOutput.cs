using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class IMUOutput : MonoBehaviour
{
    public IMUBackend imuBackend;
    public UnityPositionAndRotation unityBackend;
    public TMPro.TMP_Text dialogText;
    private StreamWriter _streamWriter;
    private IEnumerator _loggingRoutine;

    private void Awake()
    {
        _loggingRoutine = Log();
    }

    private void Start()
    {
        if (!Directory.Exists(string.Format("{0}/logs/", Application.persistentDataPath)))
        {
            Directory.CreateDirectory(string.Format("{0}/logs/", Application.persistentDataPath));
        }
    }

    public void StartLogging()
    {
        var now = DateTime.Now.ToString("MM-dd-yyyy----HH-mm-ss");
        _streamWriter = new StreamWriter(string.Format("{0}/logs/{1}-{2}.csv", Application.persistentDataPath, "imu-log", now));
        _streamWriter.Write(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}\n",
            "time",
            "accel_x",
            "accel_y",
            "accel_z",
            "gyro_x",
            "gyro_y",
            "gyro_z",
            "magneto_x",
            "magneto_y",
            "magneto_z",
            "position_x",
            "position_y",
            "position_z"));

        StartCoroutine(_loggingRoutine);
    }

    public void EndLogging()
    {
        StopCoroutine(_loggingRoutine);
        if(_streamWriter != null) _streamWriter.Close();
    }

    private void Update()
    {
        //UpdateDialogBox();
    }

    private void UpdateDialogBox()
    {
        var accelerometer = imuBackend.accelerometer;
        var gyro = imuBackend.gyro;
        var magnetometer = imuBackend.magnetometer;
        var unityPosition = unityBackend.currentPosition;
        var unityQuat = unityBackend.currentRotation;
        var unityEuler = unityBackend.currentEulerAngles;

        string outputText =
            "Accelerometer:\n" + accelerometer.x + ", " + accelerometer.y + ", " + accelerometer.z +
            "\n\nGyro:\n" + gyro.x + ", " + gyro.y + ", " + gyro.z +
            "\n\nMagnetometer:\n" + magnetometer.x + ", " + magnetometer.y + ", " + magnetometer.z +
            "\n\nUnity Position:\n" + unityPosition.x + ", " + unityPosition.y + ", " + unityPosition.z +
            "\n\nUnity Rotation (Quaternion)\n" + unityQuat.x + ", " + unityQuat.y + ", " + unityQuat.z + ", " + unityQuat.w +
            "\n\nUnity Rotation (Euler Angles)\n" + unityEuler.x + ", " + unityEuler.y + ", " + unityEuler.z;

        dialogText.text = outputText;
    }

    public IEnumerator Log()
    {
        while(true)
        {
            yield return null;
            var time = Time.unscaledTime.ToString();

            var accelerometer = imuBackend.accelerometer;
            var gyro = imuBackend.gyro;
            var magnetometer = imuBackend.magnetometer;
            var unityPosition = unityBackend.currentPosition;

            _streamWriter.Write(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}\n",
                time,
                accelerometer.x,
                accelerometer.y,
                accelerometer.z,
                gyro.x,
                gyro.y,
                gyro.z,
                magnetometer.x,
                magnetometer.y,
                magnetometer.z,
                unityPosition.x,
                unityPosition.y,
                unityPosition.z));
        }

        yield return null;
    }

    private void OnDisable()
    {
        EndLogging();
    }

    private void OnDestroy()
    {
        EndLogging();
    }
}
