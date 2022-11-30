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
};