using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.WebCam;
using System.Threading.Tasks;
using System.Net;

public class PVCameraCapture : MonoBehaviour
{
    public SendBytesToServer sendBytesToServer;
    public List<GameObject> joints;
    public Camera _camera;
    private IEnumerator _processLandmarks;
    private IEnumerator _getLandmarks;

    [System.Serializable]
    public class Landmarks
    {
        public Landmark[] landmarks;
    }

    [System.Serializable]
    public class Landmark
    {
        public string position;
        public float x;
        public float y;
        public float z;
    }

    private void Awake()
    {
        _processLandmarks = ProcessLandmarkQueue();
        _getLandmarks = GetLandmarks();
    }
    private void Start()
    {
#if WINDOWS_UWP
        sendBytesToServer.StartConnection();
#endif
        _camera = GetComponent<Camera>();
        StartCoroutine(_processLandmarks);
        StartCoroutine(_getLandmarks);
    }

    private IEnumerator GetLandmarks()
    {
        while(true)
        {
            yield return null;
#if WINDOWS_UWP
            Task task = sendBytesToServer.GetLandmarksFromServer();
            yield return new WaitUntil(() => task.IsCompleted);
#endif
        }
        yield return null;
    }

    private IEnumerator ProcessLandmarkQueue()
    {
        while(true)
        {
            yield return null;
            if (sendBytesToServer.outJsons.TryDequeue(out string jsonStr))
            {
                if (jsonStr != string.Empty)
                {
                    string newJson = "{ \"landmarks\": " + jsonStr + "}";
                    var landmarks = JsonUtility.FromJson<Landmarks>(newJson);
                    for (int i = 0; i < landmarks.landmarks.Length; i++)
                    {
                        var landmark = landmarks.landmarks[i];
                        var imagePos = new Vector2(landmark.x, landmark.y);

                        Vector2 imagePosProjected = new Vector2(2 * imagePos.x - 1, 1 - 2 * imagePos.y);

                        var cameraSpacePos = UnProjectVector(_camera.projectionMatrix, new Vector3(imagePosProjected.x, imagePosProjected.y, 1));

                        var unityCamToWorld = _camera.cameraToWorldMatrix;
                        var worldSpaceBoxPos = unityCamToWorld.MultiplyPoint(cameraSpacePos);

                        joints[i].transform.position = worldSpaceBoxPos;
                    }
                }
                else
                {
                    continue;
                }
            }
        }

        yield return null;
    }

    private static Vector3 UnProjectVector(Matrix4x4 proj, Vector3 to)
    {
        Vector3 from = new Vector3(0, 0, 0);
        var axsX = proj.GetRow(0);
        var axsY = proj.GetRow(1);
        var axsZ = proj.GetRow(2);
        from.z = to.z / axsZ.z;
        from.y = (to.y - (from.z * axsY.z)) / axsY.y;
        from.x = (to.x - (from.z * axsX.z)) / axsX.x;
        return from;
    }
}