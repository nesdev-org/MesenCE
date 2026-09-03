#include "PulseSoundManager.h"
#include "Core/Shared/EmuSettings.h"
#include "Core/Shared/MessageManager.h"
#include "Core/Shared/Audio/SoundMixer.h"
#include "Core/Shared/Emulator.h"

PulseSoundManager::PulseSoundManager(Emulator* emu)
{
	_emu = emu;

	if(!LoadPulseAudio()) {
		MessageManager::DisplayMessage("Audio", "PulseAudio load failed.");
	} else if(InitializeAudio(48000, false)) {
		_emu->GetSoundMixer()->RegisterAudioDevice(this);
	} else {
		MessageManager::DisplayMessage("Audio", "PulseAudio initialization failed.");
	}
}

PulseSoundManager::~PulseSoundManager()
{
	Release();
}

void PulseSoundManager::Release()
{
	if(_audioBuffer) {
		pa_stream_cancel_write(_stream);
		delete[] _audioBuffer;
		_audioBuffer = nullptr;
	}

	if(_stream) {
		pa_stream_disconnect(_stream);
		pa_stream_unref(_stream);
		_stream = nullptr;
	}

	if(_context) {
		pa_context_disconnect(_context);
		pa_context_unref(_context);
		_context = nullptr;
	}

	if(_mainloop) {
		pa_mainloop_free(_mainloop);
		_mainloop = nullptr;
	}

	_bufferPos = 0;
	_bufferSize = 0;
}

void PulseSoundManager::OnUnderflowCallback(pa_stream* p, void* userdata)
{
	((PulseSoundManager*)userdata)->_bufferUnderrunEventCount++;
}

bool PulseSoundManager::InitializeAudio(uint32_t sampleRate, bool isStereo)
{
	_sampleRate = sampleRate;
	_isStereo = isStereo;
	_previousLatency = _emu->GetSettings()->GetAudioConfig().AudioLatency;

	uint32_t bytesPerSample = 2 * (isStereo ? 2 : 1);
	uint32_t requestedByteLatency = (uint32_t)((float)(sampleRate * _previousLatency) / 1000.0f * bytesPerSample);

	_mainloop = pa_mainloop_new();
	if(!_mainloop) {
		return false;
	}

	_context = pa_context_new(pa_mainloop_get_api(_mainloop), "Mesen");
	if(!_context) {
		Release();
		return false;
	}

	if(pa_context_connect(_context, nullptr, PA_CONTEXT_NOFLAGS, nullptr) < 0) {
		Release();
		return false;
	}

	pa_context_state_t state = {};
	do {
		pa_mainloop_iterate(_mainloop, 1, nullptr);
		state = pa_context_get_state(_context);
		if(state == PA_CONTEXT_FAILED || state == PA_CONTEXT_TERMINATED) {
			MessageManager::Log("[Audio] Context connection failed: " + string(pa_strerror(pa_context_errno(_context))));
			Release();
			return false;
		}
	} while(state != PA_CONTEXT_READY);

	pa_sample_spec ss;
	ss.format = PA_SAMPLE_S16LE;
	ss.rate = sampleRate;
	ss.channels = (uint8_t)(isStereo ? 2 : 1);

	_stream = pa_stream_new(_context, "Audio", &ss, nullptr);
	if(!_stream) {
		MessageManager::Log("[Audio] pa_stream_new failed.");
		Release();
		return false;
	}

	pa_buffer_attr attr;
	attr.maxlength = requestedByteLatency * 4;
	attr.tlength = requestedByteLatency;
	attr.prebuf = -1;
	attr.minreq = -1;
	attr.fragsize = -1;

	pa_stream_flags_t flags = (pa_stream_flags_t)(PA_STREAM_AUTO_TIMING_UPDATE | PA_STREAM_INTERPOLATE_TIMING);
	if(pa_stream_connect_playback(_stream, _deviceName.c_str(), &attr, flags, nullptr, nullptr) < 0) {
		if(!_deviceName.empty()) {
			MessageManager::Log("[Audio] Failed connecting to requested device '" + _deviceName + "'. Retrying default.");
			pa_stream_unref(_stream);
			_stream = pa_stream_new(_context, "Audio", &ss, nullptr);
			if(_stream && pa_stream_connect_playback(_stream, nullptr, &attr, flags, nullptr, nullptr) < 0) {
				MessageManager::Log("[Audio] pa_stream_new failed on retry.");
				Release();
				return false;
			}
		} else {
			int paErr = pa_context_errno(_context);
			MessageManager::Log("[Audio] pa_stream_connect_playback failed: " + std::string(pa_strerror(paErr)));
			Release();
			return false;
		}
	}

	pa_stream_state_t streamState = {};
	do {
		pa_mainloop_iterate(_mainloop, 1, nullptr);
		streamState = pa_stream_get_state(_stream);
		if(streamState == PA_STREAM_FAILED || streamState == PA_STREAM_TERMINATED) {
			int paErr = pa_context_errno(_context);
			MessageManager::Log("[Audio] Stream state failed: " + std::string(pa_strerror(paErr)));
			Release();
			return false;
		}
	} while(streamState != PA_STREAM_READY);

	pa_stream_set_underflow_callback(_stream, PulseSoundManager::OnUnderflowCallback, this);

	_bufferSize = requestedByteLatency * 10;
	_audioBuffer = new uint8_t[_bufferSize];
	_bufferPos = 0;
	_needReset = false;

	return true;
}

