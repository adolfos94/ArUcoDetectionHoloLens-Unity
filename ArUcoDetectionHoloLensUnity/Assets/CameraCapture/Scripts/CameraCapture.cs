// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using static CameraCapture.CameraCapture;

namespace CameraCapture
{
    internal class CameraCapture : BasePlugin<CameraCapture>
    {
        public Int32 Width = 1280;
        public Int32 Height = 720;
        public Boolean EnableAudio = false;
        public Boolean EnableMrc = false;
        public Boolean EnabledPreview = false;
        public SpatialCameraTracker CameraTracker = null;

        public Renderer VideoRenderer = null;

        private Texture2D videoTexture = null;
        private CameraParameters cameraParameters;
        private IntPtr spatialCoordinateSystemPtr = IntPtr.Zero;

        private TaskCompletionSource<Wrapper.CaptureState> startPreviewCompletionSource = null;
        private TaskCompletionSource<Wrapper.CaptureState> stopCompletionSource = null;

        public delegate void OnCameraParameters(CameraParameters cameraParameters);

        public OnCameraParameters onCameraParameters;

        public delegate void OnProcessFrame(Matrix4x4 cameraToWorldMatrix);

        public OnProcessFrame onProcessFrame;

        public void CameraPreview()
        {
            if (EnabledPreview == false)
            {
                SetSpatialCoordinateSystem();
                StartPreview();
            }
            else StopPreview();
        }

        protected override void Awake()
        {
            base.Awake();

            UnityEngine.XR.WSA.WorldManager.OnPositionalLocatorStateChanged += (oldState, newState) =>
            {
                Debug.Log("WorldManager.OnPositionalLocatorStateChanged: " + newState + ", updating any capture in progress");

                SetSpatialCoordinateSystem();
            };
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                Application.RequestUserAuthorization(UserAuthorization.WebCam);
            }

            if (EnableAudio && Application.HasUserAuthorization(UserAuthorization.Microphone))
            {
                Application.RequestUserAuthorization(UserAuthorization.Microphone);
            }

            CreateCapture();

            if (VideoRenderer != null)
            {
                VideoRenderer.enabled = true;
            }
        }

        protected override void OnDisable()
        {
            startPreviewCompletionSource?.TrySetCanceled();
            stopCompletionSource?.TrySetCanceled();

            if (VideoRenderer != null)
            {
                VideoRenderer.material.SetTexture("_MainTex", null);
                VideoRenderer.enabled = false;
            }

            videoTexture = null;

            StopPreview();

            base.OnDisable();
        }

        protected override void OnCallback(Wrapper.CallbackType type, Wrapper.CallbackState args)
        {
            if (type == Wrapper.CallbackType.Capture)
            {
                switch (args.CaptureState.stateType)
                {
                    case Wrapper.CaptureStateType.PreviewStarted:
                        startPreviewCompletionSource?.TrySetResult(args.CaptureState);
                        break;

                    case Wrapper.CaptureStateType.PreviewStopped:
                        stopCompletionSource?.TrySetResult(args.CaptureState);
                        break;

                    case Wrapper.CaptureStateType.PreviewVideoFrame:
                        OnPreviewFrameChanged(args.CaptureState);
                        break;
                }
            }
        }

        protected override void OnFailed(Wrapper.FailedState args)
        {
            base.OnFailed(args);

            startPreviewCompletionSource?.TrySetCanceled();

            stopCompletionSource?.TrySetCanceled();
        }

        private void OnCompleteReadback(AsyncGPUReadbackRequest request)
        {
            if (request.hasError || !UnityEngine.WSA.Application.RunningOnAppThread())
            {
                Debug.Log("GPU readback error detected.");
                return;
            }

            onProcessFrame?.Invoke(CameraTracker.transformMatrix.Value);
        }

        private void OnPreviewFrameUpdated(Wrapper.CaptureState state)
        {
            if (!CameraTracker.UpdateCameraMatrices(state.cameraWorld, state.cameraProjection))
                return;

            AsyncGPUReadback.RequestIntoNativeArray(ref cameraParameters.data, videoTexture, 0, OnCompleteReadback);
            AsyncGPUReadback.WaitAllRequests();
        }

