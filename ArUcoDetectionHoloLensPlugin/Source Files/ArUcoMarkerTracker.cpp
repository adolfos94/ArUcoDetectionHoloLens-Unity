#include "ArUcoMarkerTracker.h"

VOID ArUcoMarkerTracker::DetectArUcoMarkersInFrame(CONST IN CameraParameters& cameraParams)
{
	if (!cameraParams.data)
		return;

	// Create the aruco dictionary from id
	cv::Ptr<cv::aruco::Dictionary> dictionary =
		cv::aruco::getPredefinedDictionary(dictId);

	// Create detector parameters
	cv::Ptr<cv::aruco::DetectorParameters> detectorParams
		= cv::aruco::DetectorParameters::create();

	// Create cv::Mat from sensor frame
	cv::Mat wrappedMat = cv::Mat(
		cameraParams.resolution.width,
		cameraParams.resolution.height,
		CV_8UC4, cameraParams.data);

	// Convert cv::Mat to grayscale for detection
	cv::Mat grayMat;
	cv::cvtColor(wrappedMat, grayMat, cv::COLOR_BGRA2GRAY);

	// Detect markers
	std::vector<int32_t> markerIds;
	std::vector<std::vector<cv::Point2f>> markers, rejectedCandidates;

	cv::aruco::detectMarkers(
		grayMat,
		dictionary,
		markers,
		markerIds,
		detectorParams,
		rejectedCandidates);

	if (markerIds.empty())
		return;

	cv::aruco::drawDetectedMarkers(wrappedMat, markers, markerIds);
}

ArUcoMarkerTracker::ArUcoMarkerTracker(CONST IN FLOAT markerSize, CONST IN INT dictId)
{
	this->markerSize = markerSize;
	this->dictId = dictId;
}