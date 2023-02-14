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

	VOID DetectArUCoBoardInFrame(
		CONST IN CameraParameters& cameraParams,
		OUT DetectedArUcoBoard& detectedBoard);

private:

	// ArUco Dictionary.
	cv::Ptr<cv::aruco::Dictionary> dictionary;

	// Detector Parameters.
	cv::Ptr<cv::aruco::DetectorParameters> detectorParams;

	void CreateMatFrames(
		CONST IN CameraParameters& cameraParams,
		OUT cv::Mat& grayMat);
};
