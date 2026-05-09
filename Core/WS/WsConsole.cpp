#include "WS/Carts/WsCartFlash.h"
#include "pch.h"
#include "WS/WsConsole.h"
#include "WS/WsCpu.h"
#include "WS/WsPpu.h"
#include "WS/WsTimer.h"
#include "WS/Carts/WsCart.h"
#include "WS/Carts/WsCartFlash.h"
#include "WS/Carts/WsRtc.h"
#include "WS/WsControlManager.h"
#include "WS/WsMemoryManager.h"
#include "WS/WsDmaController.h"
#include "WS/WsEeprom.h"
#include "WS/WsSerial.h"
#include "WS/WsDefaultVideoFilter.h"
#include "WS/APU/WsApu.h"
#include "Shared/CpuType.h"
#include "Shared/Emulator.h"
#include "Shared/SettingTypes.h"
#include "Shared/FirmwareHelper.h"
#include "Shared/BatteryManager.h"

WsConsole::WsConsole(Emulator* emu)
{
	_emu = emu;
}

WsConsole::~WsConsole()
{
	delete[] _workRam;
	delete[] _saveRam;
	delete[] _prgRom;
	delete[] _bootRom;
	delete[] _internalEepromData;
	delete[] _cartEepromData;
}

bool WsConsole::IsWWCart()
{
	//TODOWS exclude static FreyaBIOS cartridges
	return _prgRomSize == 0x80000
		&& _prgRom[0x70000] == 'E'   // FreyaBIOS header
		&& _prgRom[0x70001] == 'L'
		&& _prgRom[0x70002] == 'I'
		&& _prgRom[0x70003] == 'S'
		&& _prgRom[0x70004] == 'A'
		&& _prgRom[0x7fff6] == 0x00  // Publisher ID
		&& _prgRom[0x7fff8] == 0x00  // Game ID
		&& _prgRom[0x7fffb] == 0x04  // Save format
		&& _prgRom[0x7fffd] == 0x01; // Mapper
}

