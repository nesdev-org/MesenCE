#include "pch.h"
#include "SNES/Debugger/TraceLogger/SnesCpuTraceLogger.h"
#include "SNES/SnesCpuTypes.h"
#include "SNES/SnesPpu.h"
#include "SNES/SnesMemoryManager.h"
#include "Debugger/DisassemblyInfo.h"
#include "Debugger/Debugger.h"
#include "Debugger/DebugTypes.h"

SnesCpuTraceLogger::SnesCpuTraceLogger(Debugger* debugger, IDebugger* cpuDebugger, CpuType cpuType, SnesPpu* ppu, SnesMemoryManager* memoryManager) : BaseTraceLogger(debugger, cpuDebugger, cpuType)
{
	_ppu = ppu;
	_memoryManager = memoryManager;
}

RowDataType SnesCpuTraceLogger::GetFormatTagType(string& tag)
{
	if(tag == "A") {
		return RowDataType::A;
	} else if(tag == "X") {
		return RowDataType::X;
	} else if(tag == "Y") {
		return RowDataType::Y;
	} else if(tag == "D") {
		return RowDataType::D;
	} else if(tag == "DB") {
		return RowDataType::DB;
	} else if(tag == "P") {
		return RowDataType::PS;
	} else if(tag == "SP") {
		return RowDataType::SP;
	} else {
		return RowDataType::Text;
	}
}

void SnesCpuTraceLogger::StartRowStateTracking()
{
	_uniqueRowBankRecords = new RomState;
	memset(_uniqueRowBankRecords, 0, sizeof(RomState));
}

void SnesCpuTraceLogger::StopRowStateTracking()
{
	for(size_t i = 0u; i < BankCount; ++i) {
		if(_uniqueRowBankRecords->BankStates[i] != nullptr) {
			delete _uniqueRowBankRecords->BankStates[i];
			_uniqueRowBankRecords->BankStates[i] = nullptr;
		}
	}

	delete _uniqueRowBankRecords;
}

bool SnesCpuTraceLogger::IsUniqueRow(SnesCpuState& cpuState, DisassemblyInfo& disassemblyInfo)
{
	uint32_t pc = GetProgramCounter(cpuState);
	uint8_t bank = (uint8_t)(pc >> 16);
	uint16_t bankOffset = (uint16_t)pc;

	// Make sure to only call this function after StartRowStateTracking().
	assert(_uniqueRowBankRecords != nullptr);

	RelevantCpuStateSetter rowState;
	rowState.PackedState = 0u;
	rowState.State.D = cpuState.D;
	rowState.State.K = cpuState.K;
	rowState.State.DBR = cpuState.DBR;
	rowState.State.PS = cpuState.PS;
	rowState.State.EmulationMode = cpuState.EmulationMode;

	return (_uniqueRowBankRecords->BankStates[bank] == nullptr || _uniqueRowBankRecords->BankStates[bank]->RowStates[bankOffset].UniqueCpuStates.find(rowState.PackedState) == _uniqueRowBankRecords->BankStates[bank]->RowStates[bankOffset].UniqueCpuStates.cend());
}

void SnesCpuTraceLogger::TrackRowState(SnesCpuState& cpuState, DisassemblyInfo& disassemblyInfo)
{
	uint32_t pc = GetProgramCounter(cpuState);
	uint8_t bank = (uint8_t)(pc >> 16);
	uint16_t bankOffset = (uint16_t)pc;

	RelevantCpuStateSetter rowState;
	rowState.PackedState = 0u;
	rowState.State.D = cpuState.D;
	rowState.State.K = cpuState.K;
	rowState.State.DBR = cpuState.DBR;
	rowState.State.PS = cpuState.PS;
	rowState.State.EmulationMode = cpuState.EmulationMode;

	if(_uniqueRowBankRecords->BankStates[bank] == nullptr) {
		_uniqueRowBankRecords->BankStates[bank] = new BankState;
	}

	_uniqueRowBankRecords->BankStates[bank]->RowStates[bankOffset].UniqueCpuStates.insert(rowState.PackedState);
}

