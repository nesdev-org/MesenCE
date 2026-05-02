#pragma once
#include "pch.h"
#include "NES/BaseMapper.h"
#include "NES/NesConsole.h"
#include "NES/NesCpu.h"
#include "NES/BaseNesPpu.h"
#include "NES/Mappers/A12Watcher.h"

class Rambo1 : public BaseMapper
{
protected:
	const uint8_t PpuIrqDelay = 2;
	const uint8_t CpuIrqDelay = 1;
	bool _irqEnabled = false;
	bool _irqCycleMode = false;
	bool _needReload = false;
	uint8_t _irqCounter = 0;
	uint8_t _irqReloadValue = 0;
	uint8_t _cpuClockCounter = 0;
	A12Watcher _a12Watcher;

	uint8_t _prgMode = 0;
	uint8_t _chrMode = 0;
	uint8_t _currentRegister = 0;
	uint8_t _registers[16] = {};
	uint8_t _needIrqDelay = 0;
	bool _forceClock = false;

	struct Rambo1State
	{
		uint8_t Reg8000;
		uint8_t RegA000;
		//uint8_t RegA001; // unknown
	} _state = {};

protected:
	uint16_t GetPrgPageSize() override { return 0x2000; }
	uint16_t GetChrPageSize() override { return 0x400; }
	bool EnableCpuClockHook() override { return true; }
	bool EnableVramAddressHook() override { return true; }

	void InitMapper() override
	{
		_state.Reg8000 = GetPowerOnByte();
		_state.RegA000 = GetPowerOnByte();
		//_state.RegA001 = GetPowerOnByte();

		_chrMode = GetPowerOnByte() & 0x01;
		_prgMode = GetPowerOnByte() & 0x01;

		_currentRegister = GetPowerOnByte();

		// First 8 are shared with MMC3
		memset(_registers, 0, sizeof(_registers));
		_registers[0] = GetPowerOnByte(0);
		_registers[1] = GetPowerOnByte(2);
		_registers[2] = GetPowerOnByte(4);
		_registers[3] = GetPowerOnByte(5);
		_registers[4] = GetPowerOnByte(6);
		_registers[5] = GetPowerOnByte(7);
		_registers[6] = GetPowerOnByte(0);
		_registers[7] = GetPowerOnByte(1);

		// RAMBO-1 exclusive
		_registers[8] = GetPowerOnByte(8);
		_registers[9] = GetPowerOnByte(9);

		// Police Academy prototype relies on initial $C000 bank selection
		_registers[15] = GetPowerOnByte(2);

		_irqCounter = GetPowerOnByte();
		_irqReloadValue = GetPowerOnByte();
		_needReload = GetPowerOnByte() & 0x01;
		_irqEnabled = GetPowerOnByte() & 0x01;

		UpdateState();
		UpdateMirroring();
	}

	void Serialize(Serializer& s) override
	{
		BaseMapper::Serialize(s);
		SVArray(_registers, 16);
		SV(_a12Watcher);
		SV(_state.Reg8000);
		SV(_state.RegA000);
		//SV(_state.RegA001);
		SV(_currentRegister);
		SV(_chrMode);
		SV(_prgMode);
		SV(_irqEnabled);
		SV(_irqCycleMode);
		SV(_needReload);
		SV(_needIrqDelay);
		SV(_irqCounter);
		SV(_irqReloadValue);
		SV(_cpuClockCounter);
		SV(_forceClock);
	}

	void ProcessCpuClock() override
	{
		BaseProcessCpuClock();

		if(_needIrqDelay) {
			_needIrqDelay--;
			if(_needIrqDelay == 0) {
				_console->GetCpu()->SetIrqSource(IRQSource::External);
			}
		}

		if(_irqCycleMode || _forceClock) {
			_cpuClockCounter = (_cpuClockCounter + 1) & 0x03;
			if(_cpuClockCounter == 0) {
				ClockIrqCounter(Rambo1::CpuIrqDelay);
				_forceClock = false;
			}
		}
	}

