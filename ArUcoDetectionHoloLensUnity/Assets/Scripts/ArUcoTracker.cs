using ArUcoDetectionHoloLensUnity;
using CameraCapture;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArUcoTracker : MonoBehaviour
{
    public struct DetectedArUcoMarker
    {
        public int id;
        public Vector3 tVecs;
        public Vector3 rVecs;
    }

    /// <summary>
    /// Size of the marker in meters.
    /// </summary>
    public float markerSize;

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
        cameraCapture.onCameraParameters = onCameraParameters;
        cameraCapture.onProcessFrame = OnProcessFrame;

        cameraCapture.CameraPreview();
    }

    private void onCameraParameters(CameraParameters cameraParameters)
    {
        ArUcoTrackerWrapper.SetCameraParameters(cameraParameters, calibParams);
        ArUcoTrackerWrapper.StartArUcoMarkerTracker(markerSize, (int)arUcoDictionaryName);
    }

    private void OnProcessFrame(Matrix4x4 cameraToWorldMatrix)
    {
        DetectedArUcoMarker[] detectedObjects = new DetectedArUcoMarker[5];
        ArUcoTrackerWrapper.DetectArUcoMarkers(detectedObjects);

        foreach (var detected in detectedObjects)
        {
            if (detected.id == 0)
                continue;

            foreach (var tracked in trackedObjects)
            {
                if (tracked.id != detected.id)
                    continue;

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
    }
}