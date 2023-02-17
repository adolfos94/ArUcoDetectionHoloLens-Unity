#include "Plugin.h"
#include "ArUcoMarkerTracker.h"

std::unordered_map<std::string, CameraParameters> cameraParameters;
std::unordered_map<std::string, ArUcoMarkerTracker> cameraTrackers;

VOID INTERFACE_API SetCameraParameters(
	char* cameraName,
	bool videoVerticallyMirrored,
	Resolution resolution,
	void* dataPtr,
	float* cameraMatrix,
	float* distCoeff)
{
	CameraParameters cameraParams;

	cameraParams.data = (byte*)dataPtr;
	cameraParams.resolution = resolution;
	cameraParams.videoVerticallyMirrored = videoVerticallyMirrored;

	// Set camera intrinsic parameters for pose estimation
	cameraParams.cameraMatrix.at<double>(0, 0) = cameraMatrix[0];
	cameraParams.cameraMatrix.at<double>(1, 1) = cameraMatrix[1];
	cameraParams.cameraMatrix.at<double>(0, 2) = cameraMatrix[2];
	cameraParams.cameraMatrix.at<double>(1, 2) = cameraMatrix[3];
	cameraParams.cameraMatrix.at<double>(2, 2) = 1.0;

	// Set distortion matrix for pose estimation
	cameraParams.distCoeffs.at<double>(0, 0) = distCoeff[0];
	cameraParams.distCoeffs.at<double>(0, 1) = distCoeff[1];
	cameraParams.distCoeffs.at<double>(0, 4) = distCoeff[2];
	cameraParams.distCoeffs.at<double>(0, 2) = distCoeff[3];
	cameraParams.distCoeffs.at<double>(0, 3) = distCoeff[4];

	cameraParameters[cameraName] = cameraParams;
}

VOID INTERFACE_API StartArUcoMarkerTracker(CONST IN INT dictId, char* cameraName)
{
	cameraTrackers[cameraName] = ArUcoMarkerTracker(dictId);
}

VOID INTERFACE_API DetectArUcoMarkers(OUT DetectedArUcoMarker* detectedMarkers, IN INT numDetectObjects, char* cameraName)
{
	cameraTrackers[cameraName].DetectArUcoMarkersInFrame(cameraParameters[cameraName], detectedMarkers, numDetectObjects);
}

VOID INTERFACE_API DetectArUcoBoard(OUT DetectedArUcoBoard& detectedBoard, char* cameraName)
{
	cameraTrackers[cameraName].DetectArUCoBoardInFrame(cameraParameters[cameraName], detectedBoard);
}