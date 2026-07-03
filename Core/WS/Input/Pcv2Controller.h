#pragma once
#include "pch.h"
#include "Shared/BaseControlDevice.h"
#include "Shared/Emulator.h"
#include "Shared/InputHud.h"

class Pcv2Controller : public BaseControlDevice
{
private:
	uint32_t _turboSpeed = 0;

protected:
	string GetKeyNames() override
	{
		return "udlrepcCv";
	}

	void InternalSetStateFromInput() override
	{
		bool turboOn = IsTurboOn(_turboSpeed);

		for(KeyMapping& keyMapping : _keyMappings) {
			for(int i = Buttons::Up; i <= Buttons::View; i++) {
				SetPressedState(Buttons::Clear, keyMapping.A);
				SetPressedState(Buttons::Circle, keyMapping.B);
				SetPressedState(Buttons::Pass, keyMapping.X);
				SetPressedState(Buttons::Esc, keyMapping.Select);
				SetPressedState(Buttons::View, keyMapping.Start);
				SetPressedState(Buttons::Up, keyMapping.Up);
				SetPressedState(Buttons::Down, keyMapping.Down);
				SetPressedState(Buttons::Left, keyMapping.Left);
				SetPressedState(Buttons::Right, keyMapping.Right);
			}

			if(turboOn) {
				SetPressedState(Buttons::Clear, keyMapping.TurboA);
				SetPressedState(Buttons::Circle, keyMapping.TurboB);
				SetPressedState(Buttons::Pass, keyMapping.TurboX);
			}
		}
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

	Pcv2Controller(Emulator* emu, uint8_t port, KeyMappingSet keyMappings) : BaseControlDevice(emu, ControllerType::Pcv2Controller, port, keyMappings)
	{
		_turboSpeed = keyMappings.TurboSpeed;
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