	void ClockIrqCounter(const uint8_t delay)
	{
		if(_needReload) {
			//Fixes Hard Drivin'
			if(_irqReloadValue <= 1) {
				_irqCounter = _irqReloadValue + 1;
			} else {
				_irqCounter = _irqReloadValue + 2;
			}

			_needReload = false;
		} else if(_irqCounter == 0) {
			_irqCounter = _irqReloadValue + 1;
		}

		_irqCounter--;
		if(_irqCounter == 0 && _irqEnabled) {
			_needIrqDelay = delay;
		}
	}

	void UpdateMirroring()
	{
		SetMirroringType(((_state.RegA000 & 0x01) == 0x01) ? MirroringType::Horizontal : MirroringType::Vertical);
	}

	void UpdatePrgMapping()
	{
		if(_prgMode == 0) {
			SelectPrgPage(0, _registers[6]);
			SelectPrgPage(1, _registers[7]);
			SelectPrgPage(2, _registers[15]);
			SelectPrgPage(3, -1);
		} else if(_prgMode == 1) {
			SelectPrgPage(0, _registers[15]);
			SelectPrgPage(1, _registers[7]);
			SelectPrgPage(2, _registers[6]);
			SelectPrgPage(3, -1);
		}
	}

	void UpdateChrMapping()
	{
		uint8_t a12Inversion = _chrMode << 6;
		SelectChrPage(0 ^ a12Inversion, _registers[0]);
		SelectChrPage(2 ^ a12Inversion, _registers[1]);
		SelectChrPage(4 ^ a12Inversion, _registers[2]);
		SelectChrPage(5 ^ a12Inversion, _registers[3]);
		SelectChrPage(6 ^ a12Inversion, _registers[4]);
		SelectChrPage(7 ^ a12Inversion, _registers[5]);

		if(_state.Reg8000 & 0x20) {
			SelectChrPage(1 ^ a12Inversion, _registers[8]);
			SelectChrPage(3 ^ a12Inversion, _registers[9]);
		} else {
			SelectChrPage(1 ^ a12Inversion, _registers[0] | 0x01);
			SelectChrPage(3 ^ a12Inversion, _registers[1] | 0x01);
		}
	}

	void UpdateState()
	{
		_currentRegister = _state.Reg8000 & 0x0F;
		_chrMode = (_state.Reg8000 & 0x80) >> 7;
		_prgMode = (_state.Reg8000 & 0x40) >> 6;
		UpdatePrgMapping();
		UpdateChrMapping();
	}

	void WriteRegister(uint16_t addr, uint8_t value) override
	{
		switch(addr & 0xE001) {
			case 0x8000:
				_state.Reg8000 = value;
				UpdateState();
				break;

			case 0x8001:
				_registers[_currentRegister] = value;
				UpdateState();
				break;

			case 0xA000:
				_state.RegA000 = value;
				UpdateMirroring();
				break;

			case 0xC000:
				_irqReloadValue = value;
				break;

			case 0xC001:
				if(_irqCycleMode && ((value & 0x01) == 0x00)) {
					//"To be clear, after the write in the reg $C001, are needed more than four CPU clock cycles before the switch takes place, allowing another clock of irq running the reload." -FHorse
					//Fixes Skull & Crossbones
					_forceClock = true;
				}
				_irqCycleMode = (value & 0x01) == 0x01;
				if(_irqCycleMode) {
					_cpuClockCounter = 0;
				}
				_needReload = true;
				break;

			case 0xE000:
				_irqEnabled = false;
				_console->GetCpu()->ClearIrqSource(IRQSource::External);
				break;

			case 0xE001:
				_irqEnabled = true;
				break;
		}
	}

public:
	void NotifyVramAddressChange(uint16_t addr) override
	{
		if(!_irqCycleMode) {
			if(_a12Watcher.UpdateVramAddress<30>(addr, _console->GetPpu()->GetFrameCycle()) == A12StateChange::Rise) {
				ClockIrqCounter(Rambo1::PpuIrqDelay);
			}
		}
	}
};
