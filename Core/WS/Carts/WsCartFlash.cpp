#include "pch.h"
#include "WS/WsMemoryManager.h"
#include "WS/Carts/WsCartFlash.h"

//TODOWS sector protection
//TODOWS timing, more accurate status reads during programming
//TODOWS more verification with datasheet and hardware

WsCartFlash::WsCartFlash() : WsCart()
{
}

void WsCartFlash::PostInit()
{
	assert(_prgRomSize == 0x80000);
	_memoryManager->SetCartFlash(true);
}

uint8_t WsCartFlash::ReadMemory(uint32_t addr)
{
	bool sram = ((addr >> 16) == 1) && !_state.RomInRamBank;
	uint32_t physAddr = GetPhysicalAddress(addr);

	if(sram) {
		return _saveRam[physAddr & (_saveRamSize - 1)];
	}

	physAddr &= _prgRomSize - 1;

	uint8_t sector = physAddr >= 0x60000 ? 1 : 0;
	uint8_t value = _prgRom[physAddr];
	if(_mode[sector] == Mode::Fast) {
		return (value & 0x80) | 0x04;
	}
	if(_mode[sector] == Mode::Autoselect) {
		//TODOWS support word bus reads correctly
		if((physAddr & 0x87) == 0x00) {
			return 0x04;
		}
		if((physAddr & 0x87) == 0x02) {
			return 0x0C;
		}
		if((physAddr & 0x87) == 0x04) {
			return 0x00;
		}
		//TODOWS what is read at other addresses?
	}
	return value;
}

void WsCartFlash::WriteMemory(uint32_t addr, uint8_t value)
{
	bool sram = ((addr >> 16) == 1) && !_state.RomInRamBank;
	uint32_t physAddr = GetPhysicalAddress(addr);

	if(sram) {
		_saveRam[physAddr & (_saveRamSize - 1)] = value;
		return;
	}

	physAddr &= _prgRomSize - 1;

	uint8_t sector = physAddr >= 0x60000 ? 1 : 0;
	if(_mode[sector] == Mode::Program) {
		_prgRom[physAddr] &= value;
		_mode[sector] = Mode::Read;
	} else if(_mode[sector] == Mode::FastProgram) {
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
			if(_mode[sector] != Mode::Read) {
				_mode[sector ^ 1] = Mode::Read;
			}
		}
	}
}

void WsCartFlash::Serialize(Serializer& s)
{
	WsCart::Serialize(s);

	SV(_unlock);
	SVArray(_mode, 2);
}
