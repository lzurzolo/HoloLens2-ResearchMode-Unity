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
    private PhotoCapture _photoCapture;
    private bool _readyToCaptureFrames;
    public GameObject skeleton;
    public List<GameObject> joints;
    public Camera camera;
    private const int IMAGE_WIDTH = 1280;
    private const int IMAGE_HEIGHT = 720;
    public TMPro.TMP_Text debugText;
    public GameObject debugSphere;
    private IEnumerator _processLandmarks;
    private IEnumerator _publishPhotos;
    private ConcurrentQueue<List<byte>> _photosToBeProcessed;

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

    public List<Landmark> currentLandmarks;

    private void Awake()
    {
        //_readyToCaptureFrames = false;
        currentLandmarks = new List<Landmark>();
        _processLandmarks = ProcessLandmarkQueue();
        _publishPhotos = PublishPhoto();
        _photosToBeProcessed = new ConcurrentQueue<List<byte>>();
    }
    private void Start()
    {
        //PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
#if WINDOWS_UWP
        sendBytesToServer.StartConnection();
#endif
        camera = GetComponent<Camera>();
        StartCoroutine(_processLandmarks);
        StartCoroutine(_publishPhotos);
    }

    private void Update()
    {
        //if (!_readyToCaptureFrames) return;
        //_photoCapture.TakePhotoAsync(OnCapturedPhotoToMemory);
    }

    private IEnumerator PublishPhoto()
    {
        while(true)
        {
            yield return null;
            //if(_photosToBeProcessed.TryDequeue(out List<byte> photo))
            //{
            //var bytes = photo.ToArray();
            //int size = bytes.Length;
#if WINDOWS_UWP
            //sendBytesToServer.Publish(dummyBytes.Length,dummyBytes);
            Task task = sendBytesToServer.GetLandmarksFromServer();
            yield return new WaitUntil(() => task.IsCompleted);
#endif
            //}
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

                        var cameraSpacePos = UnProjectVector(camera.projectionMatrix, new Vector3(imagePosProjected.x, imagePosProjected.y, 1));

                        var unityCamToWorld = camera.cameraToWorldMatrix;
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

    #region "Camera Callbacks"
    void OnPhotoCaptureCreated(PhotoCapture captureObject)
    {
        _photoCapture = captureObject;

        /// For whatever reason the Hololens 2 RGB camera does not report back the correctly supported resolutions.
        /// They are:
        /// 3904x2196
        /// 1920x1080
        /// 1280x720
        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

        CameraParameters c = new CameraParameters();
        c.hologramOpacity = 0.0f;
        c.cameraResolutionWidth = IMAGE_WIDTH;
        c.cameraResolutionHeight = IMAGE_HEIGHT;
        c.pixelFormat = CapturePixelFormat.PNG;
        
        captureObject.StartPhotoModeAsync(c, OnPhotoModeStarted);
    }

    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            _readyToCaptureFrames = true;
        }
        else
        {
            Debug.LogError("Unable to start photo mode!");
        }
    }

    async void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        if (result.success)
        {
            List<byte> imageBufferList = new List<byte>();
            photoCaptureFrame.CopyRawImageDataIntoBuffer(imageBufferList);
            _photosToBeProcessed.Enqueue(imageBufferList);
        }
    }

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        _photoCapture.Dispose();
        _photoCapture = null;
    }
    #endregion

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