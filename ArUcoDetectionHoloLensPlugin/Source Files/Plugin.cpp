#include "Plugin.h"
#include "ArUcoMarkerTracker.h"

CameraParameters cameraParameters;
ArUcoMarkerTracker arUcoMarkertracker;

VOID INTERFACE_API SetCameraParameters(
	Resolution resolution,
	void* dataPtr,
	float* cameraMatrix,
	float* distCoeff)
{
	cameraParameters.data = (byte*)dataPtr;
	cameraParameters.resolution = resolution;

	// Set camera intrinsic parameters for pose estimation
	cameraParameters.cameraMatrix.at<double>(0, 0) = cameraMatrix[0];
	cameraParameters.cameraMatrix.at<double>(1, 1) = cameraMatrix[1];
	cameraParameters.cameraMatrix.at<double>(0, 2) = cameraMatrix[2];
	cameraParameters.cameraMatrix.at<double>(1, 2) = cameraMatrix[3];
	cameraParameters.cameraMatrix.at<double>(2, 2) = 1.0;

	// Set distortion matrix for pose estimation
	cameraParameters.distCoeffs.at<double>(0, 0) = distCoeff[0];
	cameraParameters.distCoeffs.at<double>(0, 1) = distCoeff[1];
	cameraParameters.distCoeffs.at<double>(0, 4) = distCoeff[2];
	cameraParameters.distCoeffs.at<double>(0, 2) = distCoeff[3];
	cameraParameters.distCoeffs.at<double>(0, 3) = distCoeff[4];
}

VOID INTERFACE_API StartArUcoMarkerTracker(CONST IN INT dictId)
{
	arUcoMarkertracker = ArUcoMarkerTracker(dictId);
}

VOID INTERFACE_API DetectArUcoMarkers(OUT DetectedArUcoMarker* detectedMarkers, IN INT numDetectObjects)
{
	arUcoMarkertracker.DetectArUcoMarkersInFrame(cameraParameters, detectedMarkers, numDetectObjects);
}

VOID INTERFACE_API DetectArUcoBoard(OUT DetectedArUcoBoard& detectedBoard)
{
	arUcoMarkertracker.DetectArUCoBoardInFrame(cameraParameters, detectedBoard);
}