#pragma once
#include "pch.h"
#include "Utilities/ISerializable.h"
#include "Shared/Utilities/S3511ARtc.h"

class Emulator;

class GbaRtc final : public S3511ARtc
{
private:
	uint8_t _clk = 0;
	uint8_t _prevValue = 0;
	uint8_t _bitOut = 0;
	bool _chipSelect = false;

public:
	GbaRtc(Emulator* emu);

	uint8_t Read();
	void Write(uint8_t value);

	void Serialize(Serializer& s) override;
};
