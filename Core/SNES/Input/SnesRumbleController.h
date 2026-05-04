#pragma once
#include "pch.h"
#include "Shared/BaseControlDevice.h"
#include "SNES/Input/SnesController.h"
#include "Utilities/Serializer.h"

class Emulator;
class SnesConsole;

class SnesRumbleController : public SnesController
{
private:
	SnesConsole* _console = nullptr;
	uint16_t _rumbleData = 0;
	bool _rumbleActive = false;
	uint32_t _lastRumbleFrame = 0;

protected:
	void Serialize(Serializer& s) override;
	void RefreshStateBuffer() override;

public:
	SnesRumbleController(Emulator* emu, SnesConsole* console, uint8_t port, KeyMappingSet keyMappings);
	virtual ~SnesRumbleController();

	uint8_t ReadRam(uint16_t addr) override;
};