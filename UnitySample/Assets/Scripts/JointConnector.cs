using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointConnector : MonoBehaviour
{
    public GameObject jointA;
    public GameObject jointB;

    private LineRenderer _lineRenderer;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    private void Start()
    {
        ConfigureLine();
    }

    private void Update()
    {
        UpdateLine();
    }

    private void UpdateLine()
    {
        _lineRenderer.SetPosition(0, jointA.transform.position);
        _lineRenderer.SetPosition(1, jointB.transform.position);
    }

    private void ConfigureLine()
    {
        _lineRenderer.positionCount = 2;
        UpdateLine();
    }
}