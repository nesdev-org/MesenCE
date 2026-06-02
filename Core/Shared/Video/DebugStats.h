#pragma once
#include "pch.h"

class Emulator;
class DebugHud;

struct VideoStats
{
	double LastFrameTime = 0;
	double FrameDurations[60] = {};
	uint32_t FrameDurationIndex = 0;
	double FrameMin = 9999;
	double FrameMax = 0;
};

class DebugStats
{
private:
	VideoStats _core;
	VideoStats _render;

	void DrawVideoStats(int x, int y, DebugHud* hud, bool forRender, double fps);

public:
	void ResetStats();
	void UpdateStats(Emulator* emu, bool forRender, double frameTime);
	void DisplayStats(Emulator* emu, DebugHud* hud);
};