#pragma once
#include "pch.h"
#include "SNES/Input/SnesRumbleController.h"
#include "Utilities/Serializer.h"

class Emulator;
class SnesConsole;

struct AxisInfo
{
	uint16_t KeyCode;
	bool Invert;
	bool Scale;
};

class SnesBlueRetroController : public SnesRumbleController
{
private:
	uint32_t _mode = 0;
	uint64_t _brState = 0;
	vector<AxisInfo> _lxAxisCodes;
	vector<AxisInfo> _lyAxisCodes;
	vector<AxisInfo> _rxAxisCodes;
	vector<AxisInfo> _ryAxisCodes;
	vector<AxisInfo> _alAxisCodes;
	vector<AxisInfo> _arAxisCodes;

	int16_t GetAxisValue(vector<AxisInfo>& axisInfos);

protected:
	void Serialize(Serializer& s) override;
	void RefreshStateBuffer() override;

public:
	SnesBlueRetroController(Emulator* emu, SnesConsole* console, uint8_t port, KeyMappingSet keyMappings);
	virtual ~SnesBlueRetroController();

	uint8_t ReadRam(uint16_t addr) override;
};