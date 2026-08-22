#pragma once
#include "pch.h"
#include "DirectSoundManager.h"
#include "WasapiSoundManager.h"

class IAudioDevice;
class Emulator;

class SoundManager
{
public:
	static IAudioDevice* Create(Emulator* emu, HWND hwnd)
	{
		switch(emu->GetSettings()->GetAudioConfig().WindowsAudio) {
			default:
			case WindowsAudioBackend::Wasapi: return new WasapiSoundManager(emu);
			case WindowsAudioBackend::DirectSound: return new DirectSoundManager(emu, hwnd);
		}
	}
};