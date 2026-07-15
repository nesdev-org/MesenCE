#pragma once
#include "pch.h"
#include "WS/Carts/WsCartBandai2003.h"

class WsRtc;

class WsCartWonderWitch final : public WsCartBandai2003
{
private:
	enum class Mode : uint8_t
	{
		Read = 0,
		Autoselect,
		Fast,
		Program,
		FastProgram,
		Erase
	};

	Mode _mode[2] = {};
	uint8_t _unlock = 0;
	bool _saveFlashRom = false;

	uint32_t GetPhysicalAddress(uint32_t addr);

public:
	WsCartWonderWitch(WsRtc* rtc);
	virtual ~WsCartWonderWitch() {}

	void LoadBattery() override;
	void SaveBattery() override;

	bool InternalReadCart(uint32_t addr, uint8_t& value) override;
	bool InternalWriteCart(uint32_t addr, uint8_t value) override;

	void Serialize(Serializer& s) override;
};
