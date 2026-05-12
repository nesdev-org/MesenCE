#pragma once
#include "pch.h"
#include "Shared/BaseControlDevice.h"
#include "Shared/Emulator.h"
#include "Shared/BatteryManager.h"
#include "Shared/Interfaces/IBattery.h"
#include "Utilities/Serializer.h"

class AsciiTurboFileTwinTf2 : public BaseControlDevice, public IBattery
{
private:
	static constexpr int FileSize = 0x2000;
	static constexpr int BitCount = FileSize * 8;
	uint16_t _position = 0;
	uint8_t _data[AsciiTurboFileTwinTf2::FileSize] = {};
	uint8_t _unlockCounter = 0; // 4-bit counter - consistently 0 at power on
	bool _unlocked = false; // Memory access is allowed when true

	SnesConsole* _console = nullptr;
	uint32_t _stateBuffer = 0; // For the status report

protected:
	void Serialize(Serializer& s) override
	{
		BaseControlDevice::Serialize(s);
		SVArray(_data, AsciiTurboFileTwinTf2::FileSize);
		SV(_position);
		SV(_unlockCounter);
		SV(_unlocked);
		SV(_stateBuffer);
	}

public:
	AsciiTurboFileTwinTf2(SnesConsole* console) : BaseControlDevice(console->GetEmulator(), ControllerType::AsciiTurboFileTwinTf2, BaseControlDevice::ExpDevicePort)
	{
		_console = console;
	}

	void Init() override
	{
		_console->InitializeRam(_data, AsciiTurboFileTwinTf2::FileSize);
		_emu->GetBatteryManager()->LoadBattery(".turbofile.sav", _data, AsciiTurboFileTwinTf2::FileSize);
	}

	void SaveBattery() override
	{
		_emu->GetBatteryManager()->SaveBattery(".turbofile.sav", _data, AsciiTurboFileTwinTf2::FileSize);
	}

	void RefreshStateBuffer() override
	{
		_stateBuffer = 0b111111110111000000000000; // Controller type $E, and then disambiguated with $FF
		if(_unlocked) {
			_stateBuffer |= 1 << 11;
		}
	}

	uint8_t ReadRam(uint16_t addr) override
	{
		uint8_t output = 0;

		if(addr == 0x4017) {
			StrobeProcessRead();

			if(_strobe) {
				_unlockCounter = (_unlockCounter + 1) & 0xF;
				_position = 0;
			} else {
				_unlocked = _unlockCounter == 0xF;
			}

			// Return one bit from the status
			output = _stateBuffer & 0x01;
			_stateBuffer >>= 1;
			_stateBuffer |= 1 << 23; // All bits after the first 24 are 1s in TFII mode

			// Get current bit in the Turbo File data
			output |= ((_data[_position / 8] >> (_position % 8)) & 0x01) << 1;
		}

		return output;
	}

	void WriteRam(uint16_t addr, uint8_t value) override
	{
		bool prevStrobe = _strobe;
		_strobe = (value & 0x01) == 0x01;

		if(prevStrobe && !_strobe) {
			RefreshStateBuffer();

			if(_unlocked) {
				//Perform write and increase position
				uint8_t ioPort = _console->GetInternalRegisters()->GetIoPortOutput();
				_data[_position / 8] &= ~(1 << (_position % 8));
				_data[_position / 8] |= ((ioPort & 0x80) ? 0x01 : 0x00) << (_position % 8);
				_position = (_position + 1) & (AsciiTurboFileTwinTf2::BitCount - 1);
			}
		}
	}
};
