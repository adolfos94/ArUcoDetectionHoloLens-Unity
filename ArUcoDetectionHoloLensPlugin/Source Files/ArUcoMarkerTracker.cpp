#include "ArUcoMarkerTracker.h"

VOID ArUcoMarkerTracker::DetectArUcoMarkersInFrame(
	CONST IN CameraParameters& cameraParams,
	OUT DetectedArUcoMarker* detectedMarkers,
	IN INT numDetectObjects)
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

	// Iterate accross the detected markers.
	for (int i = 0; i < numDetectObjects; ++i)
	{
		for (size_t j = 0; j < markerIds.size(); j++)
		{
			// Match markers
			if (detectedMarkers[i].markerId != markerIds[j])
				continue;

			std::vector<std::vector<cv::Point2f>> marker = { markers[j] };

			// Vectors for pose (translation and rotation) estimation
			std::vector<cv::Vec3d> rVecs;
			std::vector<cv::Vec3d> tVecs;

			// Estimate pose of single marker
			cv::aruco::estimatePoseSingleMarkers(
				marker,
				detectedMarkers[i].markerSize,
				cameraParams.cameraMatrix,
				cameraParams.distCoeffs,
				rVecs,
				tVecs);

			detectedMarkers[i].tVecs[0] = (float)tVecs[0][0];
			detectedMarkers[i].tVecs[1] = (float)tVecs[0][1];
			detectedMarkers[i].tVecs[2] = (float)tVecs[0][2];

			detectedMarkers[i].rVecs[0] = (float)rVecs[0][0];
			detectedMarkers[i].rVecs[1] = (float)rVecs[0][1];
			detectedMarkers[i].rVecs[2] = (float)rVecs[0][2];

			detectedMarkers[i].tracked = true;
		}
	}
}

ArUcoMarkerTracker::ArUcoMarkerTracker(CONST IN INT dictId)
{
	// Create the aruco dictionary from id
	dictionary = cv::aruco::getPredefinedDictionary(dictId);

	// Create detector parameters
	detectorParams = cv::aruco::DetectorParameters::create();
}