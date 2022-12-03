using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCalibrationParams : MonoBehaviour
{
    public Vector2 focalLength;
    public Vector2 principalPoint;
    public Vector3 radialDistortion;
    public Vector2 tangentialDistortion;
    public int imageWidth;
    public int imageHeight;

    public float[] GetCameraMatrix()
    {
        float[] cameraMatrix = new float[4];

        cameraMatrix[0] = focalLength.x;
        cameraMatrix[1] = focalLength.y;
        cameraMatrix[2] = principalPoint.x;
        cameraMatrix[3] = principalPoint.y;

        return cameraMatrix;
    }

    public float[] GetDistCoeff()
    {
        float[] distCoeff = new float[5];

        distCoeff[0] = radialDistortion.x;
        distCoeff[1] = radialDistortion.y;
        distCoeff[2] = radialDistortion.z;
        distCoeff[3] = tangentialDistortion.x;
        distCoeff[4] = tangentialDistortion.y;

        return distCoeff;
    }
}