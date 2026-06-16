#include "pch.h"
#include "Shared/Video/DebugStats.h"
#include "Shared/Video/DebugHud.h"
#include "Shared/Audio/SoundMixer.h"
#include "Shared/Interfaces/IAudioDevice.h"
#include "Shared/Emulator.h"
#include "Shared/RewindManager.h"
#include "Shared/EmuSettings.h"
#include "Utilities/StringUtilities.h"

void DebugStats::ResetStats()
{
	_core = {};
	_render = {};
}

void DebugStats::UpdateStats(Emulator* emu, bool forRender, double frameTime)
{
	VideoStats& stats = forRender ? _render : _core;

	if(frameTime > 100 && emu->GetSettings()->GetEmulationSpeed() >= 25) {
		//Ignore anything over 100ms, this is probably bad data caused by e.g the emulator being paused
		//or the debug information being turned off and then back on.
		return;
	}

	stats.LastFrameTime = frameTime;
	stats.FrameDurations[stats.FrameDurationIndex] = frameTime;
	stats.FrameDurationIndex = (stats.FrameDurationIndex + 1) % 60;

	if(emu->GetFrameCount() > 60) {
		stats.FrameMin = std::min(frameTime, stats.FrameMin);
		stats.FrameMax = std::max(frameTime, stats.FrameMax);
	} else {
		stats.FrameMin = 9999;
		stats.FrameMax = 0;
	}
}

void DebugStats::DisplayStats(Emulator* emu, DebugHud* hud)
{
	AudioStatistics stats = emu->GetSoundMixer()->GetStatistics();
	AudioConfig audioCfg = emu->GetSettings()->GetAudioConfig();

	int startFrame = -1;

	hud->DrawRectangle(8, 8, 115, 49, 0x40000000, true, 1, startFrame);
	hud->DrawRectangle(8, 8, 115, 49, 0xFFFFFF, false, 1, startFrame);

	hud->DrawString(10, 10, "Audio Stats", 0xFFFFFF, 0xFF000000, 1, startFrame);
	hud->DrawString(10, 21, "Latency: ", 0xFFFFFF, 0xFF000000, 1, startFrame);

	int color = (stats.AverageLatency > 0 && std::abs(stats.AverageLatency - audioCfg.AudioLatency) > 3) ? 0xFF0000 : 0xFFFFFF;
	hud->DrawString(54, 21, StringUtilities::ToString(stats.AverageLatency, 2), color, 0xFF000000, 1, startFrame);

	hud->DrawString(10, 30, "Underruns: " + std::to_string(stats.BufferUnderrunEventCount), 0xFFFFFF, 0xFF000000, 1, startFrame);
	hud->DrawString(10, 39, "Buffer Size: " + std::to_string(stats.BufferSize / 1024) + "kb", 0xFFFFFF, 0xFF000000, 1, startFrame);
	hud->DrawString(10, 48, "Rate: " + std::to_string((uint32_t)(audioCfg.SampleRate * emu->GetSoundMixer()->GetRateAdjustment())) + "Hz", 0xFFFFFF, 0xFF000000, 1, startFrame);

	DrawVideoStats(130, 8, hud, false, emu->GetFps());
	DrawVideoStats(256, 8, hud, true, emu->GetFps());

	hud->DrawRectangle(8, 59, 115, 33, 0x40000000, true, 1, startFrame);
	hud->DrawRectangle(8, 59, 115, 33, 0xFFFFFF, false, 1, startFrame);

	hud->DrawString(10, 62, "Misc. Stats", 0xFFFFFF, 0xFF000000, 1, startFrame);

	RewindStats rewindStats = emu->GetRewindManager()->GetStats();
	double memUsage = (double)rewindStats.MemoryUsage / (1024 * 1024);
	hud->DrawString(10, 73, "Rewind mem.: " + StringUtilities::ToString(memUsage, 2), 0xFFFFFF, 0xFF000000, 1, startFrame);

	if(rewindStats.HistoryDuration > 0) {
		hud->DrawString(9, 82, "   Per min.: " + StringUtilities::ToString(memUsage * 60 * 60 / rewindStats.HistoryDuration, 2), 0xFFFFFF, 0xFF000000, 1, startFrame);
	}
}

void DebugStats::DrawVideoStats(int x, int y, DebugHud* hud, bool forRender, double fps)
{
	VideoStats& stats = forRender ? _render : _core;
	hud->DrawRectangle(x, y, 120, 49, 0x40000000, true, 1);
	hud->DrawRectangle(x, y, 120, 49, 0xFFFFFF, false, 1);
	hud->DrawString(x + 2, y + 2, forRender ? "Renderer Stats" : "Core Stats", 0xFFFFFF, 0xFF000000, 1);

	double totalDuration = 0;
	for(int i = 0; i < 60; i++) {
		totalDuration += stats.FrameDurations[i];
	}

	hud->DrawString(x + 2, y + 13, "FPS: " + StringUtilities::ToString(1000 / (totalDuration / 60), 4), 0xFFFFFF, 0xFF000000, 1);
	hud->DrawString(x + 2, y + 22, "Last Frame: " + StringUtilities::ToString(stats.LastFrameTime, 2), 0xFFFFFF, 0xFF000000, 1);
	hud->DrawString(x + 2, y + 31, "Min Delay: " + StringUtilities::ToString((stats.FrameMin < 9999) ? stats.FrameMin : 0.0, 2), 0xFFFFFF, 0xFF000000, 1);
	hud->DrawString(x + 2, y + 40, "Max Delay: " + StringUtilities::ToString(stats.FrameMax, 2), 0xFFFFFF, 0xFF000000, 1);
	hud->DrawRectangle(x, y + 51, 120, 32, 0xFFFFFF, false, 1);
	hud->DrawRectangle(x + 1, y + 52, 118, 30, 0x40000000, true, 1);

	double expectedFrameDelay = 1000 / fps;

	for(int i = 0; i < 59; i++) {
		double duration = stats.FrameDurations[(stats.FrameDurationIndex + i) % 60];
		double nextDuration = stats.FrameDurations[(stats.FrameDurationIndex + i + 1) % 60];

		duration = std::min(25.0, std::max(10.0, duration));
		nextDuration = std::min(25.0, std::max(10.0, nextDuration));

		int lineColor = 0x00FF00;
		if(std::abs(duration - expectedFrameDelay) > 2) {
			lineColor = 0xFF0000;
		} else if(std::abs(duration - expectedFrameDelay) > 1) {
			lineColor = 0xFFA500;
		}
		hud->DrawLine(x + i * 2, y + 52 + 50 - duration * 2, x + i * 2 + 2, y + 52 + 50 - nextDuration * 2, lineColor, 1);
	}
}
