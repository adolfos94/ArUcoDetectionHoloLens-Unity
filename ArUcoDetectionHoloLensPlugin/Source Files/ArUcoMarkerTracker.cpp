#include "ArUcoMarkerTracker.h"

VOID ArUcoMarkerTracker::DetectArUcoMarkersInFrame(
	CONST IN CameraParameters& cameraParams,
	OUT DetectedArUcoMarker* detectedMarkers)
{
	if (!cameraParams.data)
		return;

	// Create cv::Mat from sensor frame
	cv::Mat wrappedMat = cv::Mat(
		cameraParams.resolution.height,
		cameraParams.resolution.width,
		CV_8UC3, cameraParams.data);

	// Convert cv::Mat to grayscale for detection
	cv::Mat grayMat;
	cv::cvtColor(wrappedMat, grayMat, cv::COLOR_RGB2GRAY);

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

	// Vectors for pose (translation and rotation) estimation
	std::vector<cv::Vec3d> rVecs;
	std::vector<cv::Vec3d> tVecs;

	// Estimate pose of single markers
	cv::aruco::estimatePoseSingleMarkers(
		markers,
		markerSize,
		cameraParams.cameraMatrix,
		cameraParams.distCoeffs,
		rVecs,
		tVecs);

	// Iterate across the detected markers
	for (size_t i = 0; i < markerIds.size(); i++)
	{
		// Add the marker
		detectedMarkers[i].id = markerIds[i];

		detectedMarkers[i].tVecs[0] = (float)tVecs[i][0];
		detectedMarkers[i].tVecs[1] = (float)tVecs[i][1];
		detectedMarkers[i].tVecs[2] = (float)tVecs[i][2];

		detectedMarkers[i].rVecs[0] = (float)rVecs[i][0];
		detectedMarkers[i].rVecs[1] = (float)rVecs[i][1];
		detectedMarkers[i].rVecs[2] = (float)rVecs[i][2];
	}
}

ArUcoMarkerTracker::ArUcoMarkerTracker(CONST IN FLOAT markerSize, CONST IN INT dictId) : markerSize(markerSize)
{
	// Create the aruco dictionary from id
	dictionary = cv::aruco::getPredefinedDictionary(dictId);

	// Create detector parameters
	detectorParams = cv::aruco::DetectorParameters::create();
}