#include "Shared/Utilities/S3511ARtc.h"
#include "WS/WsConsole.h"
#include "pch.h"
#include "WS/Carts/WsRtc.h"
#include "Utilities/Serializer.h"
#include "Shared/Emulator.h"

// TODOWS clock-accurate behavior, needs tests
// TODOWS edge case: changing commands mid-transfer, needs tests

WsRtc::WsRtc(Emulator* emu, WsConsole* console) : S3511ARtc(emu), _console(console)
{
	_state.Ready = true;
}

bool WsRtc::IsCommandRead()
{
	return (_state.Command & 0x01) != 0;
}

void WsRtc::StartByteTransfer()
{
	if(_state.Busy) {
		_state.Ready = false;
		_transferClock = _console->GetCartridgeClock() + 8;
	}
}

void WsRtc::Update()
{
	if(!_state.Ready && _console->GetCartridgeClock() >= _transferClock) {
		if(!_commandTransferred) {
			WriteBits((_state.Command & 0x0F) | 0x60, 8);
			_commandTransferred = true;
			_state.Ready = true;
			if(_bytesLeft) {
				StartByteTransfer();
			}
		} else if(_bytesLeft) {
			if(IsCommandRead()) {
				_state.Data = ReadBits(8);
			} else {
				WriteBits(_state.Data, 8);
			}
			_state.Ready = true;
			_bytesLeft--;
		}

		if(_commandTransferred && !_bytesLeft) {
			ResetBus();
			_state.Busy = false;
			_commandTransferred = false;
		}
	}
}

uint8_t WsRtc::ReadPort(uint16_t port)
{
	Update();

	if(port == 0xCA) {
		return (_state.Command & 0x0F) | (_state.Busy ? 0x10 : 0) | (_state.Ready ? 0x80 : 0);
	} else if(port == 0xCB) {
		if(_bytesLeft && IsCommandRead()) {
			StartByteTransfer();
		}
		return _state.Data;
	}

	return 0;
}

void WsRtc::WritePort(uint16_t port, uint8_t value)
{
	Update();

	if(port == 0xCA) {
		_state.Busy = (value & 0x10) == 0x10;
		_state.Command = (value & 0x0F);
		_bytesLeft = GetCommandLength(_state.Command);
		StartByteTransfer();
	} else if(port == 0xCB) {
		_state.Data = value;
		if(_bytesLeft && !IsCommandRead()) {
			StartByteTransfer();
		}
	}
}

void WsRtc::Serialize(Serializer& s)
{
	S3511ARtc::Serialize(s);

	SV(_state.Command);
	SV(_state.Data);
	SV(_state.Ready);
	SV(_state.Busy);
	
	SV(_bytesLeft);
	SV(_commandTransferred);
	SV(_transferClock);
}
