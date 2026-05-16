#pragma once
#include "pch.h"
#include "Shared/BaseControlDevice.h"
#include "Utilities/Serializer.h"

class SnesNttDataKeypad : public BaseControlDevice
{
private:
	uint32_t _stateBuffer = 0;
	uint8_t _turboSpeed = 0;

protected:
	string GetKeyNames() override;
	void InternalSetStateFromInput() override;
	uint32_t ToNumber();
	void Serialize(Serializer& s) override;
	void RefreshStateBuffer() override;

public:
	enum Buttons
	{
		A = 0,
		B,
		X,
		Y,
		L,
		R,
		Select,
		Start,
		Up,
		Down,
		Left,
		Right,
		Num0,
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

	SnesNttDataKeypad(Emulator* emu, uint8_t port, KeyMappingSet keyMappings);

	uint8_t ReadRam(uint16_t addr) override;
	void WriteRam(uint16_t addr, uint8_t value) override;

	void InternalDrawController(InputHud& hud) override;
	vector<DeviceButtonName> GetKeyNameAssociations() override;
};