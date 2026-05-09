#pragma once
#include "pch.h"
#include "Utilities/ISerializable.h"

class Emulator;

struct S3511ARtcState
{
	uint8_t Year;
	uint8_t Month;
	uint8_t Day;
	uint8_t DoW;
	uint8_t Hour;
	uint8_t Minute;
	uint8_t Second;

	uint8_t Status;
	uint8_t IntHour; //unimplemented
	uint8_t IntMinute; //unimplemented

	bool TestMode; //unimplemented
};

class S3511ARtc : public ISerializable
{
private:
	S3511ARtcState _state = {};
	uint64_t _lastUpdateTime = 0;

	uint8_t _bitCounter = 0;
	uint8_t _command = 0;

	uint64_t _dataOut = 0;
	uint8_t _dataOutSize = 0;

	uint64_t _dataIn = 0;
	uint8_t _dataInSize = 0;

	uint8_t SanitizeData(uint8_t value, uint8_t maxValue, uint8_t fixedValue);
	void FromDateTime(uint64_t data, bool includeYmd);
	uint64_t ToDateTime();

	void ProcessCommand();

	void Reset();
	void UpdateTime();

protected:
	enum class Command : uint8_t
	{
		Reset,
		Status,
		DateTime,
		Time,
		Alarm1,
		Alarm2,
		TestStart,
		TestEnd
	};

	Emulator* _emu = nullptr;

	uint8_t GetCommandLength(Command cmd);
	uint8_t GetCommandLength(uint8_t cmd);

	//GBA only uses count values of 1, WS only uses 8
	void WriteBits(uint32_t value, int count);
	uint32_t ReadBits(int count);
	void ResetBus();

public:
	S3511ARtc(Emulator* emu);

	void LoadBattery();
	void SaveBattery();

	void Serialize(Serializer& s) override;
};
