#pragma once
#include "pch.h"
#include "NES/Input/NesController.h"

class FcnsController : public NesController
{
private:
	uint32_t _fcnsState = 0; // 24 bits

protected:
	enum FcnsButtons
	{
		Num0 = 8,
		Num1,
		Num2,
		Num3,
		Num4,
		Num5,
		Num6,
		Num7,
		Num8,
		Num9,
		Star,
		Pound,
		Period,
		C,
		EndCommunication
	};

	void Serialize(Serializer& s) override
	{
		NesController::Serialize(s);
		SV(_fcnsState);
	}

	void InternalSetStateFromInput() override
	{
		NesController::InternalSetStateFromInput();

		for(KeyMapping& keyMapping : _keyMappings) {
			SetPressedState(FcnsButtons::Num0, keyMapping.CustomKeys[0]);
			SetPressedState(FcnsButtons::Num1, keyMapping.CustomKeys[1]);
			SetPressedState(FcnsButtons::Num2, keyMapping.CustomKeys[2]);
			SetPressedState(FcnsButtons::Num3, keyMapping.CustomKeys[3]);
			SetPressedState(FcnsButtons::Num4, keyMapping.CustomKeys[4]);
			SetPressedState(FcnsButtons::Num5, keyMapping.CustomKeys[5]);
			SetPressedState(FcnsButtons::Num6, keyMapping.CustomKeys[6]);
			SetPressedState(FcnsButtons::Num7, keyMapping.CustomKeys[7]);
			SetPressedState(FcnsButtons::Num8, keyMapping.CustomKeys[8]);
			SetPressedState(FcnsButtons::Num9, keyMapping.CustomKeys[9]);
			SetPressedState(FcnsButtons::Star, keyMapping.CustomKeys[10]);
			SetPressedState(FcnsButtons::Pound, keyMapping.CustomKeys[11]);
			SetPressedState(FcnsButtons::Period, keyMapping.CustomKeys[12]);
			SetPressedState(FcnsButtons::C, keyMapping.CustomKeys[13]);
			SetPressedState(FcnsButtons::EndCommunication, keyMapping.CustomKeys[14]);
		}
	}

public:
	FcnsController(Emulator* emu, uint8_t port, KeyMappingSet keyMappings) : NesController(emu, ControllerType::FcnsController, port, keyMappings)
	{
	}

	uint8_t ReadRam(uint16_t addr) override
	{
		uint8_t output = 0;
		if(addr == 0x4016) {
			StrobeProcessRead();
			output = (_fcnsState & 0x01);
			if(_port >= 2) {
				output <<= 1;
			}
			_fcnsState >>= 1;
			_fcnsState |= 0x800000;
		}
		return output;
	}

	void RefreshStateBuffer() override
	{
		uint32_t fcnsData = ((uint8_t)IsPressed(FcnsButtons::Num0) << 8) |
			((uint8_t)IsPressed(FcnsButtons::Num1) << 9) |
			((uint8_t)IsPressed(FcnsButtons::Num2) << 10) |
			((uint8_t)IsPressed(FcnsButtons::Num3) << 11) |
			((uint8_t)IsPressed(FcnsButtons::Num4) << 12) |
			((uint8_t)IsPressed(FcnsButtons::Num5) << 13) |
			((uint8_t)IsPressed(FcnsButtons::Num6) << 14) |
			((uint8_t)IsPressed(FcnsButtons::Num7) << 15) |
			((uint8_t)IsPressed(FcnsButtons::Num8) << 16) |
			((uint8_t)IsPressed(FcnsButtons::Num9) << 17) |
			((uint8_t)IsPressed(FcnsButtons::Star) << 18) |
			((uint8_t)IsPressed(FcnsButtons::Pound) << 19) |
			((uint8_t)IsPressed(FcnsButtons::Period) << 20) |
			((uint8_t)IsPressed(FcnsButtons::C) << 21) |
			((uint8_t)IsPressed(FcnsButtons::EndCommunication) << 23);

		NesController::RefreshStateBuffer();
		_fcnsState = (GetControllerStateBuffer() & 0xFF) | fcnsData;
	}
};
