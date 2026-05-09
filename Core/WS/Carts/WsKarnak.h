#pragma once
#include "pch.h"
#include "WS/WsTypes.h"
#include "Utilities/ISerializable.h"

class WsKarnak final : public ISerializable
{
protected:
	WsKarnakState _state = {};

	uint16_t _timerCounter;
	int8_t _adpcmStepIndex;
	uint8_t _adpcmInputShift;

	void Reset();
	void ProcessAdpcmInput(uint8_t sample);

public:
	WsKarnak();
	virtual ~WsKarnak() {}

	WsKarnakState& GetState() { return _state; }

	virtual uint8_t ReadPort(uint16_t port);
	virtual void WritePort(uint16_t port, uint8_t value);

	void Serialize(Serializer& s) override;
};
