using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class ArUcoTrackerWrapper
{
    public static void DetectArUcoMarkers(CameraParameters cameraParameters)
    {
        unsafe
        {
            DetectArUcoMarkers(cameraParameters.data.GetUnsafePtr<byte>(), cameraParameters.resolution);
        }
    }

    [DllImport("ArUcoDetectionPlugin", CallingConvention = CallingConvention.StdCall)]
    public static extern void StartArUcoMarkerTracker(float markerSize, int dictId);

    [DllImport("ArUcoDetectionPlugin", CallingConvention = CallingConvention.StdCall)]
    private static extern unsafe void DetectArUcoMarkers(void* dataPtr, Resolution resolution);
}