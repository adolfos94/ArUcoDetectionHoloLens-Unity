using Unity.Collections;
using UnityEngine;

public struct CameraParameters
{
    public byte videoVerticallyMirrored;
    public NativeArray<byte> data;
    public Resolution resolution;
}