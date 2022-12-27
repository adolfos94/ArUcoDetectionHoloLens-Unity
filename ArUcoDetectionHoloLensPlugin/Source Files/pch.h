#pragma once

#include <vector>

#include "Windows.h"

#include <opencv2/core.hpp>
#include <opencv2/aruco.hpp>
#include <opencv2/opencv.hpp>
#include <opencv2/rapid.hpp>

struct Resolution
{
	int width;
	int height;
	int refreshRate;
};

struct CameraParameters
{
	byte* data;
	Resolution resolution;
	cv::Mat cameraMatrix;
	cv::Mat distCoeffs;

	CameraParameters()
	{
		cameraMatrix = cv::Mat(3, 3, CV_64F, cv::Scalar(0));
		distCoeffs = cv::Mat(1, 5, CV_64F);
	}
};

struct DetectedArUcoMarker
{
	bool tracked;
	int markerId;
	float markerSize;
	float tVecs[3];
	float rVecs[3];
};

struct DetectedArUcoBoard
{
	bool tracked;
	int markersX;
	int markersY;
	int markerId;
	float markerSize;
	float markerSeparation;
	float tVec[3];
	float rVec[3];
};