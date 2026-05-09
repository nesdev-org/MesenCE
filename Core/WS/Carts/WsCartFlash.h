#pragma once
#include "pch.h"
#include "WS/WsTypes.h"
#include "WS/Carts/WsCart.h"
#include "Utilities/ISerializable.h"
#include "Shared/MemoryType.h"

class WsCartFlash : public WsCart
{
public:
	WsCartFlash();
	virtual ~WsCartFlash() {}

	void PostInit() override;

	uint8_t ReadMemory(uint32_t addr) override;
	void WriteMemory(uint32_t addr, uint8_t value) override;
};
