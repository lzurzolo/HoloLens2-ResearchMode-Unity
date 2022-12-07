using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartingMarker : MonoBehaviour
{
    public LineRenderer forwardMarker;
    public LineRenderer rightMarker;
    public LineRenderer upMarker;

    private void Start()
    {
        forwardMarker.SetPosition(0, transform.position);
        forwardMarker.SetPosition(1, transform.position + transform.forward);

        rightMarker.SetPosition(0, transform.position);
        rightMarker.SetPosition(1, transform.position + transform.right);

        upMarker.SetPosition(0, transform.position);
        upMarker.SetPosition(1, transform.position + transform.up);
    }
}
