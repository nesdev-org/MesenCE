#include "pch.h"
#include "WS/Carts/WsCart.h"
#include "WS/Carts/WsRtc.h"
#include "WS/WsMemoryManager.h"
#include "WS/WsEeprom.h"
#include "Utilities/BitUtilities.h"
#include "Utilities/Serializer.h"

void WsCart::Map(uint32_t start, uint32_t end, MemoryType type, uint32_t offset, bool readonly)
{
	_memoryManager->Map(start, end, type, offset, readonly);
}

void WsCart::Unmap(uint32_t start, uint32_t end)
{
	_memoryManager->Unmap(start, end);
}

WsCart::WsCart()
{
	_state.SelectedBanks[0] = 0x3F;
	_state.SelectedBanks[1] = 0x3FF;
	_state.SelectedBanks[2] = 0x3FF;
	_state.SelectedBanks[3] = 0x3FF;
}

void WsCart::Init(WsMemoryManager* memoryManager, WsEeprom* cartEeprom, WsRtc* cartRtc, uint8_t* prgRom, uint32_t prgRomSize, uint8_t* saveRam, uint32_t saveRamSize)
{
	_memoryManager = memoryManager;
	_cartEeprom = cartEeprom;
	_cartRtc = cartRtc;
	_state.HasRtc = cartRtc != nullptr;
	_prgRom = prgRom;
	_prgRomSize = prgRomSize;
	_saveRam = saveRam;
	_saveRamSize = saveRamSize;

	this->PostInit();
}

void WsCart::PostInit()
{
}

uint32_t WsCart::GetPhysicalAddress(uint32_t addr)
{
	if(addr >= 0x40000) {
		return (_state.SelectedBanks[0] << 20) | (addr & 0xFFFFF);
	} else {
		return (_state.SelectedBanks[addr >> 16] << 16) | (addr & 0xFFFF);
	}
}

void WsCart::RefreshMappings()
{
	if(_state.RomInRamBank) {
		Map(0x10000, 0x1FFFF, MemoryType::WsPrgRom, _state.SelectedBanks[1] * 0x10000, true);
	} else {
		Map(0x10000, 0x1FFFF, MemoryType::WsCartRam, _state.SelectedBanks[1] * 0x10000, false);
	}
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
	} else if(port >= 0xC4 && port < 0xC9 && _cartEeprom) {
		return _cartEeprom->ReadPort(port - 0xC4);
	} else if(port >= 0xCA && port < 0xCC && _cartRtc) {
		return _cartRtc->ReadPort(port);
	} else if(port == 0xCE) {
		return _state.RomInRamBank ? 0x01 : 0;
	} else if(port >= 0xD0 && port < 0xD6) {
		return _state.SelectedBanks[1 + ((port - 0xD0) >> 1)] >> ((port & 1) ? 8 : 0);
	}

	return _memoryManager->GetUnmappedPort();
}

void WsCart::WritePort(uint16_t port, uint8_t value)
{
	if(port == 0xC0 || port == 0xCF) {
		_state.SelectedBanks[0] = value & 0x3F;
		_memoryManager->RefreshMappings();
	} else if(port < 0xC4) {
		BitUtilities::SetBits<0>(_state.SelectedBanks[port - 0xC0], value);
		_memoryManager->RefreshMappings();
	} else if(port >= 0xC4 && port < 0xC9 && _cartEeprom) {
		_cartEeprom->WritePort(port - 0xC4, value);
	} else if(port >= 0xCA && port < 0xCC && _cartRtc) {
		_cartRtc->WritePort(port, value);
	} else if(port == 0xCE) {
		_state.RomInRamBank = (value & 0x01) != 0;
		_memoryManager->RefreshMappings();
	} else if(port >= 0xD0 && port < 0xD6) {
		if(port & 1) {
			BitUtilities::SetBits<8>(_state.SelectedBanks[1 + ((port - 0xD0) >> 1)], value & 0x03);
		} else {
			BitUtilities::SetBits<0>(_state.SelectedBanks[1 + ((port - 0xD0) >> 1)], value);
		}
		_memoryManager->RefreshMappings();
	}
}

uint8_t WsCart::ReadMemory(uint32_t addr)
{
	assert(false);
}

void WsCart::WriteMemory(uint32_t addr, uint8_t value)
{
	assert(false);
}

void WsCart::Serialize(Serializer& s)
{
	SVArray(_state.SelectedBanks, 4);
}
