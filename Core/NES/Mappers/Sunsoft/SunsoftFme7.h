#pragma once
#include "pch.h"
#include "NES/BaseMapper.h"
#include "NES/NesCpu.h"
#include "NES/Mappers/Audio/Sunsoft5bAudio.h"

class SunsoftFme7 : public BaseMapper
{
private:
	unique_ptr<Sunsoft5bAudio> _audio;
	uint8_t _command = 0;
	uint8_t _workRamValue = 0;
	bool _irqEnabled = false;
	bool _irqCounterEnabled = false;
	uint16_t _irqCounter = 0;

	//Only used by register viewer
	uint8_t _chrRegs[8] = {};
	uint8_t _prgRegs[3] = {};

protected:
	uint16_t GetPrgPageSize() override { return 0x2000; }
	uint16_t GetChrPageSize() override { return 0x400; }
	uint32_t GetWorkRamSize() override { return 0x8000; }
	uint32_t GetWorkRamPageSize() override { return 0x2000; }
	uint32_t GetSaveRamSize() override { return 0x8000; }
	uint32_t GetSaveRamPageSize() override { return 0x2000; }
	bool EnableCpuClockHook() override { return true; }

	void InitMapper() override
	{
		_audio.reset(new Sunsoft5bAudio(_console));

		_command = GetPowerOnByte() & 0xF;
		_workRamValue = 0;
		_irqEnabled = false;
		_irqCounterEnabled = false;
		_irqCounter = 0;

		SelectPrgPage(3, -1);

		UpdateWorkRam();
	}

	void Serialize(Serializer& s) override
	{
		BaseMapper::Serialize(s);
		SV(_audio);
		SV(_command);
		SV(_workRamValue);
		SV(_irqEnabled);
		SV(_irqCounterEnabled);
		SV(_irqCounter);
		SVArray(_chrRegs, 8);
		SVArray(_prgRegs, 3);
		if(!s.IsSaving()) {
			UpdateWorkRam();
		}
	}

	void ProcessCpuClock() override
	{
		BaseProcessCpuClock();

		if(_irqCounterEnabled) {
			_irqCounter--;
			if(_irqCounter == 0xFFFF) {
				if(_irqEnabled) {
					_console->GetCpu()->SetIrqSource(IRQSource::External);
				}
			}
		}

		_audio->Clock();
	}

	void UpdateWorkRam()
	{
		if(_workRamValue & 0x40) {
			MemoryAccessType accessType = (_workRamValue & 0x80) ? MemoryAccessType::ReadWrite : MemoryAccessType::NoAccess;
			SetCpuMemoryMapping(0x6000, 0x7FFF, _workRamValue & 0x3F, HasBattery() ? PrgMemoryType::SaveRam : PrgMemoryType::WorkRam, accessType);
		} else {
			SetCpuMemoryMapping(0x6000, 0x7FFF, _workRamValue & 0x3F, PrgMemoryType::PrgRom);
		}
	}

	void WriteRegister(uint16_t addr, uint8_t value) override
	{
		switch(addr & 0xE000) {
			case 0x8000:
				_command = value & 0x0F;
				break;
			case 0xA000:
				switch(_command) {
					case 0:
					case 1:
					case 2:
					case 3:
					case 4:
					case 5:
					case 6:
					case 7:
						_chrRegs[_command] = value;
						SelectChrPage(_command, value);
						break;

					case 8: {
						_workRamValue = value;
						UpdateWorkRam();
						break;
					}

					case 9:
					case 0xA:
					case 0xB:
						_prgRegs[_command - 9] = value & 0x3F;
						SelectPrgPage(_command - 9, value & 0x3F);
						break;

					case 0xC:
						switch(value & 0x03) {
							case 0: SetMirroringType(MirroringType::Vertical); break;
							case 1: SetMirroringType(MirroringType::Horizontal); break;
							case 2: SetMirroringType(MirroringType::ScreenAOnly); break;
							case 3: SetMirroringType(MirroringType::ScreenBOnly); break;
						}
						break;

					case 0xD:
						_irqEnabled = (value & 0x01) == 0x01;
						_irqCounterEnabled = (value & 0x80) == 0x80;
						_console->GetCpu()->ClearIrqSource(IRQSource::External);
						break;

					case 0xE:
						_irqCounter = (_irqCounter & 0xFF00) | value;
						break;

					case 0xF:
						_irqCounter = (_irqCounter & 0xFF) | (value << 8);
						break;
				}
				break;

			case 0xC000:
			case 0xE000:
				_audio->WriteRegister(addr, value);
				break;
		}
	}

