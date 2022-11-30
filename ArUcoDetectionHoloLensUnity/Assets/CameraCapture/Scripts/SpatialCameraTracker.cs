using System;
using UnityEngine;

using Spatial4x4 = SpatialTranformHelper.Matrix4x4;

public class SpatialCameraTracker : MonoBehaviour
{
    private Spatial4x4 cameraTransform = Spatial4x4.Zero;
    private Spatial4x4 cameraProjection = Spatial4x4.Zero;

    public delegate void OnSpatialFrameUpdated(Matrix4x4 projectionMatrix, Matrix4x4 cameraToWorldMatrix);

    public OnSpatialFrameUpdated onSpatialFrameUpdated;

    private void Awake()
    {
        if (transform.parent != null)
        {
            Debug.LogError("This gameObject should be on the root of the scene.");

            return;
        }
    }

    // store the matrix values, any updates will happen on the update loop
    public bool UpdateCameraMatrices(Spatial4x4 transform, Spatial4x4 projection)
    {
        // store matrix information from the sample

        cameraTransform = transform;
        cameraProjection = projection;

        Matrix4x4? transformMatrix = null;
        if (cameraTransform != Spatial4x4.Zero)
            transformMatrix = cameraTransform.ToUnityTransform();

        UnityEngine.Matrix4x4? projectionMatrix = null;
        if (cameraProjection != Spatial4x4.Zero)
            projectionMatrix = cameraProjection.ToUnity();

        if (transformMatrix == null || projectionMatrix == null)
        {
            projectionMatrix = Camera.main.projectionMatrix;
            transformMatrix = Camera.main.cameraToWorldMatrix;
        }

        // Send Matrixes for Proccesing
        onSpatialFrameUpdated?.Invoke(projectionMatrix.Value, transformMatrix.Value);

        return transformMatrix.Value.ValidTRS();
    }

    // https://docs.microsoft.com/en-us/windows/mixed-reality/locatable-camera
    // Video        Preview     Still       Horizontal Field of View(H-FOV)     Suggested usage
    // V1:1280x720  1280x720    1280x720    45deg                               (default mode)
    // V2:2272x1278 2272x1278   3904x2196   64.69                               (legacy)
}