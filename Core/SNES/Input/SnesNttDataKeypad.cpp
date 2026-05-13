#include "pch.h"
#include "SNES/Input/SnesNttDataKeypad.h"
#include "Shared/Emulator.h"
#include "Shared/EmuSettings.h"
#include "Shared/InputHud.h"

SnesNttDataKeypad::SnesNttDataKeypad(Emulator* emu, uint8_t port, KeyMappingSet keyMappings) : BaseControlDevice(emu, ControllerType::SnesNttDataKeypad, port, keyMappings)
{
	_turboSpeed = keyMappings.TurboSpeed;
}

string SnesNttDataKeypad::GetKeyNames()
{
	return "ABXYLRSTUDLR0123456789*#.CE";
}

void SnesNttDataKeypad::InternalSetStateFromInput()
{
	for(KeyMapping& keyMapping : _keyMappings) {
		SetPressedState(Buttons::A, keyMapping.A);
		SetPressedState(Buttons::B, keyMapping.B);
		SetPressedState(Buttons::X, keyMapping.X);
		SetPressedState(Buttons::Y, keyMapping.Y);
		SetPressedState(Buttons::L, keyMapping.L);
		SetPressedState(Buttons::R, keyMapping.R);
		SetPressedState(Buttons::Start, keyMapping.Start);
		SetPressedState(Buttons::Select, keyMapping.Select);
		SetPressedState(Buttons::Up, keyMapping.Up);
		SetPressedState(Buttons::Down, keyMapping.Down);
		SetPressedState(Buttons::Left, keyMapping.Left);
		SetPressedState(Buttons::Right, keyMapping.Right);
		SetPressedState(Buttons::Num0, keyMapping.CustomKeys[0]);
		SetPressedState(Buttons::Num1, keyMapping.CustomKeys[1]);
		SetPressedState(Buttons::Num2, keyMapping.CustomKeys[2]);
		SetPressedState(Buttons::Num3, keyMapping.CustomKeys[3]);
		SetPressedState(Buttons::Num4, keyMapping.CustomKeys[4]);
		SetPressedState(Buttons::Num5, keyMapping.CustomKeys[5]);
		SetPressedState(Buttons::Num6, keyMapping.CustomKeys[6]);
		SetPressedState(Buttons::Num7, keyMapping.CustomKeys[7]);
		SetPressedState(Buttons::Num8, keyMapping.CustomKeys[8]);
		SetPressedState(Buttons::Num9, keyMapping.CustomKeys[9]);
		SetPressedState(Buttons::Star, keyMapping.CustomKeys[10]);
		SetPressedState(Buttons::Pound, keyMapping.CustomKeys[11]);
		SetPressedState(Buttons::Period, keyMapping.CustomKeys[12]);
		SetPressedState(Buttons::C, keyMapping.CustomKeys[13]);
		SetPressedState(Buttons::EndCommunication, keyMapping.CustomKeys[14]);

		uint8_t turboFreq = 1 << (4 - _turboSpeed);
		bool turboOn = (uint8_t)(_emu->GetFrameCount() % turboFreq) < turboFreq / 2;
		if(turboOn) {
			SetPressedState(Buttons::A, keyMapping.TurboA);
			SetPressedState(Buttons::B, keyMapping.TurboB);
			SetPressedState(Buttons::X, keyMapping.TurboX);
			SetPressedState(Buttons::Y, keyMapping.TurboY);
			SetPressedState(Buttons::L, keyMapping.TurboL);
			SetPressedState(Buttons::R, keyMapping.TurboR);
		}

		bool allowInvalidInput = _emu->GetConsoleType() == ConsoleType::Nes ? _emu->GetSettings()->GetNesConfig().AllowInvalidInput : _emu->GetSettings()->GetSnesConfig().AllowInvalidInput;
		if(!allowInvalidInput) {
			//If both U+D or L+R are pressed at the same time, act as if neither is pressed
			if(IsPressed(Buttons::Up) && IsPressed(Buttons::Down)) {
				ClearBit(Buttons::Down);
				ClearBit(Buttons::Up);
			}
			if(IsPressed(Buttons::Left) && IsPressed(Buttons::Right)) {
				ClearBit(Buttons::Left);
				ClearBit(Buttons::Right);
			}
		}
	}
}

