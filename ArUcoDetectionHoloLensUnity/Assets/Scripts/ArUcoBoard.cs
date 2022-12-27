using System;
using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout(LayoutKind.Sequential)]
public struct DetectedArUcoBoard
{
    public byte tracked;
    public int markersX;
    public int markersY;
    public int markerId;
    public float markerSize;
    public float markerSeparation;
    public Vector3 tVec;
    public Vector3 rVec;
}

[Serializable]
public class ArUcoBoard : MonoBehaviour
{
    [Tooltip("Is World Anchored?")]
    public bool isWorldAnchored;

    [Tooltip("Number of markers in X direction.")]
    public int markersX;

    [Tooltip("Number of markers in Y direction.")]
    public int markersY;

    [Tooltip("First markerId of board to be used.")]
    public int markerId;

    [Tooltip("Size of the markers in meters.")]
    public float markerSize;

    [Tooltip("Separation between two markers in meters.")]
    public float markerSeparation;

    [Tooltip("Game object for to use for marker instantiation.")]
    public GameObject markerGo;
}