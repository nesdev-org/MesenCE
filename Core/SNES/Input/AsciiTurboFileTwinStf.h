#pragma once
#include "pch.h"
#include "Shared/BaseControlDevice.h"
#include "Shared/Emulator.h"
#include "Shared/BatteryManager.h"
#include "Shared/Interfaces/IBattery.h"
#include "Utilities/Serializer.h"

class AsciiTurboFileTwinStf : public BaseControlDevice, public IBattery
{
private:
	// Data access
	static constexpr int FileSize = 128 * 1024;
	uint32_t _position = 0;
	uint8_t _data[AsciiTurboFileTwinStf::FileSize] = {};
	uint8_t _currentByte = 0; // 8-bit shift register; current read/write byte
	uint8_t _mostRecentByte; // Byte most recently read or written

	// Command state
	bool _writeMode = false;
	bool _readMode = false;
	bool _firstAccess = false; // First data access after entering write mode or read mode
	bool _didReadWithStrobe = false; // Were any $4017 reads done while strobe was on?
	bool _didWriteAnything = false; // Have any writes actually gone through?
	uint32_t _newCommand = 0; // 28-bit shift register

	// Status report
	uint32_t _stateBuffer = 0; // For the status report

	SnesConsole* _console = nullptr;

protected:
	void Serialize(Serializer& s) override
	{
		BaseControlDevice::Serialize(s);
		SVArray(_data, AsciiTurboFileTwinStf::FileSize);
		SV(_currentByte);
		SV(_mostRecentByte);
		SV(_position);
		SV(_writeMode);
		SV(_readMode);
		SV(_firstAccess);
		SV(_didReadWithStrobe);
		SV(_didWriteAnything);
		SV(_newCommand);
	}

public:
	AsciiTurboFileTwinStf(SnesConsole* console) : BaseControlDevice(console->GetEmulator(), ControllerType::AsciiTurboFileTwinStf, BaseControlDevice::ExpDevicePort)
	{
		_console = console;
	}

	void Init() override
	{
		_console->InitializeRam(_data, AsciiTurboFileTwinStf::FileSize);
		_emu->GetBatteryManager()->LoadBattery(".turbofile_stf.sav", _data, AsciiTurboFileTwinStf::FileSize);
	}

	void SaveBattery() override
	{
		_emu->GetBatteryManager()->SaveBattery(".turbofile_stf.sav", _data, AsciiTurboFileTwinStf::FileSize);
	}

	void RefreshStateBuffer() override
	{
		_stateBuffer = 0b011111110111000000000000 | // Controller type $E, and then disambiguated with $FE
			((uint8_t)(_writeMode & !_didWriteAnything) << 24) |
			((uint8_t)_readMode << 25);
		// Bit 26 is 1 if batteries are missing
		// TODOSNES - Verify behavior of bit 26; it may actually be a "low battery" flag, but so far it's only been confirmed that it is 1 when the Turbo File Twin has no batteries in it
		// Bit 27 is 1 if write protect is on
		// Bits 28-31 are capacity; 0 = 128 KiB
	}

	uint8_t ReadRam(uint16_t addr) override
	{
		uint8_t output = 0;

		if(addr == 0x4017) {
			StrobeProcessRead();

			// Get one bit from the status
			output = _stateBuffer & 0x01;
			_stateBuffer >>= 1;
			_stateBuffer |= 1 << 31; // All bits after the first 32 are 1s in STF mode
			if(_strobe) {
				// Write mode is exited by turning the strobe and off without doing any $4017 reads
				_didReadWithStrobe = true;
				// Read mode is exited by doing a $4017 read with strobe
				_readMode = false;
				// Write mode is also exited if a $4017 read with strobe is done on the first access
				if(_firstAccess) {
					_writeMode = false;
				}
			}

			uint8_t ioBit = (_console->GetInternalRegisters()->GetIoPortOutput() & 0x80) ? 0x01 : 0x00;

			_newCommand = (_newCommand >> 1) | (ioBit << 27);

			if(!(_currentByte & 0x01)) {
				// Second output bit is the opposite of whatever bit is getting shifted out of _currentByte
				output |= 2;
			}
			_currentByte >>= 1;
			if(_writeMode && _strobe && ioBit) {
				_currentByte |= 0x80;
			}
		}

		return output;
	}

	void WriteRam(uint16_t addr, uint8_t value) override
	{
		bool prevStrobe = _strobe;
		_strobe = (value & 0x01) == 0x01;

		if(!prevStrobe && _strobe) {
			_didReadWithStrobe = false;
		}

		if(prevStrobe && !_strobe) {
			RefreshStateBuffer();
			if(!_writeMode && !_readMode) {
				// Potentially start a new command
				_position = _newCommand >> 8;
				if((_newCommand & 0xFF) == 0x24) {
					_readMode = true;
				} else if((_newCommand & 0xFF) == 0x75) {
					_writeMode = true;
				}
				_firstAccess = true;
				_didWriteAnything = false;
				_currentByte = _mostRecentByte;
			} else {
				// Don't do a read or write the first time the strobe is turned on and off
				// (Allows the program to get a status report and confirm the read/write command worked before accessing data)
				if(!_firstAccess) {
					if(_writeMode) {
						if(_didReadWithStrobe) {
							_data[_position % FileSize] = _currentByte;
							_mostRecentByte = _currentByte;
							_didWriteAnything = true;
						} else {
							_writeMode = false;
						}
					} else if(_readMode) {
						_mostRecentByte = _data[_position % FileSize];
						_currentByte = _mostRecentByte;
					}
					_position++;
				}
				_firstAccess = false;
			}
			_newCommand = 0;
		}
	}
};
