#pragma once

#include "pch.h"

#define STRINGIFY(VAR) #VAR << ": " << VAR << " "
#define API_EXPORT __declspec(dllexport)
#define INTERFACE_API __stdcall
#define EXTERN extern "C"

EXTERN VOID API_EXPORT INTERFACE_API SetCameraParameters(
	Resolution resolution,
	void* dataPtr,
	float* cameraMatrix,
	float* distCoeff);

EXTERN VOID API_EXPORT INTERFACE_API StartArUcoMarkerTracker(CONST IN INT dictId);

EXTERN VOID API_EXPORT INTERFACE_API DetectArUcoMarkers(OUT DetectedArUcoMarker* detectedMarkers, IN INT numDetectObjects);