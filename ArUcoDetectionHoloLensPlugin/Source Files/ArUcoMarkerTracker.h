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
		IN INT numDetectObjects,
		OUT cv::Mat& debugMat);

	VOID DetectArUCoBoardInFrame(
		CONST IN CameraParameters& cameraParams,
		OUT DetectedArUcoBoard& detectedBoard,
		OUT cv::Mat& debugMat);

private:

	// ArUco Dictionary.
	cv::Ptr<cv::aruco::Dictionary> dictionary;

	// Detector Parameters.
	cv::Ptr<cv::aruco::DetectorParameters> detectorParams;

	void CreateMatFrames(
		CONST IN CameraParameters& cameraParams,
		OUT cv::Mat& grayMat, OUT cv::Mat& debugMat);
};