vector<SoundDeviceInfo> PulseSoundManager::GetAvailableDeviceInfo()
{
	if(!LoadPulseAudio()) {
		return {};
	}

	pa_mainloop* mainloop = pa_mainloop_new();
	if(!mainloop) {
		return {};
	}

	DeviceEnumData enumData;
	pa_context* context = pa_context_new(pa_mainloop_get_api(mainloop), "GetAvailableDeviceInfo");
	if(context) {
		pa_context_set_state_callback(context, [](pa_context* c, void* userdata) {
			DeviceEnumData* enumData = (DeviceEnumData*)userdata;
			pa_context_state_t state = pa_context_get_state(c);
			if(state == PA_CONTEXT_READY) {
				pa_operation* op = pa_context_get_sink_info_list(c, [](pa_context*, const pa_sink_info* i, int eol, void* userdata) {
					DeviceEnumData* ctx = (DeviceEnumData*)userdata;
					if(eol != 0) {
						ctx->Done = true; 
					} else if(i && i->name) {
						ctx->DeviceList.push_back({ i->name, i->description ? i->description : "" });
					}
				}, enumData);
				if(op){ 
					pa_operation_unref(op);
				}
			} else if(state == PA_CONTEXT_FAILED || state == PA_CONTEXT_TERMINATED) {
				enumData->Done = true;
			} }, &enumData);

		if(pa_context_connect(context, nullptr, PA_CONTEXT_NOFLAGS, nullptr) >= 0) {
			while(!enumData.Done) {
				if(pa_mainloop_iterate(mainloop, 1, nullptr) < 0) {
					break;
				}
			}
		}
		pa_context_disconnect(context);
		pa_context_unref(context);
	}

	pa_mainloop_free(mainloop);
	return enumData.DeviceList;
}

string PulseSoundManager::GetAvailableDevices()
{
	string deviceString = "Default||";
	for(SoundDeviceInfo& device : GetAvailableDeviceInfo()) {
		deviceString += device.Desc + "||";
	}
	return deviceString;
}

void PulseSoundManager::SetAudioDevice(string deviceDesc)
{
	if(deviceDesc != _deviceDesc) {
		_deviceDesc = deviceDesc;

		if(deviceDesc == "Default") {
			_deviceName = "";
			_needReset = true;
		} else {
			for(SoundDeviceInfo& device : GetAvailableDeviceInfo()) {
				if(device.Desc == deviceDesc) {
					_deviceName = device.Name;
					_needReset = true;
					break;
				}
			}
		}
	}
}

