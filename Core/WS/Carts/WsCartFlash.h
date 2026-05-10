#pragma once
#include "pch.h"
#include "WS/WsTypes.h"
#include "WS/Carts/WsCart.h"
#include "Utilities/ISerializable.h"
#include "Shared/MemoryType.h"

class WsCartFlash final : public WsCart
{
private:
	enum class Mode : uint8_t
	{
		Read = 0,
		Autoselect,
		Fast,
		Program,
		FastProgram,
		Erase
	};

	Mode _mode[2];
	uint8_t _unlock;

public:
	WsCartFlash();
	virtual ~WsCartFlash() {}

	void RefreshMappings() override;

	uint8_t ReadMemory(uint32_t addr) override;
	void WriteMemory(uint32_t addr, uint8_t value) override;

	void Serialize(Serializer& s) override;
};