LoadRomResult WsConsole::LoadRom(VirtualFile& romFile)
{
	vector<uint8_t> romData;
	romFile.ReadFile(romData);

	if(romData.size() < 0x10000) {
		return LoadRomResult::Failure;
	}

	uint32_t power = (uint32_t)std::log2(romData.size());
	if(romData.size() > ((uint64_t)1 << power)) {
		//If size isn't a power of 2, pad the beginning of the ROM to the next power of 2
		uint32_t newSize = 1 << (power + 1);
		romData.insert(romData.begin(), newSize - romData.size(), 0);
	}

	MessageManager::Log("------------------------------");

	_prgRomSize = (uint32_t)romData.size();
	_prgRom = new uint8_t[_prgRomSize];
	memcpy(_prgRom, romData.data(), _prgRomSize);
	_emu->RegisterMemory(MemoryType::WsPrgRom, _prgRom, _prgRomSize);

	uint8_t sramType = _prgRom[_prgRomSize - 5];
	switch(sramType) {
		case 0x00: _saveRamSize = 0; break;
		case 0x01: _saveRamSize = 32 * 1024; break;
		case 0x02: _saveRamSize = 32 * 1024; break;
		case 0x03: _saveRamSize = 128 * 1024; break;
		case 0x04: _saveRamSize = 256 * 1024; break;
		case 0x05: _saveRamSize = 512 * 1024; break;

		case 0x10: _cartEepromSize = 0x80; break;
		case 0x20: _cartEepromSize = 0x800; break;
		case 0x50: _cartEepromSize = 0x400; break;

		default: MessageManager::Log("Save RAM: unrecognized value"); break;
	}

	bool hasColorSupport = (_prgRom[_prgRomSize - 9] & 0x01) || romFile.GetFileExtension() == ".wsc";
	uint8_t mapperType = _prgRom[_prgRomSize - 3];

	MessageManager::Log(string("Color supported: ") + (hasColorSupport ? "Yes" : "No"));
	MessageManager::Log("Save RAM size: " + std::to_string(_saveRamSize / 1024) + " KB");
	MessageManager::Log("Cart EEPROM size: " + std::to_string(_cartEepromSize) + " bytes");
	MessageManager::Log(string("Mapper: ") + (IsWWCart() ? "Bandai 2003 + NOR flash" : (mapperType == 0 ? "Bandai 2001 / KARNAK" : (mapperType == 1 ? "Bandai 2003" : ("Unknown: " + std::to_string(mapperType))))));

	MessageManager::Log("------------------------------");

	if(_saveRamSize > 0) {
		_saveRam = new uint8_t[_saveRamSize];
		memset(_saveRam, 0, _saveRamSize);
		_emu->RegisterMemory(MemoryType::WsCartRam, _saveRam, _saveRamSize);
	}

	if(_cartEepromSize > 0) {
		_cartEepromData = new uint8_t[_cartEepromSize];
		memset(_cartEepromData, 0, _cartEepromSize);
		_emu->RegisterMemory(MemoryType::WsCartEeprom, _cartEepromData, _cartEepromSize);
		_cartEeprom.reset(new WsEeprom(_emu, this, (WsEepromSize)_cartEepromSize, _cartEepromData, false));
	}

	if(mapperType >= 0x01) {
		_cartRtc.reset(new WsRtc(_emu, this));
	}

	_model = _emu->GetSettings()->GetWsConfig().Model;
	if(_model == WsModel::Auto) {
		_model = romFile.GetFileExtension() == ".pc2" ? WsModel::PocketChallenge : (hasColorSupport ? WsModel::Color : WsModel::Monochrome);
	}
	_colorModel = _model == WsModel::Color || _model == WsModel::SwanCrystal;

	_workRamSize = _colorModel ? 0x10000 : 0x4000;
	_workRam = new uint8_t[_workRamSize];
	memset(_workRam, 0, _workRamSize);
	_emu->RegisterMemory(MemoryType::WsWorkRam, _workRam, _workRamSize);

	vector<uint8_t> bootRom;
	if(_emu->GetSettings()->GetWsConfig().UseBootRom && FirmwareHelper::LoadWsBootRom(_emu, bootRom, _model)) {
		_bootRomSize = (uint32_t)bootRom.size();
		_bootRom = new uint8_t[_bootRomSize];
		memcpy(_bootRom, bootRom.data(), _bootRomSize);
		_emu->RegisterMemory(MemoryType::WsBootRom, _bootRom, _bootRomSize);
	}

	switch(_model) {
		case WsModel::PocketChallenge: _internalEepromSize = (uint32_t)WsEepromSize::Size0; break;
		case WsModel::Monochrome: _internalEepromSize = (uint32_t)WsEepromSize::Size128; break;
		default: _internalEepromSize = (uint32_t)WsEepromSize::Size2kb; break;
	}
	if(_internalEepromSize) {
		_internalEepromData = new uint8_t[_internalEepromSize];
		memset(_internalEepromData, 0, _internalEepromSize);
		_emu->RegisterMemory(MemoryType::WsInternalEeprom, _internalEepromData, _internalEepromSize);
	}

	_internalEeprom.reset(new WsEeprom(_emu, this, (WsEepromSize)_internalEepromSize, _internalEepromData, true));

	_controlManager.reset(new WsControlManager(_emu, this));
	_memoryManager.reset(new WsMemoryManager());
	_cpu.reset(new WsCpu(_emu, this, _memoryManager.get()));
	_timer.reset(new WsTimer());
	_serial.reset(new WsSerial(this));
	_dmaController.reset(new WsDmaController());
	_ppu.reset(new WsPpu(_emu, this, _memoryManager.get(), _timer.get(), _workRam));
	_apu.reset(new WsApu(_emu, this, _memoryManager.get(), _dmaController.get()));
	_cart.reset(IsWWCart() ? new WsCartFlash() : new WsCart());

	_cart->Init(_memoryManager.get(), _cartEeprom.get(), _cartRtc.get());
	_memoryManager->Init(_emu, this, _cpu.get(), _ppu.get(), _controlManager.get(), _cart.get(), _timer.get(), _dmaController.get(), _internalEeprom.get(), _apu.get(), _serial.get());
	_timer->Init(_memoryManager.get());
	_dmaController->Init(_memoryManager.get(), _apu.get());

	if(!_bootRom) {
		InitPostBootRomState();
	}

	LoadBattery();

	return LoadRomResult::Success;
}

