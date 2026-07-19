#pragma once
#include "pch.h"
#include "WS/Carts/WsCart.h"

class WsRtc;

class WsCartBandai2003 : public WsCart
{
protected:
	WsRtc* _rtc = nullptr;

	uint32_t GetSelectedBank(uint8_t index);

public:
	WsCartBandai2003(WsRtc* rtc);
	virtual ~WsCartBandai2003() {}

	void LoadBattery() override;
	void SaveBattery() override;

	void RefreshMappings() override;

	uint8_t ReadPort(uint16_t port) override;
	void WritePort(uint16_t port, uint8_t value) override;

	void Serialize(Serializer& s) override;
};
