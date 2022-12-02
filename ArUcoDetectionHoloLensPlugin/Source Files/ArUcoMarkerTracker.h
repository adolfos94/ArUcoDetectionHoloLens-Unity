#pragma once

#include "pch.h"

class ArUcoMarkerTracker
{
public:

	ArUcoMarkerTracker() {};
	ArUcoMarkerTracker(CONST IN INT dictId);

	VOID DetectArUcoMarkersInFrame(
		CONST IN CameraParameters& cameraParams,
		OUT DetectedArUcoMarker* detectedMarkers,
		IN INT numDetectObjects);

	VOID RefineArUcoMarkerTracker(
		CONST IN CameraParameters& cameraParams,
		CONST IN FLOAT* vertexes, CONST IN INT nVertexes,
		CONST IN INT* triangles, CONST IN INT nTriangles,
		OUT DetectedArUcoMarker& detectedMarker);

private:

	// ArUco Dictionary.
	cv::Ptr<cv::aruco::Dictionary> dictionary;

	// Detector Parameters.
	cv::Ptr<cv::aruco::DetectorParameters> detectorParams;
};
