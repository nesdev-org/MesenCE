#include "pch.h"
#include "WS/WsConsole.h"
#include "WS/WsControlManager.h"
#include "WS/WsMemoryManager.h"
#include "WS/Input/WsController.h"
#include "WS/Input/Pcv2Controller.h"
#include "Shared/EmuSettings.h"

WsControlManager::WsControlManager(Emulator* emu, WsConsole* console) : BaseControlManager(emu, CpuType::Ws)
{
	_console = console;
}

shared_ptr<BaseControlDevice> WsControlManager::CreateControllerDevice(ControllerType type, uint8_t port)
{
	shared_ptr<BaseControlDevice> device;

	WsConfig& cfg = _emu->GetSettings()->GetWsConfig();

	switch(type) {
		default:
		case ControllerType::None: break;

		case ControllerType::WsController:
		case ControllerType::WsControllerVertical:
			device.reset(new WsController(_emu, _console, port, cfg.ControllerHorizontal.Keys, cfg.ControllerVertical.Keys));
			break;

		case ControllerType::Pcv2Controller:
			device.reset(new Pcv2Controller(_emu, port, cfg.ControllerPcv2.Keys));
			break;
	}

	return device;
}

void WsControlManager::UpdateControlDevices()
{
	WsConfig cfg = _emu->GetSettings()->GetWsConfig();
	if(_emu->GetSettings()->IsEqual(_prevConfig, cfg) && _controlDevices.size() > 0) {
		//Do nothing if configuration is unchanged
		return;
	}

	auto lock = _deviceLock.AcquireSafe();

	ClearDevices();

	shared_ptr<BaseControlDevice> device(CreateControllerDevice(_console->GetModel() == WsModel::PocketChallenge ? ControllerType::Pcv2Controller : ControllerType::WsController, 0));
	if(device) {
		RegisterControlDevice(device);
	}
}

uint8_t WsControlManager::Read()
{
	uint8_t result = _state.InputSelect;

	for(shared_ptr<BaseControlDevice>& controller : _controlDevices) {
		if(controller->GetPort() != 0) {
			continue;
		}

		if(controller->GetControllerType() == ControllerType::WsController) {
			if(_state.InputSelect & 0x10) {
				result |= controller->IsPressed(WsController::Up2) ? 0x01 : 0;
				result |= controller->IsPressed(WsController::Right2) ? 0x02 : 0;
				result |= controller->IsPressed(WsController::Down2) ? 0x04 : 0;
				result |= controller->IsPressed(WsController::Left2) ? 0x08 : 0;
			}
			if(_state.InputSelect & 0x20) {
				result |= controller->IsPressed(WsController::Up) ? 0x01 : 0;
				result |= controller->IsPressed(WsController::Right) ? 0x02 : 0;
				result |= controller->IsPressed(WsController::Down) ? 0x04 : 0;
				result |= controller->IsPressed(WsController::Left) ? 0x08 : 0;
			}
			if(_state.InputSelect & 0x40) {
				result |= controller->IsPressed(WsController::Start) ? 0x02 : 0;
				result |= controller->IsPressed(WsController::A) ? 0x04 : 0;
				result |= controller->IsPressed(WsController::B) ? 0x08 : 0;
			}
		} else if(controller->GetControllerType() == ControllerType::Pcv2Controller) {
			result |= 0x02;
			if(_state.InputSelect & 0x10) {
				result |= controller->IsPressed(Pcv2Controller::Clear) ? 0x01 : 0;
				result |= controller->IsPressed(Pcv2Controller::Circle) ? 0x04 : 0;
				result |= controller->IsPressed(Pcv2Controller::Pass) ? 0x08 : 0;
			}
			if(_state.InputSelect & 0x20) {
				result |= controller->IsPressed(Pcv2Controller::View) ? 0x01 : 0;
				result |= controller->IsPressed(Pcv2Controller::Esc) ? 0x04 : 0;
				result |= controller->IsPressed(Pcv2Controller::Right) ? 0x08 : 0;
			}
			if(_state.InputSelect & 0x40) {
				result |= controller->IsPressed(Pcv2Controller::Left) ? 0x01 : 0;
				result |= controller->IsPressed(Pcv2Controller::Down) ? 0x04 : 0;
				result |= controller->IsPressed(Pcv2Controller::Up) ? 0x08 : 0;
			}
		}
	}

	return result;
}

void WsControlManager::Write(uint8_t value)
{
	_state.InputSelect = value & 0x70;
}

bool WsControlManager::IsSoundPressed()
{
	for(shared_ptr<BaseControlDevice>& controller : _controlDevices) {
		if(controller->GetPort() == 0 && controller->GetControllerType() == ControllerType::WsController) {
			return controller->IsPressed(WsController::Sound);
		}
	}
	return false;
}

void WsControlManager::UpdateInputState()
{
	BaseControlManager::UpdateInputState();

	uint8_t newInput = Read();
	if((_prevInput | newInput) > _prevInput) {
		//Trigger IRQ (on scanline 144) whenever any extra bits are set compared to the previous input
		_needKeyIrq = true;
	}
	_prevInput = newInput;

	bool soundButtonPressed = IsSoundPressed();
	if(soundButtonPressed && !_soundButtonPressed) {
		_console->GetApu()->ChangeMasterVolume();
	}
	_soundButtonPressed = soundButtonPressed;
}

void WsControlManager::TriggerKeyIrq()
{
	if(_needKeyIrq) {
		_console->GetMemoryManager()->SetIrqSource(WsIrqSource::KeyPressed);
		_needKeyIrq = false;
	}
}

void WsControlManager::Serialize(Serializer& s)
{
	SV(_state.InputSelect);
	SV(_prevInput);
	SV(_soundButtonPressed);
	SV(_needKeyIrq);
}
