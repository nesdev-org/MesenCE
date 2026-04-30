#pragma once
#include "pch.h"
#include <functional>
#include "Shared/BaseControlManager.h"
#include "Shared/Interfaces/IInputProvider.h"
#include "Shared/SettingTypes.h"

class Emulator;
class Gameboy;
class BaseControlDevice;

class GbControlManager final : public BaseControlManager, public IInputProvider
{
private:
	Emulator* _emu = nullptr;
	Gameboy* _console = nullptr;
	GameboyConfig _prevConfig = {};
	GbControlManagerState _state = {};

public:
	GbControlManager(Emulator* emu, Gameboy* console);
	~GbControlManager();

	GbControlManagerState GetState();

	shared_ptr<BaseControlDevice> CreateControllerDevice(ControllerType type, uint8_t port) override;
	void UpdateControlDevices() override;
	uint8_t ReadInputPort();
	void WriteInputPort(uint8_t value);

	void ProcessInputChange(std::function<void()> inputUpdateCallback);

	void UpdateInputState() override;
	
	bool SetInput(BaseControlDevice* device) override;

	void Serialize(Serializer& s) override;
};