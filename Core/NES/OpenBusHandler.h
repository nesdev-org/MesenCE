#pragma once
#include "pch.h"
#include "NES/INesMemoryHandler.h"
#include "NES/NesTypes.h"
#include "Utilities/Serializer.h"

class OpenBusHandler : public INesMemoryHandler, public ISerializable
{
private:
	uint8_t _externalOpenBus = 0;
	uint8_t _internalOpenBus = 0;

public:
	OpenBusHandler()
	{
	}

	uint8_t ReadRam(uint16_t addr) override
	{
		return _externalOpenBus;
	}

	uint8_t PeekRam(uint16_t addr) override
	{
		return addr >> 8; //Fake open bus for debugger
	}

	__forceinline uint8_t GetOpenBus()
	{
		return _externalOpenBus;
	}

	__forceinline uint8_t GetInternalOpenBus()
	{
		return _internalOpenBus;
	}

	template<NesCpuBusType busType = NesCpuBusType::Both>
	__forceinline void SetOpenBus(uint8_t value, bool forceInternal = false)
	{
		if(forceInternal) {
			_internalOpenBus = value;
			return;
		}

		switch(busType) {
			case NesCpuBusType::Internal: _internalOpenBus = value; break;
			case NesCpuBusType::External: _externalOpenBus = value; break;
			case NesCpuBusType::Both:
				_internalOpenBus = value;
				_externalOpenBus = value;
				break;
		}
	}

	void GetMemoryRanges(MemoryRanges& ranges) override
	{
	}

	void WriteRam(uint16_t addr, uint8_t value) override
	{
	}

	void Serialize(Serializer& s) override
	{
		SV(_internalOpenBus);
		SV(_externalOpenBus);
	}
};