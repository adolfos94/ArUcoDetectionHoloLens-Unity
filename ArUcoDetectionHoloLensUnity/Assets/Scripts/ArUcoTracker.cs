using ArUcoDetectionHoloLensUnity;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class ArUcoTracker : MonoBehaviour
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DetectedArUcoMarker
    {
        public byte tracked;
        public int markerId;
        public float markerSize;
        public Vector3 tVecs;
        public Vector3 rVecs;
    }

    /// <summary>
    /// ArUco Dictionary Name to get tracked.
    /// </summary>
    public CvUtils.ArUcoDictionaryName arUcoDictionaryName;

    /// <summary>
    /// Camera parameters (intrinsics and extrinsics) of the tracking sensor on the HoloLens 2.
    /// </summary>
    public CameraCalibrationParams calibParams;

    /// <summary>
    /// List of prefab instances of detected aruco markers.
    /// </summary>
    public ArUcoMarker[] trackedObjects = new ArUcoMarker[1];

    private CameraCapture.CameraCapture cameraCapture;
    private SpatialCameraTracker spatialCameraTracker;

    private void Start()
    {
        cameraCapture = GetComponent<CameraCapture.CameraCapture>();
        spatialCameraTracker = GetComponent<SpatialCameraTracker>();

        cameraCapture.CameraTracker = spatialCameraTracker;
        cameraCapture.onCameraParameters = OnCameraParameters;
        cameraCapture.onProcessFrame = OnProcessFrame;

        cameraCapture.CameraPreview();
    }

    private void OnCameraParameters(CameraParameters cameraParameters)
    {
        ArUcoTrackerWrapper.SetCameraParameters(cameraParameters, calibParams);
        ArUcoTrackerWrapper.StartArUcoMarkerTracker((int)arUcoDictionaryName);
    }

    private void OnProcessFrame(Matrix4x4 cameraToWorldMatrix)
    {
        DetectedArUcoMarker[] detectedObjects = new DetectedArUcoMarker[trackedObjects.Length];

        for (int i = 0; i < trackedObjects.Length; ++i)
        {
            detectedObjects[i].markerId = trackedObjects[i].id;
            detectedObjects[i].markerSize = trackedObjects[i].markerSize;

            // Fill mesh?
        }

        ArUcoTrackerWrapper.DetectArUcoMarkers(detectedObjects, trackedObjects.Length);

        for (int i = 0; i < trackedObjects.Length; ++i)
        {
            if (detectedObjects[i].tracked == 0x0)
                continue;

            TransformMarkerToWorldCoordiantes(detectedObjects[i], trackedObjects[i], cameraToWorldMatrix);
        }
    }

    private void TransformMarkerToWorldCoordiantes(
        DetectedArUcoMarker detected,
        ArUcoMarker tracked,
        Matrix4x4 cameraToWorldMatrix)
    {
        // Get pose from OpenCV and format for Unity
        Vector3 position = detected.tVecs;
        position.y *= -1f;
        Quaternion rotation = CvUtils.RotationQuatFromRodrigues(detected.rVecs);
        Matrix4x4 transformUnityCamera = CvUtils.TransformInUnitySpace(position, rotation);

        // Use camera to world transform to get world pose of marker
        Matrix4x4 transformUnityWorld = cameraToWorldMatrix * transformUnityCamera;

        // Apply updated transform to gameobject in world
        tracked.markerGo.transform.SetPositionAndRotation(
            CvUtils.GetVectorFromMatrix(transformUnityWorld),
            CvUtils.GetQuatFromMatrix(transformUnityWorld));
    }
}