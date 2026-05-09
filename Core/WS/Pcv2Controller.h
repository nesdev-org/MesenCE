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
		hud.DrawOutline(35, 16);

		hud.DrawButton(5, 5, 3, 3, IsPressed(Buttons::Up));
		hud.DrawButton(5, 11, 3, 3, IsPressed(Buttons::Down));
		hud.DrawButton(2, 8, 3, 3, IsPressed(Buttons::Left));
		hud.DrawButton(8, 8, 3, 3, IsPressed(Buttons::Right));
		hud.DrawButton(9, 2, 2, 2, IsPressed(Buttons::Esc));

		hud.DrawButton(30, 5, 3, 3, IsPressed(Buttons::Clear));
		hud.DrawButton(27, 8, 3, 3, IsPressed(Buttons::Circle));
		hud.DrawButton(24, 11, 3, 3, IsPressed(Buttons::Pass));
		hud.DrawButton(24, 2, 2, 2, IsPressed(Buttons::View));

		hud.DrawNumber(_port + 1, 16, 2);
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