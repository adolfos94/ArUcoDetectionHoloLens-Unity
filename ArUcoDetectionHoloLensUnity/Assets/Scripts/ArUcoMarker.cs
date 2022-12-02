using System;
using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout(LayoutKind.Sequential)]
public struct DetectedArUcoMarker
{
    public byte tracked;
    public int markerId;
    public float markerSize;
    public Vector3 tVecs;
    public Vector3 rVecs;
}

[Serializable]
public class ArUcoMarker : MonoBehaviour
{
    [Tooltip("Is World Anchored?")]
    public bool isWorldAnchored;

    [Tooltip("Marker ID")]
    public int id;

    [Tooltip("Size of the marker in meters.")]
    public float markerSize;

    [Tooltip("Game object for to use for marker instantiation")]
    public GameObject markerGo;
}