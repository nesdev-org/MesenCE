#pragma once

#include "Core/Shared/Audio/BaseSoundManager.h"
#include "Linux/include/PulseAudio.h"

class Emulator;

struct SoundDeviceInfo
{
	string Name;
	string Desc;
};

struct DeviceEnumData
{
	bool Done = false;
	vector<SoundDeviceInfo> DeviceList;
};

class PulseSoundManager : public BaseSoundManager
{
public:
	PulseSoundManager(Emulator* emu);
	~PulseSoundManager();

	void PlayBuffer(int16_t* soundBuffer, uint32_t sampleCount, uint32_t sampleRate, bool isStereo) override;
	void Pause() override;
	void Stop() override;

	void ProcessEndOfFrame() override;

	string GetAvailableDevices() override;
	void SetAudioDevice(string deviceName) override;

private:
	vector<SoundDeviceInfo> GetAvailableDeviceInfo();
	bool InitializeAudio(uint32_t sampleRate, bool isStereo);
	void Release();

	void WaitForOperation(pa_operation* op);
	bool IsStreamReady();

	void Play();

	static void OnUnderflowCallback(pa_stream* p, void* userdata);

private:
	Emulator* _emu = nullptr;
	string _deviceName;
	string _deviceDesc;
	bool _needReset = false;

	pa_mainloop* _mainloop = nullptr;
	pa_context* _context = nullptr;
	pa_stream* _stream = nullptr;

	uint8_t* _audioBuffer = nullptr;
	uint32_t _bufferPos = 0;

	uint16_t _previousLatency = 0;
};