void WsConsole::RunFrame()
{
	uint32_t frameCount = _ppu->GetFrameCount();
	while(frameCount == _ppu->GetFrameCount()) {
		_cpu->Exec();
	}

	_apu->PlayQueuedAudio();

	//Make sure to process writes to EEPROM once a frame at least (so that the update is visible in the memory viewer)
	_internalEeprom->Run();
	if(_cartEeprom) {
		_cartEeprom->Run();
	}

	_verticalMode |= _ppu->GetState().Icons.Vertical;
	if(_ppu->GetState().Icons.Horizontal) {
		_verticalMode = false;
	}
}

void WsConsole::GetScreenRotationOverride(uint32_t& rotation)
{
	if(_emu->GetSettings()->GetWsConfig().AutoRotate) {
		if(_verticalMode) {
			rotation = 270;
		} else {
			rotation = 0;
		}
	}
}

bool WsConsole::IsColorMode()
{
	return _memoryManager->GetState().ColorEnabled;
}

bool WsConsole::IsColorModel()
{
	return _colorModel;
}

bool WsConsole::IsPowerOff()
{
	return _cpu->IsPowerOff();
}

bool WsConsole::IsVerticalMode()
{
	return _verticalMode;
}

WsAudioMode WsConsole::GetAudioMode()
{
	// PCv2 always uses the headphone port for sound output
	if(_model == WsModel::PocketChallenge) {
		return WsAudioMode::Headphones;
	}
	return _emu->GetSettings()->GetWsConfig().AudioMode;
}

WsModel WsConsole::GetModel()
{
	return _model;
}

void WsConsole::ProcessEndOfFrame()
{
	_controlManager->UpdateControlDevices();
	_controlManager->UpdateInputState();
}

void WsConsole::Reset()
{
	//The WS has no reset button, behave like power cycle
	_emu->ReloadRom(true);
}

void WsConsole::InitPostBootRomState()
{
	//Init work ram
	if(_colorModel) {
		memset(_workRam, 0, 0xFE00);
		memset(_workRam + 0xFE00, 0xFF, 0x200);
	} else {
		memset(_workRam, 0, _workRamSize);
	}

	//Init port state
	WsCpuState& cpu = _cpu->GetState();
	cpu.CS = 0xFFFF;
	cpu.SP = 0x2000;

	if(_model == WsModel::PocketChallenge) {
		// The Pocket Challenge skips most of the boot ROM.
		// TODOWS: Figure out scanline, cycle, any missing port writes.
		cpu.CS = 0x4000;
		cpu.IP = 0x0010;
		return;
	} else if(_model == WsModel::Monochrome) {
		cpu.AX = 0xFF85;
		cpu.BX = 0x0040;
		cpu.DX = 0x0005;
		cpu.SI = 0x023D;
		cpu.DI = 0x040D;
		cpu.DS = 0xFF00;
		cpu.Flags.Sign = true;
	} else {
		cpu.AX = 0xFF87;
		cpu.BX = 0x0043;
		cpu.DX = 0x0001;
		cpu.SI = 0x0435;
		cpu.DI = 0x040B;
		cpu.DS = 0xFE00;
		cpu.Flags.Sign = true;
		cpu.Flags.Parity = true;
	}

	WsPpuState& ppu = _ppu->GetState();
	ppu.LcdEnabled = true;
	if(_colorModel) {
		ppu.Scanline = 127;
		ppu.Cycle = 232;
	} else {
		ppu.Scanline = 26;
		ppu.Cycle = 53;

		// clang-format off
		constexpr static uint8_t defaultBwPalette[] = {
			0,0,0,7,0,0,0,5,0,0,0,4,0,0,0,3, 
			0,0,0,2,0,0,0,1,0,0,0,0,0,0,0,0,
			0,3,5,7,0,0,0,0,0,0,0,0,0,0,0,0,
			0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
		};
		// clang-format on

		constexpr static uint8_t defaultShades[] = { 0, 2, 4, 6, 8, 10, 12, 15 };

		memcpy(ppu.BwPalettes, defaultBwPalette, sizeof(defaultBwPalette));
		memcpy(ppu.BwShades, defaultShades, sizeof(defaultShades));
	}

	_ppu->SetOutputToBgColor();

	_controlManager->Write(0x40);

	WsMemoryManagerState& mm = _memoryManager->GetState();
	mm.BootRomDisabled = true;

	//Apply flags from rom header
	_verticalMode = (_prgRom[_prgRomSize - 4] & 0x01) != 0;
	mm.CartWordBus = (_prgRom[_prgRomSize - 4] & 0x04) != 0;
	mm.SlowRom = (_prgRom[_prgRomSize - 4] & 0x08) != 0;

	WsApuState& apu = _apu->GetState();
	if(_colorModel) {
		apu.InternalMasterVolume = _internalEepromData[0x83] & 0x03;
		ppu.HighContrast = (_internalEepromData[0x83] & 0x40);
	} else {
		apu.Ch1.Frequency = 0x7E6;
		apu.Ch2.Frequency = 0x7E6;
	}

	WsEepromState& eeprom = _internalEeprom->GetState();
	eeprom.ReadDone = true;
	eeprom.Idle = true;
	eeprom.InternalEepromWriteProtected = true;

	//Copy data from rom header to internal eeprom, like the boot rom would
	for(int i = 0; i < 3; i++) {
		_internalEepromData[0x76 + i] = _prgRom[_prgRomSize - 0x10 + 6 + i];

		bool supportsColor = _prgRom[_prgRomSize - 0x10 + 7] & 0x01;
		if(_colorModel && supportsColor) {
			//For games that support color & color models, copy 76-78 to 80-82
			_internalEepromData[0x80 + i] = _internalEepromData[0x76 + i];
		}
	}

	_memoryManager->RefreshMappings();
}

