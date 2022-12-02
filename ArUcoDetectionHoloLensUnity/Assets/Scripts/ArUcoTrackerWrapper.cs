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
                cameraParameters.resolution,
                cameraParameters.data.GetUnsafePtr<byte>(),
                cameraCalibrationParams.GetCameraMatrix(),
                cameraCalibrationParams.GetDistCoeff());
        }
    }

    [DllImport("ArUcoDetectionPlugin", CallingConvention = CallingConvention.StdCall)]
    private static extern unsafe void SetCameraParameters(
        Resolution resolution,
        void* dataPtr,
        float[] cameraMatrix,
        float[] distCoeff);

    [DllImport("ArUcoDetectionPlugin", CallingConvention = CallingConvention.StdCall)]
    public static extern void StartArUcoMarkerTracker(int dictId);

    [DllImport("ArUcoDetectionPlugin", CallingConvention = CallingConvention.StdCall)]
    public static extern void DetectArUcoMarkers(DetectedArUcoMarker[] detectedArUcoMarkers, int numDetectObjects);

    [DllImport("ArUcoDetectionPlugin", CallingConvention = CallingConvention.StdCall)]
    public static extern void RefineArUcoMarkerTracker(Vector3[] vertexes, int nVertexes, int[] triangles, int nTriangles, ref DetectedArUcoMarker detectedArUcoMarker);
}