#include "ArUcoMarkerTracker.h"

std::vector<cv::Vec3f> Pts3d = { { -0.5, 0.5, 0 }, {0.5, 0.5, 0}, {0.5, -0.5, 0}, {-0.5, -0.5, 0} };
std::vector<cv::Vec3i> Tri3d = { { 0, 2, 1}, {0, 3, 2} };

void ArUcoMarkerTracker::CreateMatFrames(
	CONST IN CameraParameters& cameraParams,
	OUT cv::Mat& grayMat, OUT cv::Mat& debugMat)
{
	// Create cv::Mat from sensor frame
	cv::Mat wrappedMat = cv::Mat(
		cameraParams.resolution.height,
		cameraParams.resolution.width,
		CV_8UC3, cameraParams.data);

	cv::Mat sensorMat;
	if (cameraParams.videoVerticallyMirrored)
		cv::flip(wrappedMat, sensorMat, 0);
	else
		sensorMat = wrappedMat.clone();

	// Convert cv::Mat to grayscale for detection
	cv::cvtColor(sensorMat, grayMat, cv::COLOR_RGB2GRAY);

#ifndef UWP

	cv::cvtColor(sensorMat, debugMat, cv::COLOR_RGB2BGR);

#endif // !UWP
}

VOID ArUcoMarkerTracker::DetectArUcoMarkersInFrame(
	CONST IN CameraParameters& cameraParams,
	OUT DetectedArUcoMarker* detectedMarkers,
	IN INT numDetectObjects,
	OUT cv::Mat& debugMat)
{
	if (!cameraParams.data)
		return;

	cv::Mat grayMat;
	CreateMatFrames(cameraParams, grayMat, debugMat);

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

			std::vector<cv::Vec3f> pts3d(Pts3d.size());
			std::transform(Pts3d.begin(), Pts3d.end(), pts3d.begin(), [&](cv::Vec3f pt3d)
				{
					return pt3d * detectedMarkers[i].markerSize;
				});

			// Tracking and refinement with rapid
			bool tracked = true;
			for (int k = 0; k < 5; ++k)
			{
				auto ratio = cv::rapid::rapid(grayMat, 50, 10, pts3d, Tri3d,
					cameraParams.cameraMatrix, rVecs.front(), tVecs.front());

				if (ratio < 0.8f)
				{
					tracked = false;
					break;
				}
			}

#ifndef UWP

			cv::drawFrameAxes(
				debugMat,
				cameraParams.cameraMatrix,
				cameraParams.distCoeffs,
				rVecs.front(), tVecs.front(),
				detectedMarkers[i].markerSize);

#endif // !UWP

			detectedMarkers[i].tVecs[0] = (float)tVecs[0][0];
			detectedMarkers[i].tVecs[1] = (float)tVecs[0][1];
			detectedMarkers[i].tVecs[2] = (float)tVecs[0][2];

			detectedMarkers[i].rVecs[0] = (float)rVecs[0][0];
			detectedMarkers[i].rVecs[1] = (float)rVecs[0][1];
			detectedMarkers[i].rVecs[2] = (float)rVecs[0][2];

			detectedMarkers[i].tracked = tracked;
		}
	}
}

VOID ArUcoMarkerTracker::DetectArUCoBoardInFrame(
	CONST IN CameraParameters& cameraParams,
	OUT DetectedArUcoBoard& detectedBoard,
	OUT cv::Mat& debugMat)
{
	if (!cameraParams.data)
		return;

	cv::Mat grayMat;
	CreateMatFrames(cameraParams, grayMat, debugMat);

	// Create grid board
	auto board = cv::aruco::GridBoard::create(
		detectedBoard.markersX,
		detectedBoard.markersY,
		detectedBoard.markerSize,
		detectedBoard.markerSeparation,
		dictionary, detectedBoard.markerId);

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

	// Refine detected markers
	cv::aruco::refineDetectedMarkers(
		grayMat,
		board,
		markers,
		markerIds,
		rejectedCandidates,
		cameraParams.cameraMatrix,
		cameraParams.distCoeffs);

	if (markerIds.empty())
		return;

	// Estimate pose board
	cv::Vec3d rvec, tvec;
	auto valid = cv::aruco::estimatePoseBoard(
		markers,
		markerIds,
		board,
		cameraParams.cameraMatrix,
		cameraParams.distCoeffs,
		rvec, tvec);

	if (!valid)
		return;

#ifndef UWP

	cv::drawFrameAxes(
		debugMat,
		cameraParams.cameraMatrix,
		cameraParams.distCoeffs,
		rvec, tvec, detectedBoard.markerSize);

#endif // !UWP

	// If at least one board marker detected
	detectedBoard.tVec[0] = (float)tvec[0];
	detectedBoard.tVec[1] = (float)tvec[1];
	detectedBoard.tVec[2] = (float)tvec[2];

	detectedBoard.rVec[0] = (float)rvec[0];
	detectedBoard.rVec[1] = (float)rvec[1];
	detectedBoard.rVec[2] = (float)rvec[2];

	detectedBoard.tracked = valid ? true : false;
}

ArUcoMarkerTracker::ArUcoMarkerTracker(CONST IN INT dictId)
{
	// Create the aruco dictionary from id
	dictionary = cv::aruco::getPredefinedDictionary(dictId);

	// Create detector parameters
	detectorParams = cv::aruco::DetectorParameters::create();
}