#pragma once
#include "pch.h"
#include "WS/WsTypes.h"
#include "Utilities/ISerializable.h"
#include "Shared/Utilities/S3511ARtc.h"

class Emulator;
class WsConsole;

class WsRtc final : public S3511ARtc
{
private:
	WsConsole* _console;
	WsRtcState _state = {};

	uint8_t _bytesLeft = 0;
	bool _commandTransferred = false;
	uint32_t _transferClock = 0;

	bool IsCommandRead();
	void StartByteTransfer();
	void Update();

public:
	WsRtc(Emulator* emu, WsConsole* console);

	WsRtcState& GetState() { return _state; }

	uint8_t ReadPort(uint16_t port);
	void WritePort(uint16_t port, uint8_t value);

	void Serialize(Serializer& s) override;
};