uint32_t SnesNttDataKeypad::ToNumber()
{
	//See https://snes.nesdev.org/wiki/NTT_Data_Keypad

	return ((uint8_t)IsPressed(Buttons::B) |
		((uint8_t)IsPressed(Buttons::Y) << 1) |
		((uint8_t)IsPressed(Buttons::Select) << 2) |
		((uint8_t)IsPressed(Buttons::Start) << 3) |
		((uint8_t)IsPressed(Buttons::Up) << 4) |
		((uint8_t)IsPressed(Buttons::Down) << 5) |
		((uint8_t)IsPressed(Buttons::Left) << 6) |
		((uint8_t)IsPressed(Buttons::Right) << 7) |
		((uint8_t)IsPressed(Buttons::A) << 8) |
		((uint8_t)IsPressed(Buttons::X) << 9) |
		((uint8_t)IsPressed(Buttons::L) << 10) |
		((uint8_t)IsPressed(Buttons::R) << 11) |
		(1 << 13) | // Signature to signal that this is the NTT Data Keypad
		((uint8_t)IsPressed(Buttons::Num0) << 16) |
		((uint8_t)IsPressed(Buttons::Num1) << 17) |
		((uint8_t)IsPressed(Buttons::Num2) << 18) |
		((uint8_t)IsPressed(Buttons::Num3) << 19) |
		((uint8_t)IsPressed(Buttons::Num4) << 20) |
		((uint8_t)IsPressed(Buttons::Num5) << 21) |
		((uint8_t)IsPressed(Buttons::Num6) << 22) |
		((uint8_t)IsPressed(Buttons::Num7) << 23) |
		((uint8_t)IsPressed(Buttons::Num8) << 24) |
		((uint8_t)IsPressed(Buttons::Num9) << 25) |
		((uint8_t)IsPressed(Buttons::Star) << 26) |
		((uint8_t)IsPressed(Buttons::Pound) << 27) |
		((uint8_t)IsPressed(Buttons::Period) << 28) |
		((uint8_t)IsPressed(Buttons::C) << 29) |
		((uint8_t)IsPressed(Buttons::EndCommunication) << 31));
}

void SnesNttDataKeypad::Serialize(Serializer& s)
{
	BaseControlDevice::Serialize(s);
	SV(_stateBuffer);
}

void SnesNttDataKeypad::RefreshStateBuffer()
{
	_stateBuffer = (uint32_t)ToNumber();
}

uint8_t SnesNttDataKeypad::ReadRam(uint16_t addr)
{
	uint8_t output = 0;

	if(IsCurrentPort(addr)) {
		StrobeProcessRead();
		output = _stateBuffer & 0x01;
		_stateBuffer >>= 1;

		//Bits past the first 32 are always 1
		_stateBuffer |= 0x80000000;
	}

	return output;
}

void SnesNttDataKeypad::WriteRam(uint16_t addr, uint8_t value)
{
	StrobeProcessWrite(value);
}

void SnesNttDataKeypad::InternalDrawController(InputHud& hud)
{
	hud.DrawOutline(35, 14);

	hud.DrawButton(5, 3, 3, 3, IsPressed(Buttons::Up));
	hud.DrawButton(5, 9, 3, 3, IsPressed(Buttons::Down));
	hud.DrawButton(2, 6, 3, 3, IsPressed(Buttons::Left));
	hud.DrawButton(8, 6, 3, 3, IsPressed(Buttons::Right));
	hud.DrawButton(5, 6, 3, 3, false);

	hud.DrawButton(27, 3, 3, 3, IsPressed(Buttons::X));
	hud.DrawButton(27, 9, 3, 3, IsPressed(Buttons::B));
	hud.DrawButton(30, 6, 3, 3, IsPressed(Buttons::A));
	hud.DrawButton(24, 6, 3, 3, IsPressed(Buttons::Y));

	hud.DrawButton(4, 0, 5, 2, IsPressed(Buttons::L));
	hud.DrawButton(26, 0, 5, 2, IsPressed(Buttons::R));

	hud.DrawButton(13, 9, 4, 2, IsPressed(Buttons::Select));
	hud.DrawButton(18, 9, 4, 2, IsPressed(Buttons::Start));

	hud.DrawNumber(hud.GetControllerIndex() + 1, 16, 2);
}

vector<DeviceButtonName> SnesNttDataKeypad::GetKeyNameAssociations()
{
	return {
		{ "a", Buttons::A },
		{ "b", Buttons::B },
		{ "x", Buttons::X },
		{ "y", Buttons::Y },
		{ "l", Buttons::L },
		{ "r", Buttons::R },
		{ "start", Buttons::Start },
		{ "select", Buttons::Select },
		{ "up", Buttons::Up },
		{ "down", Buttons::Down },
		{ "left", Buttons::Left },
		{ "right", Buttons::Right },
		{ "zero", Buttons::Num0 },
		{ "one", Buttons::Num1 },
		{ "two", Buttons::Num2 },
		{ "three", Buttons::Num3 },
		{ "four", Buttons::Num4 },
		{ "five", Buttons::Num5 },
		{ "six", Buttons::Num6 },
		{ "seven", Buttons::Num7 },
		{ "eight", Buttons::Num8 },
		{ "nine", Buttons::Num9 },
		{ "star", Buttons::Star },
		{ "pound", Buttons::Pound },
		{ "period", Buttons::Period },
		{ "c", Buttons::C },
		{ "end communication", Buttons::EndCommunication },
	};
}