#pragma once
#include "pch.h"

class Emulator;
class DebugHud;

struct VideoStats
{
	double LastFrameTime = 0;
	double FrameDurations[60] = {};
	uint32_t FrameDurationIndex = 0;
	double MaxGap = 0;
};

class DebugStats
{
private:
	VideoStats _core;
	VideoStats _render;
	uint32_t _skipFrames = 0;

	void DrawVideoStats(int x, int y, DebugHud* hud, bool forRender, double fps);

public:
	void ResetStats();
	void UpdateStats(Emulator* emu, bool forRender, double frameTime);
	void DisplayStats(Emulator* emu, DebugHud* hud);
};