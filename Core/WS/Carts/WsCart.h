#pragma once
#include "pch.h"
#include "WS/WsTypes.h"
#include "Utilities/ISerializable.h"
#include "Shared/MemoryType.h"

class Emulator;
class WsConsole;
class WsMemoryManager;
class WsEeprom;

class WsCart : public ISerializable
{
protected:
	WsCartState _state = {};

	Emulator* _emu = nullptr;
	WsMemoryManager* _memoryManager = nullptr;

	uint8_t* _prgRom = nullptr;
	uint32_t _prgRomSize = 0;
	uint8_t* _saveRam = nullptr;
	uint32_t _saveRamSize = 0;

	bool _hasCustomReadHandler = false;
	bool _hasCustomWriteHandler = false;

	void Map(uint32_t start, uint32_t end, MemoryType type, uint32_t offset, bool readonly);
	void Unmap(uint32_t start, uint32_t end);

	virtual bool InternalReadCart(uint32_t addr, uint8_t& value) { return false; }
	virtual bool InternalWriteCart(uint32_t addr, uint8_t value) { return false; }

public:
	WsCart();
	virtual ~WsCart() {}

	virtual void LoadBattery();
	virtual void SaveBattery();

	void Init(Emulator* emu, WsCartType cartType, WsMemoryManager* memoryManager, uint8_t* prgRom, uint32_t prgRomSize, uint8_t* saveRam, uint32_t saveRamSize);
	virtual void RefreshMappings();

	virtual void OnBeforeBreak() {}

	WsCartState& GetState() { return _state; }

	__forceinline bool ReadCart(uint32_t addr, uint8_t& value)
	{
		if(_hasCustomReadHandler) {
			return InternalReadCart(addr, value);
		}
		return false;
	}

	__forceinline bool WriteCart(uint32_t addr, uint8_t value)
	{
		if(_hasCustomWriteHandler) {
			return InternalWriteCart(addr, value);
		}
		return false;
	}

	virtual uint8_t ReadPort(uint16_t port);
	virtual void WritePort(uint16_t port, uint8_t value);

	void Serialize(Serializer& s) override;
};
