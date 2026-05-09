#pragma once
#include "pch.h"
#include "Utilities/ISerializable.h"
#include "Shared/Utilities/S3511ARtc.h"

class Emulator;

class WsRtc final : public S3511ARtc
{
private:
	WsConsole* _console;

	uint8_t _command = 0;
	uint8_t _data = 0;
	uint8_t _bytesLeft = 0;
	bool _commandTransferred = false;
	uint32_t _transferClock = 0;
	bool _ready = true;
	bool _busy = false;

	bool IsCommandRead();
	void StartByteTransfer();
	void Update();

public:
	WsRtc(Emulator* emu, WsConsole* console);

	uint8_t ReadPort(uint16_t port);
	void WritePort(uint16_t port, uint8_t value);

	void Serialize(Serializer& s) override;
};
