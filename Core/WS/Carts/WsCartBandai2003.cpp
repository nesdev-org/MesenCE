#include "pch.h"
#include "WS/Carts/WsCartBandai2003.h"
#include "WS/Carts/WsCart.h"
#include "WS/Carts/WsRtc.h"
#include "WS/WsMemoryManager.h"
#include "Utilities/Serializer.h"

WsCartBandai2003::WsCartBandai2003(WsRtc* rtc) : WsCart()
{
	_rtc = rtc;
	_state.ExtSelectedBanks[0] = 0x03;
	_state.ExtSelectedBanks[1] = 0x03;
	_state.ExtSelectedBanks[2] = 0x03;
}

void WsCartBandai2003::LoadBattery()
{
	WsCart::LoadBattery();
	_rtc->LoadBattery();
}

void WsCartBandai2003::SaveBattery()
{
	WsCart::SaveBattery();
	_rtc->SaveBattery();
}

uint32_t WsCartBandai2003::GetSelectedBank(uint8_t index)
{
	if(index == 0) {
		return _state.SelectedBanks[0];
	} else {
		return (_state.ExtSelectedBanks[index - 1] << 8) | _state.SelectedBanks[index];
	}
}

void WsCartBandai2003::RefreshMappings()
{
	if(_state.RomInRamBank) {
		Map(0x10000, 0x1FFFF, MemoryType::WsPrgRom, GetSelectedBank(1) * 0x10000, true);
	} else {
		Map(0x10000, 0x1FFFF, MemoryType::WsCartRam, GetSelectedBank(1) * 0x10000, false);
	}
	Map(0x20000, 0x2FFFF, MemoryType::WsPrgRom, GetSelectedBank(2) * 0x10000, true);
	Map(0x30000, 0x3FFFF, MemoryType::WsPrgRom, GetSelectedBank(3) * 0x10000, true);
	Map(0x40000, 0xFFFFF, MemoryType::WsPrgRom, GetSelectedBank(0) * 0x100000 + 0x40000, true);
}

uint8_t WsCartBandai2003::ReadPort(uint16_t port)
{
	if(port >= 0xCA && port < 0xCC) {
		return _rtc->ReadPort(port);
	} else if(port == 0xCE) {
		return _state.RomInRamBank ? 0x01 : 0;
	} else if(port == 0xCF) {
		return _state.SelectedBanks[0];
	} else if(port >= 0xD0 && port < 0xD6) {
		if(port & 1) {
			return _state.ExtSelectedBanks[((port - 0xD0) >> 1)];
		} else {
			return _state.SelectedBanks[1 + ((port - 0xD0) >> 1)];
		}
	}

	return WsCart::ReadPort(port);
}

void WsCartBandai2003::WritePort(uint16_t port, uint8_t value)
{
	if(port >= 0xCA && port < 0xCC) {
		_rtc->WritePort(port, value);
	} else if(port == 0xCE) {
		_state.RomInRamBank = (value & 0x01) != 0;
		_memoryManager->RefreshMappings();
	} else if(port == 0xCF) {
		_state.SelectedBanks[0] = value & 0x3F;
		_memoryManager->RefreshMappings();
	} else if(port >= 0xD0 && port < 0xD6) {
		if(port & 1) {
			_state.ExtSelectedBanks[((port - 0xD0) >> 1)] = value & 0x03;
		} else {
			_state.SelectedBanks[1 + ((port - 0xD0) >> 1)] = value;
		}
		_memoryManager->RefreshMappings();
	} else {
		WsCart::WritePort(port, value);
	}
}

void WsCartBandai2003::Serialize(Serializer& s)
{
	WsCart::Serialize(s);
	SVArray(_state.ExtSelectedBanks, 3);
	SV(_state.RomInRamBank);
}
