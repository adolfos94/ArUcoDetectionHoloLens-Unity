using ArUcoDetectionHoloLensUnity;
using UnityEngine;

public class ArUcoBoardTracker : MonoBehaviour
{
    /// <summary>
    /// ArUco Dictionary Name to get tracked.
    /// </summary>
    public CvUtils.ArUcoDictionaryName arUcoDictionaryName;

    /// <summary>
    /// Camera parameters (intrinsics and extrinsics) of the tracking sensor on the HoloLens 2.
    /// </summary>
    public CameraCalibrationParams calibParams;

    /// <summary>
    /// Prefab instances of detected board.
    /// </summary>
    public ArUcoBoard trackedBoard;

    private CameraCapture.CameraCapture cameraCapture;
    private SpatialCameraTracker spatialCameraTracker;

    private string cameraName = "CAMERA_H2";

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
        ArUcoTrackerWrapper.SetCameraParameters(cameraParameters, calibParams, cameraName);
        ArUcoTrackerWrapper.StartArUcoMarkerTracker((int)arUcoDictionaryName, cameraName);
    }

    private void OnProcessFrame(Matrix4x4 cameraToWorldMatrix)
    {
        DetectedArUcoBoard detectedBoard = new DetectedArUcoBoard();
        detectedBoard.markersX = trackedBoard.markersX;
        detectedBoard.markersY = trackedBoard.markersY;
        detectedBoard.markerId = trackedBoard.markerId;
        detectedBoard.markerSize = trackedBoard.markerSize;
        detectedBoard.markerSeparation = trackedBoard.markerSeparation;

        ArUcoTrackerWrapper.DetectArUcoBoard(ref detectedBoard, cameraName);

        if (detectedBoard.tracked == 0x0)
            return;

        TransformBoardToWorldCoordiantes(trackedBoard, detectedBoard, cameraToWorldMatrix);
    }

    private void TransformBoardToWorldCoordiantes(
        ArUcoBoard tracked,
        DetectedArUcoBoard detected,
        Matrix4x4 cameraToWorldMatrix)
    {
        // Get pose from OpenCV and format for Unity
        Vector3 position = detected.tVec; position.y *= -1f;
        Quaternion rotation = CvUtils.RotationQuatFromRodrigues(detected.rVec);
        Matrix4x4 transformUnityCamera = CvUtils.TransformInUnitySpace(position, rotation);

        // Use camera to world transform to get world pose of marker
        Matrix4x4 transformUnityWorld = cameraToWorldMatrix * transformUnityCamera;
        transformUnityWorld *= Matrix4x4.Scale(new Vector3(1, -1, -1));

        // Apply updated transform to gameobject in world
        tracked.markerGo.transform.SetPositionAndRotation(
            CvUtils.GetVectorFromMatrix(transformUnityWorld),
            CvUtils.GetQuatFromMatrix(transformUnityWorld));

        // Apply relative position between marker and pivot
        tracked.markerGo.transform.Translate(tracked.transform.position);
        tracked.markerGo.transform.Rotate(tracked.transform.rotation.eulerAngles);
    }
}