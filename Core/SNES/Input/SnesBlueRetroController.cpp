#include "pch.h"
#include "SNES/Input/SnesBlueRetroController.h"
#include "SNES/SnesConsole.h"
#include "Shared/Emulator.h"
#include "Shared/KeyManager.h"

SnesBlueRetroController::SnesBlueRetroController(Emulator* emu, SnesConsole* console, uint8_t port, KeyMappingSet keyMappings) : SnesRumbleController(emu, console, port, keyMappings)
{
	_type = ControllerType::SnesBlueRetroController;

	_lxAxisCodes = {
		{ KeyManager::GetKeyCode("Pad1 LT X"), false, false },
		{ KeyManager::GetKeyCode("Joy1 X"), false, false },
		{ KeyManager::GetKeyCode("Pad1 X"), false, false },
	};

	_lyAxisCodes = {
		{ KeyManager::GetKeyCode("Pad1 LT Y"), false, false },
		{ KeyManager::GetKeyCode("Joy1 Y"), true, false },
		{ KeyManager::GetKeyCode("Pad1 Y"), false, false },
	};

	_rxAxisCodes = {
		{ KeyManager::GetKeyCode("Pad1 RT X"), false, false },
		{ KeyManager::GetKeyCode("Joy1 Z"), false, false },
		{ KeyManager::GetKeyCode("Pad1 X2"), false, false },
	};

	_ryAxisCodes = {
		{ KeyManager::GetKeyCode("Pad1 RT Y"), false, false },
		{ KeyManager::GetKeyCode("Joy1 Z2"), true, false },
		{ KeyManager::GetKeyCode("Pad1 Y2"), false, false },
	};

	_alAxisCodes = {
		{ KeyManager::GetKeyCode("Pad1 AL"), false, false },
		{ KeyManager::GetKeyCode("Joy1 X2"), false, true },
		{ KeyManager::GetKeyCode("Pad1 Z"), true, true },
	};

	_arAxisCodes = {
		{ KeyManager::GetKeyCode("Pad1 AR"), false, false },
		{ KeyManager::GetKeyCode("Joy1 Y2"), false, true },
		{ KeyManager::GetKeyCode("Pad1 Z2"), false, true },
	};
}

SnesBlueRetroController::~SnesBlueRetroController()
{
}

void SnesBlueRetroController::Serialize(Serializer& s)
{
	SnesRumbleController::Serialize(s);
	SV(_brState);
	SV(_mode);
}

void SnesBlueRetroController::RefreshStateBuffer()
{
	SnesRumbleController::RefreshStateBuffer();

	_brState = ((uint64_t)IsPressed(Buttons::B) << 63) |
		((uint64_t)IsPressed(Buttons::Y) << 62) |
		((uint64_t)IsPressed(Buttons::Select) << 61) |
		((uint64_t)IsPressed(Buttons::Start) << 60) |
		((uint64_t)IsPressed(Buttons::Up) << 59) |
		((uint64_t)IsPressed(Buttons::Down) << 58) |
		((uint64_t)IsPressed(Buttons::Left) << 57) |
		((uint64_t)IsPressed(Buttons::Right) << 56) |
		((uint64_t)IsPressed(Buttons::A) << 55) |
		((uint64_t)IsPressed(Buttons::X) << 54) |
		((uint64_t)IsPressed(Buttons::L) << 53) |
		((uint64_t)IsPressed(Buttons::R) << 52);

	if(_mode) {
		int16_t lx = GetAxisValue(_lxAxisCodes);
		int16_t ly = GetAxisValue(_lyAxisCodes);

		int16_t rx = GetAxisValue(_rxAxisCodes);
		int16_t ry = GetAxisValue(_ryAxisCodes);

		int16_t al = GetAxisValue(_alAxisCodes);
		int16_t ar = GetAxisValue(_arAxisCodes);

		if(_mode == 1) {
			lx = (lx >> 8) & 0xFF;
			ly = (ly >> 8) & 0xFF;
			rx = (rx >> 8) & 0xFF;
			ry = (ry >> 8) & 0xFF;
			al = (al >> 0) & 0xFF;
			ar = (ar >> 0) & 0xFF;
			_brState |= (0x6ULL << 48) | ((uint64_t)lx << 40) | ((uint64_t)ly << 32) | ((uint64_t)rx << 24) | ((uint64_t)ry << 16) | ((uint64_t)al << 8) | ((uint64_t)ar << 0);
		} else {
			lx = (lx >> 12) & 0xF;
			ly = (ly >> 12) & 0xF;
			rx = (rx >> 12) & 0xF;
			ry = (ry >> 12) & 0xF;
			al = (al >> 4) & 0xF;
			ar = (ar >> 4) & 0xF;
			_brState |= (0x7ULL << 48) | ((uint64_t)lx << 44) | ((uint64_t)ly << 40) | ((uint64_t)rx << 36) | ((uint64_t)ry << 32) | ((uint64_t)al << 28) | ((uint64_t)ar << 24) | 0x0000000000FFFFFF;
		}
	} else {
		_brState |= 0x0000FFFFFFFFFFFF;
	}
}

int16_t SnesBlueRetroController::GetAxisValue(vector<AxisInfo>& axisInfos)
{
	for(AxisInfo axisInfo : axisInfos) {
		if(axisInfo.KeyCode != 0) {
			optional<int16_t> axis = KeyManager::GetAxisPosition(axisInfo.KeyCode);
			if(axis.has_value()) {
				int16_t value = axis.value();
				if(axisInfo.Invert) {
					value = (value == INT16_MIN ? INT16_MAX : -value);
				}
				if(axisInfo.Scale) {
					value = (value >> 8) + 0x80;
				}
				return value;
			}
		}
	}

	return 0;
}

uint8_t SnesBlueRetroController::ReadRam(uint16_t addr)
{
	SnesRumbleController::ReadRam(addr);

	if((_rumbleData & 0xFF00) == 0x6200) {
		_mode = _rumbleData & 0x0003;
	}

	uint8_t output = 0;
	if((addr == 0x4016 && (_port & 0x01) == 0) || (addr == 0x4017 && (_port & 0x01) == 1)) {
		StrobeProcessRead();

		output = ((_brState & 0x8000000000000000) >> 63) & 1;
		if(_port >= 2) {
			output <<= 1;
		}
		_brState <<= 1;
		_brState |= 0x1;
	}
	return output;
}
