#include "pch.h"
#include "SNES/Input/SnesRumbleController.h"
#include "SNES/SnesConsole.h"
#include "SNES/InternalRegisters.h"
#include "Shared/Emulator.h"
#include "Shared/EmuSettings.h"
#include "Shared/KeyManager.h"

SnesRumbleController::SnesRumbleController(Emulator* emu, SnesConsole* console, uint8_t port, KeyMappingSet keyMappings) : SnesController(emu, port, keyMappings)
{
	_console = console;
	_type = ControllerType::SnesRumbleController;
}

SnesRumbleController::~SnesRumbleController()
{
	KeyManager::SetForceFeedback(0, 0);
}

void SnesRumbleController::Serialize(Serializer& s)
{
	SnesController::Serialize(s);
	SV(_rumbleData);
	SV(_rumbleActive);
	SV(_lastRumbleFrame);
}

void SnesRumbleController::RefreshStateBuffer()
{
	_rumbleData = 0;

	//The LRG rumble controller stops rumbling shortly after the last rumble command.
	//Not accurate, assumes the game regularly latches the controller.
	if(_rumbleActive) {
		if(_emu->GetFrameCount() - _lastRumbleFrame > 2) {
			KeyManager::SetForceFeedback(0, 0);
			_rumbleActive = false;
		}
	}

	SnesController::RefreshStateBuffer();
}

uint8_t SnesRumbleController::ReadRam(uint16_t addr)
{
	if(IsCurrentPort(addr)) {
		uint8_t ioPort = _console->GetInternalRegisters()->GetIoPortOutput();

		//Technically, when plugged into port 2, this uses bit 7, but activating the rumble
		//in both ports at the same time could potentially draw too much current, so the
		//UI current prevents picking a rumble controller for P2
		uint8_t ioBit = (GetPort() == 0 ? (ioPort >> 6) : (ioPort >> 7)) & 0x01;

		_rumbleData = (_rumbleData << 1) | ioBit;

		if((_rumbleData & 0xFF00) == 0x7200) {
			uint8_t rumble = _rumbleData & 0xFF;

			//Multiply by 4369 to use all values from 0 to 65535
			uint16_t rightRumble = (rumble >> 4) * 4369;
			uint16_t leftRumble = (rumble & 0x0F) * 4369;

			KeyManager::SetForceFeedback(rightRumble, leftRumble);

			_rumbleActive = true;
			_lastRumbleFrame = _emu->GetFrameCount();
		}
	}

	return SnesController::ReadRam(addr);
}
