#pragma once
#include "pch.h"
#include "WS/Carts/WsCart.h"

class WsEeprom;

class WsCartBandai2001 final : public WsCart
{
private:
	WsEeprom* _cartEeprom = nullptr;

public:
	WsCartBandai2001(WsEeprom* eeprom);
	virtual ~WsCartBandai2001() {}

	void LoadBattery() override;
	void SaveBattery() override;

	void OnBeforeBreak() override;

	uint8_t ReadPort(uint16_t port) override;
	void WritePort(uint16_t port, uint8_t value) override;
};
