#pragma once

#include "pch.h"
#include "NES/Mappers/Nintendo/MMC3.h"

class Namco108 : public MMC3
{
protected:
	void UpdateMirroring() override
	{
		//Do nothing - Namco 108 has hardwired mirroring only
		//"Mirroring is hardwired, one game uses 4-screen mirroring (Gauntlet, DRROM)."
	}

	void WriteRegister(uint16_t addr, uint8_t value) override
	{
		if (addr >= 0xa000) return;
		addr &= 0x8001;

		if(addr == 0x8000) {
			//Disable CHR Mode 1 and PRG Mode 1
			//"PRG always has the last two 8KiB banks fixed to the end."
			//"CHR always gives the left pattern table (0000-0FFF) the two 2KiB banks, and the right pattern table (1000-1FFF) the four 1KiB banks."
			value &= 0x7;
		}

		MMC3::WriteRegister(addr, value);
	}

	void ResetNamco108()
	{
		MMC3::WriteRegister(0x8000, GetPowerOnByte() & 7);
		MMC3::WriteRegister(0xA000, 0);
		MMC3::WriteRegister(0xA001, 0x80);

		_registers[0] = GetPowerOnByte(0);
		_registers[1] = GetPowerOnByte(2);
		_registers[2] = GetPowerOnByte(4);
		_registers[3] = GetPowerOnByte(5);
		_registers[4] = GetPowerOnByte(6);
		_registers[5] = GetPowerOnByte(7);
		_registers[6] = GetPowerOnByte(0);
		_registers[7] = GetPowerOnByte(1); // For compatibility

		_irqCounter = 0; // doesn't exist but should be initialized
		_irqReloadValue = 0;
		_irqReload = 0;
		_irqEnabled = 0;
	}

	void InitMapper() override
	{
		ResetNamco108();
		SetCpuMemoryMapping(0x6000, 0x7FFF, 0, HasBattery() ? PrgMemoryType::SaveRam : PrgMemoryType::WorkRam);
		UpdateState();
		// N108 uses hardwired mirroring, so let the header deal with that
	}

	// more thorough IRQ removal
	void NotifyVramAddressChange(uint16_t addr) override
	{
	}
};