        protected void OnPreviewFrameChanged(Wrapper.CaptureState state)
        {
            var sizeChanged = false;

            if (videoTexture == null)
            {
                if (state.width != Width || state.height != Height)
                {
                    Debug.Log("Video texture does not match the size requested, using " + state.width + " x " + state.height);
                }

                videoTexture = Texture2D.CreateExternalTexture(state.width, state.height, TextureFormat.RGB24, false, false, state.imgTexture);

                // Create Resources for Proccessing
                cameraParameters.data = new NativeArray<byte>(state.width * state.height * 3, Allocator.Persistent);
                cameraParameters.resolution.width = state.width;
                cameraParameters.resolution.height = state.height;

                onCameraParameters?.Invoke(cameraParameters);

                sizeChanged = true;

                if (VideoRenderer != null)
                {
                    VideoRenderer.enabled = true;
                    VideoRenderer.sharedMaterial.SetTexture("_MainTex", videoTexture);
                    VideoRenderer.sharedMaterial.SetTextureScale("_MainTex", new Vector2(1, -1));
                }
            }
            else if (videoTexture.width != state.width || videoTexture.height != state.height)
            {
                Debug.Log("Video texture size changed, using " + state.width + " x " + state.height);

                videoTexture.UpdateExternalTexture(state.imgTexture);

                sizeChanged = true;
            }

            if (sizeChanged)
            {
                Debug.Log($"Size Changed = {state.width} x {state.height}");

                Width = state.width;
                Height = state.height;
            }

            OnPreviewFrameUpdated(state);
        }

        private void SetSpatialCoordinateSystem()
        {
            spatialCoordinateSystemPtr = UnityEngine.XR.WSA.WorldManager.GetNativeISpatialCoordinateSystemPtr();
            if (instanceId != Wrapper.InvalidHandle)
            {
                CheckHR(Native.SetCoordinateSystem(instanceId, spatialCoordinateSystemPtr));
            }
        }

        private void CreateCapture()
        {
            IntPtr thisObjectPtr = GCHandle.ToIntPtr(thisObject);
            CheckHR(Wrapper.CreateCapture(stateChangedCallback, thisObjectPtr, out instanceId));
        }

        public async void StartPreview()
        {
            await StartPreviewAsync(Width, Height, EnableAudio, EnableMrc);
        }

        public async void StopPreview()
        {
            if (!await StopPreviewAsync())
            {
                StopPreview();
            }
        }

        public async Task<bool> StartPreviewAsync(int width, int height, bool enableAudio, bool useMrc)
        {
            if (EnabledPreview)
                return true;

            startPreviewCompletionSource?.TrySetCanceled();

            var hr = Native.StartPreview(instanceId, (UInt32)width, (UInt32)height, enableAudio, useMrc);
            if (hr == 0)
            {
                EnabledPreview = true;
                startPreviewCompletionSource = new TaskCompletionSource<Wrapper.CaptureState>();

                try
                {
                    await startPreviewCompletionSource.Task;
                }
                catch (Exception ex)
                {
                    // task could have been cancelled
                    Debug.LogError(ex.Message);
                    hr = ex.HResult;
                }

                startPreviewCompletionSource = null;
            }
            else
            {
                await Task.Yield();
            }

            return (hr == 0);
        }

        public async Task<bool> StopPreviewAsync()
        {
            if (!EnabledPreview)
                return true;

            stopCompletionSource?.TrySetCanceled();

            var hr = Native.StopPreview(instanceId);
            if (hr == 0)
            {
                EnabledPreview = false;
                stopCompletionSource = new TaskCompletionSource<Wrapper.CaptureState>();

                try
                {
                    var state = await stopCompletionSource.Task;
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);

                    hr = ex.HResult;
                }
            }
            else
            {
                await Task.Yield();
            }

            stopCompletionSource = null;

            videoTexture = null;

            // Dispose Resources for Proccessing
            cameraParameters.resolution.width = 0;
            cameraParameters.resolution.height = 0;
            cameraParameters.data.Dispose();

            return CheckHR(hr) == 0;
        }

        private static class Native
        {
            [DllImport(Wrapper.ModuleName, CallingConvention = CallingConvention.StdCall, EntryPoint = "CaptureStartPreview")]
            internal static extern Int32 StartPreview(Int32 handle, UInt32 width, UInt32 height, [MarshalAs(UnmanagedType.I1)] Boolean enableAudio, [MarshalAs(UnmanagedType.I1)] Boolean enableMrc);

            [DllImport(Wrapper.ModuleName, CallingConvention = CallingConvention.StdCall, EntryPoint = "CaptureStopPreview")]
            internal static extern Int32 StopPreview(Int32 handle);

            [DllImport(Wrapper.ModuleName, CallingConvention = CallingConvention.StdCall, EntryPoint = "CaptureSetCoordinateSystem")]
            internal static extern Int32 SetCoordinateSystem(Int32 instanceId, IntPtr spatialCoordinateSystemPtr);
        }
    }
}