void WsConsole::LoadBattery()
{
	_internalEeprom->LoadBattery();
	if(_cartEeprom) {
		_cartEeprom->LoadBattery();
	}
	if(_cartRtc) {
		_cartRtc->LoadBattery();
	}

	if(_saveRam) {
		_emu->GetBatteryManager()->LoadBattery(".sav", _saveRam, _saveRamSize);
	}
}

void WsConsole::SaveBattery()
{
	_internalEeprom->SaveBattery();
	if(_cartEeprom) {
		_cartEeprom->SaveBattery();
	}
	if(_cartRtc) {
		_cartRtc->SaveBattery();
	}

	if(_saveRam) {
		_emu->GetBatteryManager()->SaveBattery(".sav", _saveRam, _saveRamSize);
	}
}

BaseControlManager* WsConsole::GetControlManager()
{
	return _controlManager.get();
}

ConsoleRegion WsConsole::GetRegion()
{
	return ConsoleRegion::Ntsc;
}

ConsoleType WsConsole::GetConsoleType()
{
	return ConsoleType::Ws;
}

vector<CpuType> WsConsole::GetCpuTypes()
{
	return { CpuType::Ws };
}

uint64_t WsConsole::GetCartridgeClock()
{
	return _cpu->GetCycleCount() / 8;
}

uint64_t WsConsole::GetMasterClock()
{
	return _cpu->GetCycleCount();
}

uint32_t WsConsole::GetMasterClockRate()
{
	return 12288000 / 4;
}

double WsConsole::GetFps()
{
	return 12000.0 / _ppu->GetScanlineCount();
}

BaseVideoFilter* WsConsole::GetVideoFilter(bool getDefaultFilter)
{
	if(getDefaultFilter) {
		return new WsDefaultVideoFilter(_emu, this, false);
	}

	VideoFilterType filterType = _emu->GetSettings()->GetVideoConfig().VideoFilter;

	switch(filterType) {
		case VideoFilterType::NtscBisqwit:
		case VideoFilterType::NtscBlargg:
			return new WsDefaultVideoFilter(_emu, this, true);

		default: return new WsDefaultVideoFilter(_emu, this, false);
	}
}

