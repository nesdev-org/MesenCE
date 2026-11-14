#pragma once
#include "pch.h"
#include "NES/Mappers/Nintendo/MMC3.h"

class MMC3_114 : public MMC3
{
private:
	static constexpr uint8_t _security[2][8] =
		{
			{ 0, 3, 1, 5, 6, 7, 2, 4 }, // submapper 0
			{ 0, 2, 5, 3, 6, 1, 7, 4 }  // submapper 1
		};
	static constexpr uint16_t _mmc3RemapRegs[2][8] =
		{
			/*
			  Mapper 114 regs:
			  0x8000  0x8001  0xA000  0xA001  0xC000  0xC001  0xE000  0xE001
			*/
			{ 0xA001, 0xA000, 0x8000, 0xC000, 0x8001, 0xC001, 0xE000, 0xE001 }, // MMC3 register meant for submapper 0
			{ 0xA001, 0x8001, 0x8000, 0xC001, 0xA000, 0xC000, 0xE000, 0xE001 }  // MMC3 register meant for submapper 1
		};
	uint8_t _exRegs[2] = {};
	uint8_t _submapperID;
	uint16_t _remappedAddr;

protected:
	uint16_t RegisterStartAddress() override { return 0x5000; }
	bool ForceMmc3RevAIrqs() override { return true; }

	void InitMapper() override
	{
		MMC3::InitMapper();
		_exRegs[0] = _exRegs[1] = 0;
		_submapperID = _romInfo.SubMapperID;
		if (_submapperID > 1) {
			MessageManager::DisplayMessage("Error", "UnsupportedMapper", "iNES #114 #sub " + std::to_string(_submapperID));
			_submapperID = 0; // prevent potential buffer overflow
			_emu->Pause();
		}
	}

	void Serialize(Serializer& s) override
	{
		MMC3::Serialize(s);
		SV(_exRegs[0]);
		SV(_exRegs[1]);
		SV(_submapperID);
		SV(_remappedAddr);
	}

	void UpdatePrgMapping() override
	{
		if(_exRegs[0] & 0x80) {
			SelectPrgPage2x(0, (_exRegs[0] & 0x0F) << 1);
			SelectPrgPage2x(1, (_exRegs[0] & 0x0F) << 1);
		} else {
			MMC3::UpdatePrgMapping();
		}
	}

	void WriteRegister(uint16_t addr, uint8_t value) override
	{
		if(addr < 0x8000) {
			_exRegs[0] = value;
			UpdatePrgMapping();
		} else {
			/*
			  Transform addr to _mmc3RemapRegs index
			  0x8000 -> 0, 0x8001 -> 1, 0xA000 -> 2, etc
			*/
			addr &= 0xE001;
			int i = (addr>>12) + (addr&0x01) - 8;

			_remappedAddr = _mmc3RemapRegs[_submapperID][i];
			switch (_remappedAddr) {
				case 0x8000:
					MMC3::WriteRegister(0x8000, (value & 0xC0) | _security[_submapperID][value & 0x07]);
					_exRegs[1] = 1;
					break;

				case 0x8001:
					if(_exRegs[1]) {
						_exRegs[1] = 0;
						MMC3::WriteRegister(0x8001, value);
					}
					break;

				case 0xA000:
					MMC3::WriteRegister(0xA000, value);
					break;

				case 0xC000:
					_irqReloadValue = value;
					break;

				case 0xC001:
					_irqReload = true;
					break;

				case 0xE000:
					_console->GetCpu()->ClearIrqSource(IRQSource::External);
					_irqEnabled = false;
					break;

				case 0xE001:
					_irqEnabled = true;
					break;
			}
		}
	}
};