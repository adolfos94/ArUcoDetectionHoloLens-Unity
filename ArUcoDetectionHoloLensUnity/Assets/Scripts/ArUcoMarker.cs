﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ArUcoMarker
{
    [Tooltip("Is World Anchored?")]
    public bool isWorldAnchored;

    [Tooltip("Marker ID")]
    public int id;

    [Tooltip("Game object for to use for marker instantiation")]
    public GameObject markerGo;
}