void PulseSoundManager::PlayBuffer(int16_t* soundBuffer, uint32_t sampleCount, uint32_t sampleRate, bool isStereo)
{
	uint32_t latency = _emu->GetSettings()->GetAudioConfig().AudioLatency;

	if(_sampleRate != sampleRate || _isStereo != isStereo || _needReset || _previousLatency != latency) {
		Release();
		InitializeAudio(sampleRate, isStereo);
	}

	if(!IsStreamReady()) {
		return;
	}

	Play();

	pa_mainloop_iterate(_mainloop, 0, nullptr);

	uint32_t bytesToWrite = sampleCount * (isStereo ? 4 : 2);
	if(_bufferPos + bytesToWrite < _bufferSize) {
		memcpy(_audioBuffer + _bufferPos, soundBuffer, bytesToWrite);
		_bufferPos += bytesToWrite;
	}

	while(_bufferPos > 0) {
		void* destBuffer = nullptr;
		size_t writeSize = _bufferPos;

		if(pa_stream_begin_write(_stream, &destBuffer, &writeSize) < 0 || !destBuffer || writeSize == 0) {
			break;
		}

		size_t bytesToCopy = std::min<size_t>(writeSize, _bufferPos);
		memcpy(destBuffer, _audioBuffer, bytesToCopy);

		if(pa_stream_write(_stream, destBuffer, bytesToCopy, nullptr, 0, PA_SEEK_RELATIVE) < 0) {
			break;
		}

		_bufferPos -= bytesToCopy;
		if(_bufferPos > 0) {
			memmove(_audioBuffer, _audioBuffer + bytesToCopy, _bufferPos);
		}
	}
}

void PulseSoundManager::WaitForOperation(pa_operation* op)
{
	if(!op) {
		return;
	}
	while(pa_operation_get_state(op) == PA_OPERATION_RUNNING) {
		if(pa_mainloop_iterate(_mainloop, 1, nullptr) < 0) {
			break;
		}
	}
	pa_operation_unref(op);
}

bool PulseSoundManager::IsStreamReady()
{
	return _stream && _mainloop && pa_stream_get_state(_stream) == PA_STREAM_READY;
}

void PulseSoundManager::Play()
{
	if(!IsStreamReady()) {
		return;
	}
	WaitForOperation(pa_stream_cork(_stream, 0, nullptr, nullptr));
}

void PulseSoundManager::Pause()
{
	if(!IsStreamReady()) {
		return;
	}
	WaitForOperation(pa_stream_cork(_stream, 1, nullptr, nullptr));
}

void PulseSoundManager::Stop()
{
	if(!IsStreamReady()) {
		return;
	}

	Pause();
	WaitForOperation(pa_stream_flush(_stream, nullptr, nullptr));

	_bufferPos = 0;
	ResetStats();
}

void PulseSoundManager::ProcessEndOfFrame()
{
	if(!IsStreamReady()) {
		return;
	}

	AudioConfig cfg = _emu->GetSettings()->GetAudioConfig();
	SetAudioDevice(cfg.AudioDevice);

	WaitForOperation(pa_stream_update_timing_info(_stream, nullptr, nullptr));

	const pa_timing_info* info = pa_stream_get_timing_info(_stream);
	if(info) {
		if(info->write_index >= info->read_index) {
			ProcessLatency(0, info->write_index - info->read_index);
		} else {
			_bufferUnderrunEventCount++;
		}
	}

	uint32_t emulationSpeed = _emu->GetSettings()->GetEmulationSpeed();
	if(_averageLatency > 0 && emulationSpeed <= 100 && emulationSpeed > 0 && abs((int32_t)_averageLatency - (int32_t)_emu->GetSettings()->GetAudioConfig().AudioLatency) > 50) {
		Stop();
	}
}