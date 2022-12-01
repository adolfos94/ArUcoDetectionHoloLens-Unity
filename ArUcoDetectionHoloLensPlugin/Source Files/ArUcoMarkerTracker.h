#pragma once

#include "pch.h"

class ArUcoMarkerTracker
{
public:

	ArUcoMarkerTracker() {};
	ArUcoMarkerTracker(CONST IN FLOAT markerSize, CONST IN INT dictId);

	VOID DetectArUcoMarkersInFrame(
		CONST IN CameraParameters& cameraParams,
		OUT DetectedArUcoMarker* detectedMarkers);

private:

	float markerSize;
	int dictId;
};
