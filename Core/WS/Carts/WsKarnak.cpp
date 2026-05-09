#include "pch.h"
#include "WS/Carts/WsKarnak.h"
#include "Utilities/Serializer.h"

static const int16_t adpcmAccumulatorStep[16][16] = {
	{ 0, 0, 1, 2, 3, 5, 7, 10, 0, 0, -1, -2, -3, -5, -7, -10 },
	{ 0, 1, 2, 3, 4, 6, 8, 13, 0, -1, -2, -3, -4, -6, -8, -13 },
	{ 0, 1, 2, 4, 5, 7, 10, 15, 0, -1, -2, -4, -5, -7, -10, -15 },
	{ 0, 1, 3, 4, 6, 9, 13, 19, 0, -1, -3, -4, -6, -9, -13, -19 },
	{ 0, 2, 3, 5, 8, 11, 15, 23, 0, -2, -3, -5, -8, -11, -15, -23 },
	{ 0, 2, 4, 7, 10, 14, 19, 29, 0, -2, -4, -7, -10, -14, -19, -29 },
	{ 0, 3, 5, 8, 12, 16, 22, 33, 0, -3, -5, -8, -12, -16, -22, -33 },
	{ 1, 4, 7, 10, 15, 20, 29, 43, -1, -4, -7, -10, -15, -20, -29, -43 },
	{ 1, 4, 8, 13, 18, 25, 35, 53, -1, -4, -8, -13, -18, -25, -35, -53 },
	{ 1, 6, 10, 16, 22, 31, 43, 64, -1, -6, -10, -16, -22, -31, -43, -64 },
	{ 2, 7, 12, 19, 27, 37, 51, 76, -2, -7, -12, -19, -27, -37, -51, -76 },
	{ 2, 9, 16, 24, 34, 46, 64, 96, -2, -9, -16, -24, -34, -46, -64, -96 },
	{ 3, 11, 19, 29, 41, 57, 79, 117, -3, -11, -19, -29, -41, -57, -79, -117 },
	{ 4, 13, 24, 36, 50, 69, 96, 143, -4, -13, -24, -36, -50, -69, -96, -143 },
	{ 4, 16, 29, 44, 62, 85, 118, 175, -4, -16, -29, -44, -62, -85, -118, -175 },
	{ 6, 20, 36, 54, 76, 104, 144, 214, -6, -20, -36, -54, -76, -104, -144, -214 }
};
static const int8_t adpcmIndexStep[8] = {
	-1,
	-1,
	0,
	0,
	1,
	2,
	2,
	3
};

WsKarnak::WsKarnak()
{
	Reset();
}

void WsKarnak::Reset()
{
	_state.AdpcmAccumulator = 0x100;
	_adpcmInputShift = 0;
	_adpcmStepIndex = 0;
}

void WsKarnak::ProcessAdpcmInput(uint8_t sample)
{
	_state.AdpcmAccumulator += adpcmAccumulatorStep[_adpcmStepIndex][sample & 0xF];
	_state.AdpcmAccumulator &= 0x3FF;
	_adpcmStepIndex = std::clamp(_adpcmStepIndex + adpcmIndexStep[sample & 0x7], 0, 15);
}

uint8_t WsKarnak::ReadPort(uint16_t port)
{
	if(port == 0xD6) {
		return (_state.Enable ? 0x80 : 0) | (_state.TimerPeriod & 0x7F);
	} else if(port == 0xD9) {
		if(_state.AdpcmAccumulator >= 0x300) {
			return 0x00;
		} else if(_state.AdpcmAccumulator >= 0x200) {
			return 0xFF;
		} else {
			return _state.AdpcmAccumulator >> 1;
		}
	}

	return 0xFF;
}

void WsKarnak::WritePort(uint16_t port, uint8_t value)
{
	if(port == 0xD6) {
		_state.Enable = (value & 0x80) != 0;
		_state.TimerPeriod = value & 0x7F;
		if(!_state.Enable) {
			Reset();
		}
	} else if(port == 0xD8) {
		if(_state.Enable) {
			ProcessAdpcmInput(value >> (_adpcmInputShift ? 0 : 4));
			_adpcmInputShift ^= 1;
		}
	}
}

void WsKarnak::Serialize(Serializer& s)
{
	SV(_state.Enable);
	SV(_state.TimerPeriod);
	SV(_state.AdpcmAccumulator);
	SV(_adpcmInputShift);
	SV(_adpcmStepIndex);
	SV(_timerCounter);
}
