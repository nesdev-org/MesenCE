#pragma once
#include "pch.h"
#include "SNES/Coprocessors/BaseCoprocessor.h"
#include "SNES/Coprocessors/GSU/GsuTypes.h"
#include "SNES/MemoryMappings.h"
#include "SNES/IMemoryHandler.h"

class Emulator;
class SnesConsole;
class SnesCpu;
class SnesMemoryManager;
class EmuSettings;

enum class MemoryOperationType;

class Gsu : public BaseCoprocessor
{
private:
	Emulator* _emu = nullptr;
	SnesConsole* _console = nullptr;
	SnesMemoryManager* _memoryManager = nullptr;
	SnesCpu* _cpu = nullptr;
	EmuSettings* _settings = nullptr;
	uint8_t _clockMultiplier = 1;

	GsuState _state = {};

	uint8_t _cache[512] = {};
	bool _cacheValid[32] = {};
	bool _waitForRomAccess = false;
	bool _waitForRamAccess = false;
	bool _stopped = true;
	bool _r15Changed = false;
	uint8_t _maxPrgRomBank = 0;
	uint32_t _lastOpAddr = 0;

	uint32_t _gsuRamSize = 0;
	uint8_t* _gsuRam = nullptr;

	MemoryMappings _mappings;
	vector<unique_ptr<IMemoryHandler>> _gsuRamHandlers;
	vector<unique_ptr<IMemoryHandler>> _gsuCpuRamHandlers;
	vector<unique_ptr<IMemoryHandler>> _gsuCpuRomHandlers;

	__forceinline void Exec();

	void InitProgramCache(uint16_t cacheAddr);

	uint8_t ReadOperand();
	__forceinline uint8_t ReadOpCode();
	__forceinline uint8_t ReadProgramByte(MemoryOperationType opType);

	uint16_t ReadSrcReg();
	void WriteDestReg(uint16_t value);
	void WriteRegister(uint8_t reg, uint16_t value);

	void ResetFlags();
	void InvalidateCache();

	void WaitRomOperation();
	void WaitRamOperation();

	void WaitForRomAccess();
	void WaitForRamAccess();
	void UpdateRunningState();

	uint8_t ReadRomBuffer();
	uint8_t ReadRamBuffer(uint16_t addr);
	void WriteRam(uint16_t addr, uint8_t value);

	__forceinline void Step(uint64_t cycles)
	{
		_state.CycleCount += cycles;

		if(_state.RomDelay) {
			_state.RomDelay -= std::min<uint8_t>((uint8_t)cycles, _state.RomDelay);
			if(_state.RomDelay == 0) {
				WaitForRomAccess();
				_state.RomReadBuffer = ReadGsu((_state.RomBank << 16) | _state.R[14], MemoryOperationType::Read);
				_state.SFR.RomReadPending = false;
			}
		}

		if(_state.RamDelay) {
			_state.RamDelay -= std::min<uint8_t>((uint8_t)cycles, _state.RamDelay);
			if(_state.RamDelay == 0) {
				WaitForRamAccess();
				WriteGsu(0x700000 | (_state.RamBank << 16) | _state.RamWriteAddress, _state.RamWriteValue, MemoryOperationType::Write);
			}
		}
	}

	void STOP();
	void NOP();
	void CACHE();

	void Branch(bool branch);

	void BRA();
	void BLT();
	void BGE();
	void BNE();
	void BEQ();
	void BPL();
	void BMI();
	void BCC();
	void BCS();
	void BVC();
	void BVS();
	void JMP(uint8_t reg);

	void TO(uint8_t reg);
	void FROM(uint8_t reg);
	void WITH(uint8_t reg);

	void STORE(uint8_t reg);
	void LOAD(uint8_t reg);

	void LOOP();
	void ALT1();
	void ALT2();
	void ALT3();

	void MERGE();
	void SWAP();

	void PlotRpix();
	void ColorCMode();

	uint16_t GetTileIndex(uint8_t x, uint8_t y);
	uint32_t GetTileAddress(uint8_t x, uint8_t y);

	uint8_t ReadPixel(uint8_t x, uint8_t y);
	bool IsTransparentPixel();
	void DrawPixel(uint8_t x, uint8_t y);
	void FlushPrimaryCache(uint8_t x, uint8_t y);
	void WritePixelCache(GsuPixelCache& cache);

	uint8_t GetColor(uint8_t source);

	void Add(uint8_t reg);
	void SubCompare(uint8_t reg);
	void MULT(uint8_t reg);
	void FMultLMult();

	void AndBitClear(uint8_t reg);
	void SBK();

	void LINK(uint8_t reg);

	void SignExtend();

	void NOT();
	void LSR();
	void ROL();
	void ASR();
	void ROR();

	void LOB();
	void HIB();

	void IbtSmsLms(uint8_t reg);
	void IwtLmSm(uint8_t reg);

	void OrXor(uint8_t reg);
	void INC(uint8_t reg);
	void DEC(uint8_t reg);

	void GetCRamBRomB();
	void GETB();

	void ClearCharFx3(uint16_t offset);
	void ProcessClearCommandFx3(uint8_t start, uint8_t end);
	void ProcessCommandFx3();

	void UpdateClockMultiplier();

public:
	static constexpr uint8_t Fx3RomType = 0x17;
	static constexpr uint8_t Fx3BatteryRomType = 0x18; //FX3 with battery

	Gsu(SnesConsole* console, uint32_t gsuRamSize, bool isFx3);
	virtual ~Gsu();

	void ProcessEndOfFrame() override;

	uint8_t ReadGsu(uint32_t addr, MemoryOperationType opType);
	void WriteGsu(uint32_t addr, uint8_t value, MemoryOperationType opType);

	void LoadBattery() override;
	void SaveBattery() override;

	void Run() override;
	void Reset() override;

	uint8_t Read(uint32_t addr) override;
	uint8_t Peek(uint32_t addr) override;
	void PeekBlock(uint32_t addr, uint8_t* output) override;
	void Write(uint32_t addr, uint8_t value) override;
	AddressInfo GetAbsoluteAddress(uint32_t address) override;

	void Serialize(Serializer& s) override;

	GsuState& GetState();
	MemoryMappings* GetMemoryMappings();
	uint32_t DebugGetProgramCounter();
	void DebugSetProgramCounter(uint32_t addr);
};