#include "pch.h"
#include "GBA/Cart/GbaRtc.h"
#include "Shared/Utilities/S3511ARtc.h"
#include "Utilities/Serializer.h"

GbaRtc::GbaRtc(Emulator* emu) : S3511ARtc(emu)
{
}

uint8_t GbaRtc::Read()
{
	if(_chipSelect) {
		return (_chipSelect << 2) | (_bitOut << 1) | _clk;
	} else {
		return 0x07;
	}
}

void GbaRtc::Write(uint8_t value)
{
	uint8_t clk = value & 0x01;

	//Writes use the data that was set in bit 1 on the previous write
	uint8_t data = (_prevValue & 0x02) >> 1;
	_prevValue = value;

	bool chipSelect = value & 0x04;

	if(!chipSelect || !_chipSelect) {
		ResetBus();
		_clk = 0;
		_bitOut = 1;
		_chipSelect = chipSelect;
		return;
	}

	if(clk && !_clk) {
		WriteBits(data, 1);
	} else if(!clk && _clk) {
		_bitOut = ReadBits(1);
	}

	_clk = clk;
}

void GbaRtc::Serialize(Serializer& s)
{
	S3511ARtc::Serialize(s);

	SV(_clk);
	SV(_bitOut);
	SV(_chipSelect);
	SV(_prevValue);
}
