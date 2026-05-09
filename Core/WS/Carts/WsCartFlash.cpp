#include "pch.h"
#include "WS/WsMemoryManager.h"
#include "WS/Carts/WsCartFlash.h"

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

    return _prgRom[physAddr & (_prgRomSize - 1)];
}

void WsCartFlash::WriteMemory(uint32_t addr, uint8_t value)
{
    bool sram = ((addr >> 16) == 1) && !_state.RomInRamBank;
    uint32_t physAddr = GetPhysicalAddress(addr);

    if(sram) {
        _saveRam[physAddr & (_saveRamSize - 1)] = value;
        return;
    }
}
