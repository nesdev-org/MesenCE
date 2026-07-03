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
	_skipFrames = 60;
}

void DebugStats::UpdateStats(Emulator* emu, bool forRender, double frameTime)
{
	VideoStats& stats = forRender ? _render : _core;
	uint32_t speed = emu->GetSettings()->GetEmulationSpeed();
	if(frameTime > 100 && speed >= 25) {
		//Ignore anything over 100ms, this is probably bad data caused by e.g the emulator being paused
		//or the debug information being turned off and then back on.
		return;
	}

	stats.LastFrameTime = frameTime;
	stats.FrameDurations[stats.FrameDurationIndex] = frameTime;
	stats.FrameDurationIndex = (stats.FrameDurationIndex + 1) % 60;

	if(speed > 0 && emu->GetFrameCount() > 1) {
		if(_skipFrames && --_skipFrames) {
			return;
		}

		double expectedFps = emu->GetFps() * speed / 100;
		double expectedFrameDelay = 1000 / expectedFps;
		stats.MaxGap = std::max(stats.MaxGap, std::abs(frameTime - expectedFrameDelay));
	} else {
		stats.MaxGap = 0;
		_skipFrames = 60;
	}
}

void DebugStats::DisplayStats(Emulator* emu, DebugHud* hud)
{
	AudioStatistics stats = emu->GetSoundMixer()->GetStatistics();
	AudioConfig audioCfg = emu->GetSettings()->GetAudioConfig();

	hud->DrawRectangle(8, 8, 115, 49, 0x40000000, true, 1);
	hud->DrawRectangle(8, 8, 115, 49, 0xFFFFFF, false, 1);

	hud->DrawString(10, 10, "Audio Stats", 0xFFFFFF, 0xFF000000, 1);
	hud->DrawString(10, 21, "Latency: ", 0xFFFFFF, 0xFF000000, 1);

	int color = (stats.AverageLatency > 0 && std::abs(stats.AverageLatency - audioCfg.AudioLatency) > 3) ? 0xFF0000 : 0xFFFFFF;
	hud->DrawString(54, 21, StringUtilities::ToString(stats.AverageLatency, 2), color, 0xFF000000, 1);

	hud->DrawString(10, 30, "Underruns: " + std::to_string(stats.BufferUnderrunEventCount), 0xFFFFFF, 0xFF000000, 1);
	hud->DrawString(10, 39, "Buffer Size: " + std::to_string(stats.BufferSize / 1024) + "kb", 0xFFFFFF, 0xFF000000, 1);
	hud->DrawString(10, 48, "Rate: " + std::to_string((uint32_t)(audioCfg.SampleRate * emu->GetSoundMixer()->GetRateAdjustment())) + "Hz", 0xFFFFFF, 0xFF000000, 1);

	double expectedFps = emu->GetFps() * emu->GetSettings()->GetEmulationSpeed() / 100;
	DrawVideoStats(130, 8, hud, false, expectedFps);
	DrawVideoStats(256, 8, hud, true, expectedFps);

	hud->DrawRectangle(8, 59, 115, 33, 0x40000000, true, 1);
	hud->DrawRectangle(8, 59, 115, 33, 0xFFFFFF, false, 1);

	hud->DrawString(10, 62, "Misc. Stats", 0xFFFFFF, 0xFF000000, 1);

	RewindStats rewindStats = emu->GetRewindManager()->GetStats();
	double memUsage = (double)rewindStats.MemoryUsage / (1024 * 1024);
	hud->DrawString(10, 73, "Rewind mem.: " + StringUtilities::ToString(memUsage, 2), 0xFFFFFF, 0xFF000000, 1);

	if(rewindStats.HistoryDuration > 0) {
		hud->DrawString(9, 82, "   Per min.: " + StringUtilities::ToString(memUsage * 60 * 60 / rewindStats.HistoryDuration, 2), 0xFFFFFF, 0xFF000000, 1);
	}
}

void DebugStats::DrawVideoStats(int x, int y, DebugHud* hud, bool forRender, double fps)
{
	VideoStats& stats = forRender ? _render : _core;
	hud->DrawRectangle(x, y, 120, 41, 0x40000000, true, 1);
	hud->DrawRectangle(x, y, 120, 41, 0xFFFFFF, false, 1);
	hud->DrawString(x + 2, y + 2, forRender ? "Renderer Stats" : "Core Stats", 0xFFFFFF, 0xFF000000, 1);

	double totalDuration = 0;
	for(int i = 0; i < 60; i++) {
		totalDuration += stats.FrameDurations[i];
	}

	hud->DrawString(x + 2, y + 13, "FPS: " + StringUtilities::ToString(1000 / (totalDuration / 60), 4), 0xFFFFFF, 0xFF000000, 1);
	hud->DrawString(x + 2, y + 22, "Last Frame: " + StringUtilities::ToString(stats.LastFrameTime, 2) + " ms", 0xFFFFFF, 0xFF000000, 1);
	hud->DrawString(x + 2, y + 31, "Max Gap: " + StringUtilities::ToString(stats.MaxGap, 2) + " ms", 0xFFFFFF, 0xFF000000, 1);
	hud->DrawRectangle(x, y + 43, 120, 35, 0xFFFFFF, false, 1);
	hud->DrawRectangle(x + 1, y + 44, 118, 33, 0x40000000, true, 1);

	double expectedFrameDelay = 1000 / fps;

	for(int i = 0; i < 59; i++) {
		double duration = stats.FrameDurations[(stats.FrameDurationIndex + i) % 60];
		double nextDuration = stats.FrameDurations[(stats.FrameDurationIndex + i + 1) % 60];

		double gap = std::max(-2.0, std::min(2.0, duration - expectedFrameDelay));
		double nextGap = std::max(-2.0, std::min(2.0, nextDuration - expectedFrameDelay));

		double maxGap = std::max(std::abs(gap), std::abs(nextGap));
		int lineColor = 0x00FF00;
		if(maxGap >= 2) {
			lineColor = 0xFF0000;
		} else if(maxGap >= 1) {
			lineColor = 0xFF8500;
		} else if(maxGap >= 0.5) {
			lineColor = 0xFFC500;
		} else if(maxGap >= 0.25) {
			lineColor = 0xFFFF00;
		}

		hud->DrawLine(x + i * 2, y + 60 + std::round(gap * 8), x + i * 2 + 2, y + 60 + std::round(nextGap * 8), lineColor, 1);
	}
}
