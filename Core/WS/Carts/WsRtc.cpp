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
	_ready = true;
}

bool WsRtc::IsCommandRead()
{
	return (_command & 0x01) != 0;
}

void WsRtc::StartByteTransfer()
{
	if(_busy) {
		_ready = false;
		_transferClock = _console->GetCartridgeClock() + 8;
	}
}

void WsRtc::Update()
{
	if(!_ready && _console->GetCartridgeClock() >= _transferClock) {
		if(!_commandTransferred) {
			WriteBits((_command & 0x0F) | 0x60, 8);
			_commandTransferred = true;
			_ready = true;
			if(_bytesLeft) {
				StartByteTransfer();
			}
		} else if(_bytesLeft) {
			if(IsCommandRead()) {
				_data = ReadBits(8);
			} else {
				WriteBits(_data, 8);
			}
			_ready = true;
			_bytesLeft--;
		}

		if(_commandTransferred && !_bytesLeft) {
			ResetBus();
			_busy = false;
			_commandTransferred = false;
		}
	}
}

uint8_t WsRtc::ReadPort(uint16_t port)
{
	Update();

	if(port == 0xCA) {
		return (_command & 0x0F) | (_busy ? 0x10 : 0) | (_ready ? 0x80 : 0);
	} else if(port == 0xCB) {
		if(_bytesLeft && IsCommandRead()) {
			StartByteTransfer();
		}
		return _data;
	}

	return 0;
}

void WsRtc::WritePort(uint16_t port, uint8_t value)
{
	Update();

	if(port == 0xCA) {
		_busy = (value & 0x10) == 0x10;
		_command = (value & 0x0F);
		_bytesLeft = GetCommandLength(_command);
		StartByteTransfer();
	} else if(port == 0xCB) {
		_data = value;
		if(_bytesLeft && !IsCommandRead()) {
			StartByteTransfer();
		}
	}
}

void WsRtc::Serialize(Serializer& s)
{
	S3511ARtc::Serialize(s);

	SV(_command);
	SV(_data);
	SV(_bytesLeft);
	SV(_commandTransferred);
	SV(_transferClock);
	SV(_ready);
	SV(_busy);
}
