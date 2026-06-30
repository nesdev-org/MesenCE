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

	uint8_t* _prgRom = nullptr;
	uint32_t _prgRomSize = 0;
	uint8_t* _saveRam = nullptr;
	uint32_t _saveRamSize = 0;

	void Map(uint32_t start, uint32_t end, MemoryType type, uint32_t offset, bool readonly);
	void Unmap(uint32_t start, uint32_t end);

	uint32_t GetPhysicalAddress(uint32_t addr);

public:
	WsCart();
	virtual ~WsCart() {}

	virtual void Init(WsMemoryManager* memoryManager, WsEeprom* cartEeprom, WsRtc* cartRtc, uint8_t* prgRom, uint32_t prgRomSize, uint8_t* saveRam, uint32_t saveRamSize);
	virtual void RefreshMappings();

	WsCartState& GetState() { return _state; }
	WsEeprom* GetEeprom() { return _cartEeprom; }
	WsRtc* GetRtc() { return _cartRtc; }

	virtual uint8_t ReadMemory(uint32_t addr);
	virtual void WriteMemory(uint32_t addr, uint8_t value);

	virtual uint8_t ReadPort(uint16_t port);
	virtual void WritePort(uint16_t port, uint8_t value);

	void Serialize(Serializer& s) override;
};