void SnesCpuTraceLogger::GetTraceData(vector<uint8_t>* target, SnesCpuState& cpuState, DisassemblyInfo& disassemblyInfo, TraceLogOutput::OutputFormat traceFormat)
{
	switch(traceFormat) {
		case TraceLogOutput::OutputFormat::Text:
			// Should never be called - handled in BaseTraceLogger.
			break;
		case TraceLogOutput::OutputFormat::Diztinguish:
			target->push_back(0xEF);
			break;
		case TraceLogOutput::OutputFormat::DiztinguishAbridged:
			target->push_back(0xEE);
			break;
	}

	uint32_t pc = GetProgramCounter(cpuState);

	if(traceFormat == TraceLogOutput::OutputFormat::Diztinguish || traceFormat == TraceLogOutput::OutputFormat::DiztinguishAbridged) {
		size_t sizeIndex = target->size();
		target->push_back(0xFF); // Stub - we'll fill this out later.

		target->push_back((pc >> 0) & 0xFF);
		target->push_back((pc >> 8) & 0xFF);
		target->push_back((pc >> 16) & 0xFF);

		target->push_back(disassemblyInfo.GetOpSize());

		target->push_back((cpuState.D >> 0) & 0xFF);
		target->push_back((cpuState.D >> 8) & 0xFF);

		target->push_back(cpuState.DBR);
		target->push_back(cpuState.PS);

		if(traceFormat == TraceLogOutput::OutputFormat::Diztinguish) {
			uint8_t* byteCode = disassemblyInfo.GetByteCode();
			target->push_back(byteCode[0u]);
			target->push_back(byteCode[1u]);
			target->push_back(byteCode[2u]);
			target->push_back(byteCode[3u]);

			target->push_back((cpuState.A >> 0) & 0xFF);
			target->push_back((cpuState.A >> 8) & 0xFF);

			target->push_back((cpuState.X >> 0) & 0xFF);
			target->push_back((cpuState.X >> 8) & 0xFF);

			target->push_back((cpuState.Y >> 0) & 0xFF);
			target->push_back((cpuState.Y >> 8) & 0xFF);

			target->push_back((cpuState.SP >> 0) & 0xFF);
			target->push_back((cpuState.SP >> 8) & 0xFF);

			target->push_back((uint8_t)cpuState.EmulationMode);
		}

		(*target)[sizeIndex] = (uint8_t)(target->size() - sizeIndex - 1u);
	}
}

void SnesCpuTraceLogger::GetTraceRow(string& output, SnesCpuState& cpuState, TraceLogPpuState& ppuState, DisassemblyInfo& disassemblyInfo)
{
	constexpr char activeStatusLetters[8] = { 'N', 'V', 'M', 'X', 'D', 'I', 'Z', 'C' };
	constexpr char inactiveStatusLetters[8] = { 'n', 'v', 'm', 'x', 'd', 'i', 'z', 'c' };

	for(RowPart& rowPart : _rowParts) {
		switch(rowPart.DataType) {
			case RowDataType::A: WriteIntValue(output, cpuState.A, rowPart); break;
			case RowDataType::X: WriteIntValue(output, cpuState.X, rowPart); break;
			case RowDataType::Y: WriteIntValue(output, cpuState.Y, rowPart); break;
			case RowDataType::D: WriteIntValue(output, cpuState.D, rowPart); break;
			case RowDataType::DB: WriteIntValue(output, cpuState.DBR, rowPart); break;
			case RowDataType::SP: WriteIntValue(output, cpuState.SP, rowPart); break;
			case RowDataType::PS: GetStatusFlag(activeStatusLetters, inactiveStatusLetters, output, cpuState.PS, rowPart); break;
			default: ProcessSharedTag(rowPart, output, cpuState, ppuState, disassemblyInfo); break;
		}
	}
}

void SnesCpuTraceLogger::LogPpuState()
{
	_ppuState[_currentPos] = {
		_ppu->GetCycle(),
		_memoryManager->GetHClock(),
		_ppu->GetScanline(),
		_ppu->GetFrameCount()
	};
}