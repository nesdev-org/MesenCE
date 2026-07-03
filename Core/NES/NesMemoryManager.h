#pragma once

#include "pch.h"

#include "NES/INesMemoryHandler.h"
#include "NES/NesTypes.h"
#include "NES/OpenBusHandler.h"
#include "NES/BaseMapper.h"
#include "Shared/Emulator.h"
#include "Shared/MemoryOperationType.h"
#include "Shared/CheatManager.h"
#include "Utilities/ISerializable.h"

class BaseMapper;
class CheatManager;
class Emulator;
class NesConsole;

class NesMemoryManager : public ISerializable
{
private:
	static constexpr int CpuMemorySize = 0x10000;
	static const int NesInternalRamSize = 0x800;
	static const int FamicomBoxInternalRamSize = 0x2000;

	Emulator* _emu = nullptr;
	CheatManager* _cheatManager = nullptr;
	NesConsole* _console = nullptr;
	BaseMapper* _mapper = nullptr;

	uint8_t* _internalRam = nullptr;
	uint32_t _internalRamSize = 0;

	OpenBusHandler _openBusHandler = {};
	unique_ptr<INesMemoryHandler> _internalRamHandler;
	INesMemoryHandler** _ramReadHandlers = nullptr;
	INesMemoryHandler** _ramWriteHandlers = nullptr;

	void InitializeMemoryHandlers(INesMemoryHandler** memoryHandlers, INesMemoryHandler* handler, vector<uint16_t>* addresses, bool allowOverride);

protected:
	void Serialize(Serializer& s) override;

public:
	NesMemoryManager(NesConsole* console, BaseMapper* mapper);
	virtual ~NesMemoryManager();

	void Reset(bool softReset);
	void RegisterIODevice(INesMemoryHandler* handler);
	void RegisterWriteHandler(INesMemoryHandler* handler, uint32_t start, uint32_t end);
	void RegisterReadHandler(INesMemoryHandler* handler, uint32_t start, uint32_t end);
	void UnregisterIODevice(INesMemoryHandler* handler);

	uint8_t DebugRead(uint16_t addr);
	uint16_t DebugReadWord(uint16_t addr);
	void DebugWrite(uint16_t addr, uint8_t value, bool disableSideEffects = true);

	uint8_t* GetInternalRam();

	template<NesCpuBusType busType = NesCpuBusType::Both>
	__forceinline uint8_t Read(uint16_t addr, MemoryOperationType operationType = MemoryOperationType::Read)
	{
		uint8_t value;
		if(addr >= 0x6000) {
			//Reading from 0x6000+ is by far the most common CPU operation (e.g CPU loading ROM to execute)
			//The mapper is always mapped in this address range, allowing us to directly call the
			//mapper's read function here, which allows it to be inlined and skips a virtual function call
			//(which isn't possible when called via the ReadRam interface)
			value = _mapper->Read(addr);
		} else {
			value = _ramReadHandlers[addr]->ReadRam(addr);
		}
		if(_cheatManager->HasCheats<CpuType::Nes>()) {
			_cheatManager->ApplyCheat<CpuType::Nes>(addr, value);
		}
		_emu->ProcessMemoryRead<CpuType::Nes>(addr, value, operationType);

		_openBusHandler.SetOpenBus<busType>(value, addr == 0x4015);

		return value;
	}

	void Write(uint16_t addr, uint8_t value, MemoryOperationType operationType);

	uint8_t GetOpenBus(uint8_t mask = 0xFF);
	uint8_t GetInternalOpenBus(uint8_t mask = 0xFF);

	template<NesCpuBusType busType = NesCpuBusType::Both>
	void SetOpenBus(uint8_t value)
	{
		_openBusHandler.SetOpenBus<busType>(value);
	}
};