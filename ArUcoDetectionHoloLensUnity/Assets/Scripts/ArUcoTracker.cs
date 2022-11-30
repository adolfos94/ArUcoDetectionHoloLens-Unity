using CameraCapture;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArUcoTracker : MonoBehaviour
{
    // Start is called before the first frame update

    private CameraCapture.CameraCapture cameraCapture;
    private SpatialCameraTracker spatialCameraTracker;

    private void Start()
    {
        cameraCapture = GetComponent<CameraCapture.CameraCapture>();
        spatialCameraTracker = GetComponent<SpatialCameraTracker>();

        cameraCapture.CameraTracker = spatialCameraTracker;
        cameraCapture.OnCameraPreview();
    }

    // Update is called once per frame
    private void Update()
    {
    }
}