#include "Plugin.h"
#include "ArUcoMarkerTracker.h"

ArUcoMarkerTracker arUcoMarkertracker;

VOID INTERFACE_API StartArUcoMarkerTracker(
	CONST IN FLOAT markerSize,
	CONST IN INT dictId)
{
	arUcoMarkertracker = ArUcoMarkerTracker(markerSize, dictId);
}

VOID INTERFACE_API DetectArUcoMarkers(
	void* dataPtr,
	Resolution resolution)
{
	CameraParameters cameraParameters;
	cameraParameters.data = (byte*)dataPtr;
	cameraParameters.resolution = resolution;

	arUcoMarkertracker.DetectArUcoMarkersInFrame(cameraParameters);
}