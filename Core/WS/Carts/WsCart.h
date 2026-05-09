#pragma once
#include "pch.h"
#include "WS/WsTypes.h"
#include "Utilities/ISerializable.h"
#include "Shared/MemoryType.h"

class WsConsole;
class WsMemoryManager;
class WsEeprom;
class WsRtc;

class WsCart : public ISerializable
{
protected:
	WsCartState _state = {};

	WsMemoryManager* _memoryManager = nullptr;
	WsEeprom* _cartEeprom = nullptr;
	WsRtc* _cartRtc = nullptr;

	void Map(uint32_t start, uint32_t end, MemoryType type, uint32_t offset, bool readonly);
	void Unmap(uint32_t start, uint32_t end);

public:
	WsCart();
	virtual ~WsCart() {}

	void Init(WsMemoryManager* memoryManager, WsEeprom* cartEeprom, WsRtc* cartRtc);
	void RefreshMappings();

	WsCartState& GetState() { return _state; }
	WsEeprom* GetEeprom() { return _cartEeprom; }
	WsRtc* GetRtc() { return _cartRtc; }

	virtual uint8_t ReadPort(uint16_t port);
	virtual void WritePort(uint16_t port, uint8_t value);

	void Serialize(Serializer& s) override;
};
