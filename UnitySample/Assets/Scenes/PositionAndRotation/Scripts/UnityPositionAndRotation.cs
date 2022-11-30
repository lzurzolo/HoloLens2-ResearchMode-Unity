using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityPositionAndRotation : MonoBehaviour
{
    public GameObject startingMarker;

    [HideInInspector]
    public Quaternion currentRotation;
    [HideInInspector]
    public Vector3 currentEulerAngles;
    [HideInInspector]
    public Vector3 currentPosition;

    private Camera _camera;

    private void Start()
    {
        _camera = Camera.main;
        //Instantiate(startingMarker, _camera.transform.position, _camera.transform.rotation);
    }

    private void Update()
    {
        //var transform = _camera.transform;

        //currentRotation = transform.rotation;
        //currentEulerAngles = currentRotation.eulerAngles;
        //currentPosition = transform.position;
    }
}
