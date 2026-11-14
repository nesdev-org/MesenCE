#pragma once
#include "NES/NesConsole.h"
#include "NES/NesCpu.h"
#include "Utilities/ISerializable.h"
#include "Utilities/Serializer.h"

class VrcIrq final : public ISerializable
{
private:
	NesConsole* _console;
	uint8_t _irqReloadValue = 0;
	uint8_t _irqCounter = 0;
	int16_t _irqPrescalerCounter = 0;
	bool _irqEnabled = false;
	bool _irqEnabledAfterAck = false;
	bool _irqCycleMode = false;

protected:
	void Serialize(Serializer& s) override
	{
		SV(_irqReloadValue);
		SV(_irqCounter);
		SV(_irqPrescalerCounter);
		SV(_irqEnabled);
		SV(_irqEnabledAfterAck);
		SV(_irqCycleMode);
	}

public:
	VrcIrq(NesConsole* console)
	{
		_console = console;
	}

	void Reset()
	{
		_irqPrescalerCounter = 0;
		_irqReloadValue = 0;
		_irqCounter = 0;
		_irqEnabled = false;
		_irqEnabledAfterAck = false;
		_irqCycleMode = false;
	}

	void ProcessCpuClock()
	{
		if(_irqEnabled) {
			_irqPrescalerCounter -= 3;

			if(_irqCycleMode || (_irqPrescalerCounter <= 0 && !_irqCycleMode)) {
				if(_irqCounter == 0xFF) {
					_irqCounter = _irqReloadValue;
					_console->GetCpu()->SetIrqSource(IRQSource::External);
				} else {
					_irqCounter++;
				}
				_irqPrescalerCounter += 341;
			}
		}
	}

	void SetReloadValue(uint8_t value)
	{
		_irqReloadValue = value;
	}

	void SetReloadValueNibble(uint8_t value, bool highBits)
	{
		if(highBits) {
			_irqReloadValue = (_irqReloadValue & 0x0F) | ((value & 0x0F) << 4);
		} else {
			_irqReloadValue = (_irqReloadValue & 0xF0) | (value & 0x0F);
		}
	}

	void SetControlValue(uint8_t value)
	{
		_irqEnabledAfterAck = (value & 0x01) == 0x01;
		_irqEnabled = (value & 0x02) == 0x02;
		_irqCycleMode = (value & 0x04) == 0x04;

		if(_irqEnabled) {
			_irqCounter = _irqReloadValue;
			_irqPrescalerCounter = 341;
		}

		_console->GetCpu()->ClearIrqSource(IRQSource::External);
	}

	void AcknowledgeIrq()
	{
		_irqEnabled = _irqEnabledAfterAck;
		_console->GetCpu()->ClearIrqSource(IRQSource::External);
	}

	void GetMapperStateEntries(vector<MapperStateEntry>& entries, bool swapBottomAddressBits = false)
	{
		entries.push_back(MapperStateEntry("$F000-$F002", "VRC IRQ"));

		std::string base = swapBottomAddressBits ? "$F001" : "$F002";

		entries.push_back(MapperStateEntry("$F000", "IRQ Reload", _irqReloadValue, MapperStateValueType::Number8));
		entries.push_back(MapperStateEntry(base + ".0", "IRQ Enabled After Ack", _irqEnabledAfterAck, MapperStateValueType::Bool));
		entries.push_back(MapperStateEntry(base + ".1", "IRQ Enabled", _irqEnabled, MapperStateValueType::Bool));
		entries.push_back(MapperStateEntry(base + ".2", "IRQ Cycle Mode", _irqCycleMode, MapperStateValueType::Bool));

		entries.push_back(MapperStateEntry("--", "IRQ Prescaler Counter", _irqPrescalerCounter, MapperStateValueType::Number16));

		entries.push_back(MapperStateEntry("--", "IRQ Counter", _irqCounter, MapperStateValueType::Number8));
	}
};
