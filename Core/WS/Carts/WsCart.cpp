#include "pch.h"
#include "WS/Carts/WsCart.h"
#include "WS/WsMemoryManager.h"
#include "Shared/BatteryManager.h"
#include "Shared/Emulator.h"
#include "Utilities/Serializer.h"

WsCart::WsCart()
{
	_state.SelectedBanks[0] = 0x3F;
	_state.SelectedBanks[1] = 0xFF;
	_state.SelectedBanks[2] = 0xFF;
	_state.SelectedBanks[3] = 0xFF;
}

void WsCart::LoadBattery()
{
	if(_saveRam) {
		_emu->GetBatteryManager()->LoadBattery(".sav", _saveRam, _saveRamSize);
	}
}

void WsCart::SaveBattery()
{
	if(_saveRam) {
		_emu->GetBatteryManager()->SaveBattery(".sav", _saveRam, _saveRamSize);
	}
}

void WsCart::Init(Emulator* emu, WsCartType cartType, WsMemoryManager* memoryManager, uint8_t* prgRom, uint32_t prgRomSize, uint8_t* saveRam, uint32_t saveRamSize)
{
	_emu = emu;
	_memoryManager = memoryManager;
	_state.CartType = cartType;
	_prgRom = prgRom;
	_prgRomSize = prgRomSize;
	_saveRam = saveRam;
	_saveRamSize = saveRamSize;
}

void WsCart::Map(uint32_t start, uint32_t end, MemoryType type, uint32_t offset, bool readonly)
{
	_memoryManager->Map(start, end, type, offset, readonly);
}

void WsCart::Unmap(uint32_t start, uint32_t end)
{
	_memoryManager->Unmap(start, end);
}

void WsCart::RefreshMappings()
{
	Map(0x10000, 0x1FFFF, MemoryType::WsCartRam, _state.SelectedBanks[1] * 0x10000, false);
	Map(0x20000, 0x2FFFF, MemoryType::WsPrgRom, _state.SelectedBanks[2] * 0x10000, true);
	Map(0x30000, 0x3FFFF, MemoryType::WsPrgRom, _state.SelectedBanks[3] * 0x10000, true);
	Map(0x40000, 0xFFFFF, MemoryType::WsPrgRom, _state.SelectedBanks[0] * 0x100000 + 0x40000, true);
}

uint8_t WsCart::ReadPort(uint16_t port)
{
	if(port == 0xC0 || port == 0xCF) {
		return _state.SelectedBanks[0];
	} else if(port < 0xC4) {
		return _state.SelectedBanks[port - 0xC0];
	}

	return _memoryManager->GetUnmappedPort();
}

void WsCart::WritePort(uint16_t port, uint8_t value)
{
	if(port == 0xC0) {
		_state.SelectedBanks[0] = value & 0x3F;
		_memoryManager->RefreshMappings();
	} else if(port < 0xC4) {
		_state.SelectedBanks[port - 0xC0] = value;
		_memoryManager->RefreshMappings();
	}
}

void WsCart::Serialize(Serializer& s)
{
	SVArray(_state.SelectedBanks, 4);
}
