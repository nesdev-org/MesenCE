#pragma once
#include "pch.h"

class PlatformUtilities
{
public:
	static void IdleLoop();

	static void DisableScreensaver();
	static void EnableScreensaver();

	static void EnableHighResolutionTimer();
	static void RestoreTimerResolution();
};