PpuFrameInfo WsConsole::GetPpuFrame()
{
	PpuFrameInfo frame = {};
	frame.FirstScanline = 0;
	frame.FrameCount = _ppu->GetFrameCount();
	frame.Width = _ppu->GetScreenWidth();
	frame.Height = _ppu->GetScreenHeight();
	frame.ScanlineCount = WsConstants::ScanlineCount;
	frame.CycleCount = WsConstants::ClocksPerScanline;
	frame.FrameBufferSize = frame.Width * frame.Height * sizeof(uint16_t);
	frame.FrameBuffer = (uint8_t*)_ppu->GetScreenBuffer(false);
	return frame;
}

RomFormat WsConsole::GetRomFormat()
{
	return RomFormat::Ws;
}

AudioTrackInfo WsConsole::GetAudioTrackInfo()
{
	//TODOWS
	return AudioTrackInfo();
}

void WsConsole::ProcessAudioPlayerAction(AudioPlayerActionParams p)
{
	//TODOWS
}

AddressInfo WsConsole::GetAbsoluteAddress(uint32_t relAddress)
{
	return _memoryManager->GetAbsoluteAddress(relAddress);
}

AddressInfo WsConsole::GetAbsoluteAddress(AddressInfo& relAddress)
{
	if(relAddress.Type == MemoryType::WsPort) {
		return { relAddress.Address & 0xFFFF, MemoryType::WsPort };
	} else {
		return GetAbsoluteAddress(relAddress.Address);
	}
}

AddressInfo WsConsole::GetRelativeAddress(AddressInfo& absAddress, CpuType cpuType)
{
	return { _memoryManager->GetRelativeAddress(absAddress), MemoryType::WsMemory };
}

WsState WsConsole::GetState()
{
	WsState state = {};
	state.Model = _model;
	state.Cpu = _cpu->GetState();
	state.Ppu = _ppu->GetState();
	state.Apu = _apu->GetState();
	state.MemoryManager = _memoryManager->GetState();
	state.ControlManager = _controlManager->GetState();
	state.DmaController = _dmaController->GetState();
	state.Timer = _timer->GetState();
	state.Serial = _serial->GetState();
	state.InternalEeprom = _internalEeprom->GetState();
	state.Cart = _cart->GetState();
	if(_cartEeprom) {
		state.CartEeprom = _cartEeprom->GetState();
	}
	if(_cartRtc) {
		state.CartRtc = _cartRtc->GetState();
	}
	return state;
}

void WsConsole::GetConsoleState(BaseState& state, ConsoleType consoleType)
{
	(WsState&)state = GetState();
}

static WsModel GetModelForCompatibilityCheck(WsModel model)
{
	if(model == WsModel::SwanCrystal) {
		//WSC and SC save states are currently interchangeable.
		return WsModel::Color;
	}
	return model;
}

static string GetModelShortName(WsModel model)
{
	switch(model) {
		case WsModel::Color:
		case WsModel::SwanCrystal:
			return "WSC";
		case WsModel::PocketChallenge:
			return "PCv2";
		default:
			return "WS";
	}
}

void WsConsole::Serialize(Serializer& s)
{
	WsModel model = _model;
	SV(model);
	if(!s.IsSaving() && GetModelForCompatibilityCheck(model) != GetModelForCompatibilityCheck(_model)) {
		MessageManager::DisplayMessage("SaveStates", "Can't load " + GetModelShortName(model) + " state in " + GetModelShortName(_model) + " mode.");
		s.SetErrorFlag();
		return;
	}

	SV(_cpu);
	SV(_ppu);
	SV(_apu);

	//Process cart before memory manager to ensure mappings are updated properly
	SV(_cart);
	SV(_memoryManager);

	SV(_dmaController);
	SV(_controlManager);
	SV(_timer);
	SV(_serial);
	SV(_internalEeprom);

	SV(_verticalMode);

	SVArray(_workRam, _workRamSize);
	if(_saveRam && _saveRamSize) {
		SVArray(_saveRam, _saveRamSize);
	}
	if(_internalEepromData && _internalEepromSize) {
		SVArray(_internalEepromData, _internalEepromSize);
	}
	if(_cartEepromData && _cartEepromSize) {
		SVArray(_cartEepromData, _cartEepromSize);
	}

	if(_cartEeprom) {
		SV(_cartEeprom);
	}
	if(_cartRtc) {
		SV(_cartRtc);
	}
}
