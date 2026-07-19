#include "pch.h"
#include "WS/Carts/WsCartBandai2001.h"
#include "WS/Carts/WsCart.h"
#include "WS/WsEeprom.h"

WsCartBandai2001::WsCartBandai2001(WsEeprom* eeprom) : WsCart()
{
	_cartEeprom = eeprom;
}

void WsCartBandai2001::LoadBattery()
{
	WsCart::LoadBattery();
	if(_cartEeprom) {
		_cartEeprom->LoadBattery();
	}
}

void WsCartBandai2001::SaveBattery()
{
	WsCart::SaveBattery();
	if(_cartEeprom) {
		_cartEeprom->SaveBattery();
	}
}

void WsCartBandai2001::OnBeforeBreak()
{
	if(_cartEeprom) {
		_cartEeprom->Run();
	}
}

uint8_t WsCartBandai2001::ReadPort(uint16_t port)
{
	if(port >= 0xC4 && port < 0xC9 && _cartEeprom) {
		return _cartEeprom->ReadPort(port - 0xC4);
	}
	return WsCart::ReadPort(port);
}

void WsCartBandai2001::WritePort(uint16_t port, uint8_t value)
{
	if(port >= 0xC4 && port < 0xC9 && _cartEeprom) {
		_cartEeprom->WritePort(port - 0xC4, value);
	} else {
		WsCart::WritePort(port, value);
	}
}
