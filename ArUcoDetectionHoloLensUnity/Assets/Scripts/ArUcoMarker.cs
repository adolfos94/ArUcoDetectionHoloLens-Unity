using System;
using UnityEngine;

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