	vector<MapperStateEntry> GetMapperStateEntries() override
	{
		vector<MapperStateEntry> entries;
		string mirroringType;
		uint8_t mirValue = 0;
		switch(GetMirroringType()) {
			case MirroringType::Horizontal:
				mirroringType = "Horizontal";
				mirValue = 3;
				break;
			case MirroringType::Vertical:
				mirroringType = "Vertical";
				mirValue = 2;
				break;
			case MirroringType::ScreenBOnly:
				mirroringType = "Screen B";
				mirValue = 1;
				break;
			case MirroringType::ScreenAOnly:
				mirroringType = "Screen A";
				mirValue = 0;
				break;
		}
		entries.push_back(MapperStateEntry("$8000", "Current Register", _command, MapperStateValueType::Number8));

		entries.push_back(MapperStateEntry("", "CHR Bank 0", _chrRegs[0], MapperStateValueType::Number8));
		entries.push_back(MapperStateEntry("", "CHR Bank 1", _chrRegs[1], MapperStateValueType::Number8));
		entries.push_back(MapperStateEntry("", "CHR Bank 2", _chrRegs[2], MapperStateValueType::Number8));
		entries.push_back(MapperStateEntry("", "CHR Bank 3", _chrRegs[3], MapperStateValueType::Number8));
		entries.push_back(MapperStateEntry("", "CHR Bank 4", _chrRegs[4], MapperStateValueType::Number8));
		entries.push_back(MapperStateEntry("", "CHR Bank 5", _chrRegs[5], MapperStateValueType::Number8));
		entries.push_back(MapperStateEntry("", "CHR Bank 6", _chrRegs[6], MapperStateValueType::Number8));
		entries.push_back(MapperStateEntry("", "CHR Bank 7", _chrRegs[7], MapperStateValueType::Number8));

		entries.push_back(MapperStateEntry("", "PRG Bank 0 ($6000)", _workRamValue & 0x3F, MapperStateValueType::Number8));
		entries.push_back(MapperStateEntry("", "PRG Bank 0 is RAM", (_workRamValue & 0x40) == 0x40, MapperStateValueType::Bool));
		entries.push_back(MapperStateEntry("", "PRG Bank 1 ($8000)", _prgRegs[0], MapperStateValueType::Number8));
		entries.push_back(MapperStateEntry("", "PRG Bank 2 ($A000)", _prgRegs[1], MapperStateValueType::Number8));
		entries.push_back(MapperStateEntry("", "PRG Bank 3 ($C000)", _prgRegs[2], MapperStateValueType::Number8));

		entries.push_back(MapperStateEntry("", "RAM Enabled", (_workRamValue & 0x80) == 0x80, MapperStateValueType::Bool));
		entries.push_back(MapperStateEntry("", "Mirroring", mirroringType, mirValue));

		entries.push_back(MapperStateEntry("", "IRQ Enabled", _irqEnabled, MapperStateValueType::Bool));
		entries.push_back(MapperStateEntry("", "IRQ Count Enabled", _irqCounterEnabled, MapperStateValueType::Bool));

		entries.push_back(MapperStateEntry("", "IRQ Counter Value", _irqCounter, MapperStateValueType::Number16));
		return entries;
	}
};