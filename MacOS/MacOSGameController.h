#pragma once

#import <Foundation/Foundation.h>
#import <Cocoa/Cocoa.h>
#import <GameController/GameController.h>
#import <CoreHaptics/CoreHaptics.h>
#import <IOKit/hid/IOHIDLib.h>

#include <optional>
#include <vector>

class Emulator;

class MacOSGameController
{
private:
	struct HidAxisElement {
		IOHIDElementRef element = nullptr;
		int slot = -1; // 0=X, 1=Y, 2=X2, 3=Y2
		CFIndex logicalMin = 0;
		CFIndex logicalMax = 255;
	};

	struct HidHatElement {
		IOHIDElementRef element = nullptr;
		CFIndex logicalMin = 0;
		CFIndex logicalMax = 7;
	};

	Emulator* _emu;

	GCController* _controller = nil;
	GCExtendedGamepad* _input = nil;
	CHHapticEngine* _haptics = nil;
	id<CHHapticPatternPlayer> _player = nil;

	IOHIDDeviceRef _hidDevice = nullptr;
	std::vector<IOHIDElementRef> _hidButtons;
	std::vector<HidAxisElement> _hidAxes;
	std::vector<HidHatElement> _hidHats;

	bool _buttonState[24] = {};
	int16_t _axisState[4] = {};

	void HandleDpad(GCControllerDirectionPad* dpad);
	void HandleThumbstick(GCControllerDirectionPad* stick, int stickNumber);

public:
	MacOSGameController(Emulator* emu, GCController* controller);
	MacOSGameController(Emulator* emu, IOHIDDeviceRef hidDevice);
	~MacOSGameController();

	bool IsGameController(GCController* controller);
	bool IsHidDevice(IOHIDDeviceRef device) const { return _hidDevice != nullptr && _hidDevice == device; }
	bool IsHidController() const { return _hidDevice != nullptr; }
	bool PollHid();

	bool IsButtonPressed(int buttonNumber);
	std::optional<int16_t> GetAxisPosition(int axis);

	void SetForceFeedback(uint16_t magnitudeRight, uint16_t magnitudeLeft);
};
