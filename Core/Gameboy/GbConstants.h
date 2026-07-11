#pragma once
#include "pch.h"

class GbConstants
{
public:
	static constexpr uint32_t ScreenWidth = 160;
	static constexpr uint32_t ScreenHeight = 144;
	static constexpr uint32_t ScanlineCount = 154;
	static constexpr uint32_t PixelCount = GbConstants::ScreenWidth * GbConstants::ScreenHeight;
	static constexpr uint32_t LinkedPixelCount = GbConstants::PixelCount * 2;
	static constexpr uint32_t EventViewerBufferSize = 456 * GbConstants::ScreenHeight;
};