#pragma once
#include "pch.h"
#include "WS/WsConsole.h"
#include "Shared/BaseControlDevice.h"
#include "Shared/Emulator.h"
#include "Shared/EmuSettings.h"
#include "Shared/InputHud.h"
#include "Utilities/Serializer.h"

class Pcv2Controller : public BaseControlDevice
{
private:
	WsConsole* _console = nullptr;

protected:
	string GetKeyNames() override
	{
		return "udlrepcCv";
	}

	void InternalSetStateFromInput() override
	{
		for(KeyMapping& keyMapping : _keyMappings) {
			for(int i = Buttons::Up; i <= Buttons::View; i++) {
				SetPressedState(i, keyMapping.CustomKeys[i]);
			}
		}
	}

	void RefreshStateBuffer() override
	{
	}

public:
	enum Buttons
	{
		Up = 0,
		Down,
		Left,
		Right,
		Esc,
		Pass,
		Circle,
		Clear,
		View
	};

	Pcv2Controller(Emulator* emu, WsConsole* console, uint8_t port, KeyMappingSet mappings) : BaseControlDevice(emu, ControllerType::Pcv2Controller, port, mappings)
	{
		_console = console;
	}

	uint8_t ReadRam(uint16_t addr) override
	{
		return 0;
	}

	void WriteRam(uint16_t addr, uint8_t value) override
	{
	}

	void InternalDrawController(InputHud& hud) override
	{
		// TODOWS
	}

	vector<DeviceButtonName> GetKeyNameAssociations() override
	{
		return {
			{ "up", Buttons::Up },
			{ "down", Buttons::Down },
			{ "left", Buttons::Left },
			{ "right", Buttons::Right },
			{ "esc", Buttons::Esc },
			{ "pass", Buttons::Pass },
			{ "circle", Buttons::Circle },
			{ "clear", Buttons::Clear },
			{ "view", Buttons::View },
		};
	}
};