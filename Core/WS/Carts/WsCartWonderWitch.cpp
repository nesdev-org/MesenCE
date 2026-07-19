#include "pch.h"
#include "WS/WsMemoryManager.h"
#include "WS/Carts/WsCartWonderWitch.h"
#include "Shared/BatteryManager.h"
#include "Shared/Emulator.h"

//TODOWS sector protection
//TODOWS timing, more accurate status reads during programming
//TODOWS more verification with datasheet and hardware

WsCartWonderWitch::WsCartWonderWitch(WsRtc* rtc) : WsCartBandai2003(rtc)
{
	_hasCustomReadHandler = true;
	_hasCustomWriteHandler = true;
	_saveFlashRom = false;
}

void WsCartWonderWitch::LoadBattery()
{
	WsCartBandai2003::LoadBattery();
	_emu->GetBatteryManager()->LoadBattery(".flash", _prgRom, _prgRomSize);
}

void WsCartWonderWitch::SaveBattery()
{
	WsCartBandai2003::SaveBattery();
	if(_saveFlashRom) {
		_emu->GetBatteryManager()->SaveBattery(".flash", _prgRom, _prgRomSize);
	}
}

uint32_t WsCartWonderWitch::GetPhysicalAddress(uint32_t addr)
{
	if(addr >= 0x40000) {
		return (GetSelectedBank(0) << 20) | (addr & 0xFFFFF);
	} else {
		return (GetSelectedBank(addr >> 16) << 16) | (addr & 0xFFFF);
	}
}

bool WsCartWonderWitch::InternalReadCart(uint32_t addr, uint8_t& value)
{
	if(addr < 0x10000) {
		return false;
	}

	bool sram = ((addr >> 16) == 1) && !_state.RomInRamBank;
	uint32_t physAddr = GetPhysicalAddress(addr);

	if(sram) {
		value = _saveRam[physAddr & (_saveRamSize - 1)];
		return true;
	}

	physAddr &= _prgRomSize - 1;

	uint8_t sector = physAddr >= 0x60000 ? 1 : 0;

	value = _prgRom[physAddr];
	if(_mode[sector] == Mode::Fast) {
		value = (value & 0x80) | 0x04;
	} else if(_mode[sector] == Mode::Autoselect) {
		//TODOWS support word bus reads correctly
		if((physAddr & 0x87) == 0x00) {
			value = 0x04;
		} else if((physAddr & 0x87) == 0x02) {
			value = 0x0C;
		} else if((physAddr & 0x87) == 0x04) {
			value = 0x00;
		}
		//TODOWS what is read at other addresses?
	}
	return true;
}

bool WsCartWonderWitch::InternalWriteCart(uint32_t addr, uint8_t value)
{
	if(addr < 0x10000) {
		return false;
	}

	bool sram = ((addr >> 16) == 1) && !_state.RomInRamBank;
	uint32_t physAddr = GetPhysicalAddress(addr);

	if(sram) {
		_saveRam[physAddr & (_saveRamSize - 1)] = value;
		return true;
	}

	physAddr &= _prgRomSize - 1;

	uint8_t sector = physAddr >= 0x60000 ? 1 : 0;
	if(_mode[sector] == Mode::Program) {
		_saveFlashRom = true;
		_prgRom[physAddr] &= value;
		_mode[sector] = Mode::Read;
	} else if(_mode[sector] == Mode::FastProgram) {
		_saveFlashRom = true;
		_prgRom[physAddr] &= value;
		_mode[sector] = Mode::Fast;
	} else if(_mode[sector] == Mode::Fast) {
		if(value == 0xA0) {
			_mode[sector] = Mode::FastProgram;
		} else if(value == 0x90) {
			_mode[sector] = Mode::Read;
		}
	} else {
		//TODOWS how does this operate in erase mode?
		if(value == 0xF0) {
			_mode[sector] = Mode::Read;
		}

		if(_unlock == 0 && value == 0xAA && (physAddr & 0xFFF) == 0xAAA) {
			_unlock = 1;
		} else if(_unlock == 1) {
			_unlock = (value == 0x55 && (physAddr & 0xFFF) == 0x555) ? 2 : 0;
		} else if(_unlock == 2) {
			if(_mode[sector] == Mode::Erase) {
				if(value == 0x10) {
					_saveFlashRom = true;
					memset(_prgRom, 0xFF, _prgRomSize);
				} else if(value == 0x30) {
					uint32_t offset, size;
					if(physAddr < 0x60000) {
						offset = physAddr & 0x70000;
						size = 0x10000;
					} else if(physAddr < 0x64000) {
						offset = 0x60000;
						size = 0x4000;
					} else if(physAddr < 0x6c000) {
						offset = 0x64000;
						size = 0x8000;
					} else if(physAddr < 0x74000) {
						offset = physAddr & 0x7e000;
						size = 0x2000;
					} else if(physAddr < 0x7c000) {
						offset = 0x74000;
						size = 0x8000;
					} else {
						offset = 0x7c000;
						size = 0x4000;
					}
					_saveFlashRom = true;
					memset(_prgRom + offset, 0xFF, size);
				}
				_mode[sector] = Mode::Read;
			} else {
				switch(value) {
					case 0x90: _mode[sector] = Mode::Autoselect; break;
					case 0xA0: _mode[sector] = Mode::Program; break;
					case 0x80: _mode[sector] = Mode::Erase; break;
					case 0x20: _mode[sector] = Mode::Fast; break;
				}
			}

			_unlock = 0;
			if(_mode[0] != Mode::Read && _mode[1] != Mode::Read) {
				_mode[sector ^ 1] = Mode::Read;
			}
		}
	}

	return true;
}

void WsCartWonderWitch::Serialize(Serializer& s)
{
	WsCartBandai2003::Serialize(s);
	SV(_unlock);
	SVArray(_mode, 2);
	SV(_saveFlashRom);

	//TODOWS rom would need to be in save state data for rewind/step back/etc. to work properly after the rom is modified
}
