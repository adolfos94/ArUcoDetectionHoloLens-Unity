#pragma once

#include <vector>

#include "Windows.h"

#include <opencv2/core.hpp>
#include <opencv2/aruco.hpp>
#include <opencv2/opencv.hpp>

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
	int id;
	float tVecs[3];
	float rVecs[3];
};