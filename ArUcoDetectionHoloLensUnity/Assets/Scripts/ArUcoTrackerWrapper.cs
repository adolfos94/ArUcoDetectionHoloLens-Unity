using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class ArUcoTrackerWrapper
{
    public static void SetCameraParameters(CameraParameters cameraParameters, CameraCalibrationParams cameraCalibrationParams)
    {
        unsafe
        {
            SetCameraParameters(
                cameraParameters.videoVerticallyMirrored,
                cameraParameters.resolution,
                cameraParameters.data.GetUnsafePtr<byte>(),
                cameraCalibrationParams.GetCameraMatrix(),
                cameraCalibrationParams.GetDistCoeff());
        }
    }

    [DllImport("ArUcoDetectionPlugin", CallingConvention = CallingConvention.StdCall)]
    private static extern unsafe void SetCameraParameters(
        byte videoVerticallyMirrored,
        Resolution resolution,
        void* dataPtr,
        float[] cameraMatrix,
        float[] distCoeff);

    [DllImport("ArUcoDetectionPlugin", CallingConvention = CallingConvention.StdCall)]
    public static extern void StartArUcoMarkerTracker(int dictId);

    [DllImport("ArUcoDetectionPlugin", CallingConvention = CallingConvention.StdCall)]
    public static extern void DetectArUcoMarkers(DetectedArUcoMarker[] detectedArUcoMarkers, int numDetectObjects);

    [DllImport("ArUcoDetectionPlugin", CallingConvention = CallingConvention.StdCall)]
    public static extern void DetectArUcoBoard(ref DetectedArUcoBoard detectedArUcoMarkers);
}