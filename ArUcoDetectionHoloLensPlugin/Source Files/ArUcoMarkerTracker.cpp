#include "ArUcoMarkerTracker.h"

VOID ArUcoMarkerTracker::RefineArUcoMarkerTracker(
	CONST IN CameraParameters& cameraParams,
	CONST IN FLOAT* vertexes, CONST IN INT nVertexes,
	CONST IN INT* triangles, CONST IN INT nTriangles,
	OUT DetectedArUcoMarker& detectedMarker)
{
	if (!cameraParams.data || !vertexes || !triangles)
		return;

	// Vectors for rapid pose computation.
	std::vector<cv::Vec3f> pts3d(nVertexes);
	std::vector<cv::Vec3i> tris(nTriangles);

	for (int i = 0; i < nVertexes; ++i)
	{
		pts3d[i] = { vertexes[(i * 3) + 0], vertexes[(i * 3) + 1], vertexes[(i * 3) + 2] };

		if (i < nTriangles)
			tris[i] = { triangles[(i * 3) + 0], triangles[(i * 3) + 1], triangles[(i * 3) + 2] };
	}

	// Vectors for pose (translation and rotation) refinement
	cv::Vec3f rVec;
	cv::Vec3f tVec;

	rVec = { detectedMarker.rVecs[0], detectedMarker.rVecs[1], detectedMarker.rVecs[2] };
	tVec = { detectedMarker.tVecs[0], detectedMarker.tVecs[1], detectedMarker.tVecs[2] };

	// Create cv::Mat from sensor frame
	cv::Mat wrappedMat = cv::Mat(
		cameraParams.resolution.height,
		cameraParams.resolution.width,
		CV_8UC3, cameraParams.data);

	auto ratio = cv::rapid::rapid(
		wrappedMat, 100, 15, pts3d, tris,
		cameraParams.cameraMatrix, rVec, tVec);
}

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