#pragma once
#include "pch.h"
#include "Debugger/BaseTraceLogger.h"
#include "SNES/SnesCpuTypes.h"

#include <array>
#include <set>

class DisassemblyInfo;
class Debugger;
class SnesPpu;
class SnesMemoryManager;

class SnesCpuTraceLogger : public BaseTraceLogger<SnesCpuTraceLogger, SnesCpuState>
{
private:
	SnesPpu* _ppu = nullptr;
	SnesMemoryManager* _memoryManager = nullptr;

	static constexpr size_t BankCount = 0x100;
	static constexpr size_t BankSize = 0x10000;

	typedef uint64_t PackedRelevantCpuState;

	struct RelevantCpuState
	{
		uint16_t D;
		uint8_t K;
		uint8_t DBR;
		uint8_t PS;
		// Could be turned into a single bit, but currently, we won't fit below
		// a uint64_t, anyways.
		bool EmulationMode;
	};

	union RelevantCpuStateSetter
	{
		RelevantCpuState State;
		PackedRelevantCpuState PackedState;

		static_assert(sizeof(PackedState) >= sizeof(State));
	};

	struct RowState
	{
		std::set<PackedRelevantCpuState> UniqueCpuStates;
	};

	struct BankState
	{
		RowState RowStates[BankSize];
	};

	struct RomState
	{
		// Allocated dynamically to avoid allocating memory that isn't needed.
		BankState* BankStates[BankCount];
	};

	// Maybe this one doesn't absolutely need dynamic allocation, since it only contains
	// 256 pointers, so roughly 2 KB of memory. But whatever.
	RomState* _uniqueRowBankRecords;

protected:
	RowDataType GetFormatTagType(string& tag) override;
	void StartRowStateTracking() override;
	void StopRowStateTracking() override;
	bool IsUniqueRow(SnesCpuState& cpuState, DisassemblyInfo& disassemblyInfo) override;
	void TrackRowState(SnesCpuState& cpuState, DisassemblyInfo& disassemblyInfo) override;
	void GetTraceData(vector<uint8_t>* target, SnesCpuState& cpuState, DisassemblyInfo& disassemblyInfo, TraceLogOutput::OutputFormat traceFormat) override;

public:
	SnesCpuTraceLogger(Debugger* debugger, IDebugger* cpuDebugger, CpuType cpuType, SnesPpu* ppu, SnesMemoryManager* memoryManager);

	void GetTraceRow(string& output, SnesCpuState& cpuState, TraceLogPpuState& ppuState, DisassemblyInfo& disassemblyInfo);
	void LogPpuState();

	__forceinline uint32_t GetProgramCounter(SnesCpuState& state) { return (state.K << 16) | state.PC; }
	__forceinline uint64_t GetCycleCount(SnesCpuState& state) { return state.CycleCount; }
	__forceinline uint8_t GetStackPointer(SnesCpuState& state) { return (uint8_t)state.SP; }
};
