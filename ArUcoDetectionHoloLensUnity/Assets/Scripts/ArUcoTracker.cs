using ArUcoDetectionHoloLensUnity;
using CameraCapture;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArUcoTracker : MonoBehaviour
{
    public float markerSize;
    public CvUtils.ArUcoDictionaryName arUcoDictionaryName;

    private CameraCapture.CameraCapture cameraCapture;
    private SpatialCameraTracker spatialCameraTracker;

    private void Start()
    {
        cameraCapture = GetComponent<CameraCapture.CameraCapture>();
        spatialCameraTracker = GetComponent<SpatialCameraTracker>();

        cameraCapture.CameraTracker = spatialCameraTracker;
        cameraCapture.onProcessFrame = OnProcessFrame;
        cameraCapture.OnCameraPreview();

        ArUcoTrackerWrapper.StartArUcoMarkerTracker(markerSize, (int)arUcoDictionaryName);
    }

    private void OnProcessFrame(CameraParameters cameraParameters)
    {
        ArUcoTrackerWrapper.DetectArUcoMarkers(cameraParameters);
    }
}