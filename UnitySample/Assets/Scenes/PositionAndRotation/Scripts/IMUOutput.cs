using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IMUOutput : MonoBehaviour
{
    public IMUBackend imuBackend;
    public UnityPositionAndRotation unityBackend;
    public TMPro.TMP_Text dialogText;

    private void Update()
    {
        UpdateDialogBox();
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
}
