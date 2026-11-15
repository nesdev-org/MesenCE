#import <Foundation/Foundation.h>
#import <Cocoa/Cocoa.h>
#import <GameController/GameController.h>
#import <CoreHaptics/CoreHaptics.h>

#include <algorithm>
#include <iostream>
#include <stdint.h>
#include <optional>

#include "MacOSGameController.h"
//The MacOS SDK defines a global function 'Debugger', colliding with Mesen's Debugger class
//Redefine it temporarily so the headers don't cause compilation errors due to this
#define Debugger MesenDebugger
#include "Shared/MessageManager.h"
#include "Shared/Emulator.h"
#include "Shared/EmuSettings.h"
#undef Debugger

MacOSGameController::MacOSGameController(Emulator* emu, GCController* controller)
{
	_emu = emu;
	_controller = [controller retain];
	_input = [[_controller extendedGamepad] retain];

	[_input setValueChangedHandler:^ void (GCExtendedGamepad* input, GCControllerElement* element) {
		if([input buttonA] == element) _buttonState[0] = [((GCControllerButtonInput*) element) isPressed];
		if([input buttonB] == element) _buttonState[1] = [((GCControllerButtonInput*) element) isPressed];
		if([input buttonX] == element) _buttonState[2] = [((GCControllerButtonInput*) element) isPressed];
		if([input buttonY] == element) _buttonState[3] = [((GCControllerButtonInput*) element) isPressed];
		if([input leftShoulder] == element) _buttonState[4] = [((GCControllerButtonInput*) element) isPressed];
		if([input rightShoulder] == element) _buttonState[5] = [((GCControllerButtonInput*) element) isPressed];
		if([input buttonMenu] == element) _buttonState[6] = [((GCControllerButtonInput*) element) isPressed];
		if([input buttonOptions] == element) _buttonState[7] = [((GCControllerButtonInput*) element) isPressed];
		if([input dpad] == element) HandleDpad((GCControllerDirectionPad*) element);
		if([input leftTrigger] == element) _buttonState[12] = [((GCControllerButtonInput*) element) isPressed];
		if([input rightTrigger] == element) _buttonState[13] = [((GCControllerButtonInput*) element) isPressed];
		if([input leftThumbstickButton] == element) _buttonState[14] = [((GCControllerButtonInput*) element) isPressed];
		if([input rightThumbstickButton] == element) _buttonState[15] = [((GCControllerButtonInput*) element) isPressed];
		if([input leftThumbstick] == element) HandleThumbstick((GCControllerDirectionPad*) element, 0);
		if([input rightThumbstick] == element) HandleThumbstick((GCControllerDirectionPad*) element, 1);
	}];

	_haptics = nil;
	_player = nil;
	if([_controller haptics] != nil) {
		_haptics = [[_controller haptics] createEngineWithLocality:GCHapticsLocalityDefault];
		NSError* error = nil;
		[_haptics startAndReturnError:&error];
		if(error) {
			MessageManager::Log("[Input Device] Failed to initialize force feedback");
			_haptics = nil;
		} else {
			[_haptics retain];
		}
	}
}

MacOSGameController::MacOSGameController(Emulator* emu, IOHIDDeviceRef hidDevice)
{
	_emu = emu;
	_hidDevice = hidDevice;
	if(!_hidDevice) {
		return;
	}
	CFRetain(_hidDevice);

	CFArrayRef elements = IOHIDDeviceCopyMatchingElements(_hidDevice, nullptr, kIOHIDOptionsTypeNone);
	if(!elements) {
		return;
	}

	std::vector<std::pair<uint32_t, IOHIDElementRef>> buttons;
	bool slotUsed[4] = {};

	CFIndex count = CFArrayGetCount(elements);
	for(CFIndex i = 0; i < count; i++) {
		IOHIDElementRef el = (IOHIDElementRef)CFArrayGetValueAtIndex(elements, i);
		IOHIDElementType type = IOHIDElementGetType(el);
		uint32_t page = IOHIDElementGetUsagePage(el);
		uint32_t usage = IOHIDElementGetUsage(el);

		if(type == kIOHIDElementTypeInput_Button && page == kHIDPage_Button) {
			CFRetain(el);
			buttons.push_back({ usage, el });
		} else if((type == kIOHIDElementTypeInput_Misc || type == kIOHIDElementTypeInput_Axis) && page == kHIDPage_GenericDesktop) {
			if(usage == kHIDUsage_GD_Hatswitch) {
				CFRetain(el);
				HidHatElement hat;
				hat.element = el;
				hat.logicalMin = IOHIDElementGetLogicalMin(el);
				hat.logicalMax = IOHIDElementGetLogicalMax(el);
				_hidHats.push_back(hat);
			} else {
				int slot = -1;
				if(usage == kHIDUsage_GD_X) slot = 0;
				else if(usage == kHIDUsage_GD_Y) slot = 1;
				else if(usage == kHIDUsage_GD_Z || usage == kHIDUsage_GD_Rx) slot = 2;
				else if(usage == kHIDUsage_GD_Ry || usage == kHIDUsage_GD_Rz) slot = 3;

				if(slot >= 0 && !slotUsed[slot]) {
					slotUsed[slot] = true;
					CFRetain(el);
					HidAxisElement ax;
					ax.element = el;
					ax.slot = slot;
					ax.logicalMin = IOHIDElementGetLogicalMin(el);
					ax.logicalMax = IOHIDElementGetLogicalMax(el);
					_hidAxes.push_back(ax);
				}
			}
		}
	}
	CFRelease(elements);

	std::sort(buttons.begin(), buttons.end(), [](const auto& a, const auto& b) { return a.first < b.first; });
	for(const auto& b : buttons) {
		_hidButtons.push_back(b.second);
	}
}

MacOSGameController::~MacOSGameController()
{
	for(IOHIDElementRef el : _hidButtons) {
		CFRelease(el);
	}
	_hidButtons.clear();
	for(const auto& ax : _hidAxes) {
		if(ax.element) CFRelease(ax.element);
	}
	_hidAxes.clear();
	for(const auto& hat : _hidHats) {
		if(hat.element) CFRelease(hat.element);
	}
	_hidHats.clear();
	if(_hidDevice) {
		IOHIDDeviceClose(_hidDevice, kIOHIDOptionsTypeNone);
		CFRelease(_hidDevice);
		_hidDevice = nullptr;
	}
	if(_haptics) {
		if(_player) {
			NSError* error = nil;
			[_player stopAtTime:0.0 error:&error];
			[_player release];
		}
		[_haptics stopWithCompletionHandler:^ void (NSError* error) {}];
		[_haptics release];
	}
	if(_input) {
		[_input setValueChangedHandler:nil];
		[_input release];
	}
	if(_controller) {
		[_controller release];
	}
}

bool MacOSGameController::PollHid()
{
	if(!_hidDevice) {
		return false;
	}

	for(int i = 0; i < 24; i++) {
		_buttonState[i] = false;
	}

	double deadZoneRatio = _emu->GetSettings()->GetControllerDeadzoneRatio() * 0.4;
	int thresh = (int)(deadZoneRatio * 32767.0);
	if(thresh < 6000) thresh = 6000;
	if(thresh > 24000) thresh = 24000;

	for(const auto& ax : _hidAxes) {
		IOHIDValueRef valRef = nullptr;
		IOReturn ret = IOHIDDeviceGetValue(_hidDevice, ax.element, &valRef);
		if(ret == kIOReturnNotOpen || ret == kIOReturnNotAttached) {
			return false;
		}
		if(ret != kIOReturnSuccess || !valRef || ax.logicalMax <= ax.logicalMin) {
			continue;
		}
		CFIndex raw = IOHIDValueGetIntegerValue(valRef);
		double norm = ((double)(raw - ax.logicalMin) / (double)(ax.logicalMax - ax.logicalMin)) * 2.0 - 1.0;
		if(norm < -1.0) norm = -1.0;
		if(norm > 1.0) norm = 1.0;
		int16_t s = (int16_t)(norm * 32767.0);
		_axisState[ax.slot] = s;

		if(ax.slot == 0) {
			if(_hidHats.empty()) {
				_buttonState[10] = s < -thresh; // Left
				_buttonState[11] = s > thresh;  // Right
			} else {
				_buttonState[17] = s < -thresh; // X-
				_buttonState[16] = s > thresh;  // X+
			}
		} else if(ax.slot == 1) {
			if(_hidHats.empty()) {
				_buttonState[8] = s < -thresh;  // Up
				_buttonState[9] = s > thresh;   // Down
			} else {
				_buttonState[19] = s < -thresh; // Y-
				_buttonState[18] = s > thresh;  // Y+
			}
		} else if(ax.slot == 2) {
			_buttonState[21] = s < -thresh; // X2-
			_buttonState[20] = s > thresh;  // X2+
		} else if(ax.slot == 3) {
			_buttonState[23] = s < -thresh; // Y2-
			_buttonState[22] = s > thresh;  // Y2+
		}
	}

	for(const auto& hat : _hidHats) {
		IOHIDValueRef valRef = nullptr;
		IOReturn ret = IOHIDDeviceGetValue(_hidDevice, hat.element, &valRef);
		if(ret == kIOReturnNotOpen || ret == kIOReturnNotAttached) {
			return false;
		}
		if(ret == kIOReturnSuccess && valRef) {
			CFIndex v = IOHIDValueGetIntegerValue(valRef) - hat.logicalMin;
			if(v >= 0 && v <= 7) {
				if(v == 7 || v == 0 || v == 1) _buttonState[8] = true;  // Up
				if(v >= 1 && v <= 3) _buttonState[11] = true;            // Right
				if(v >= 3 && v <= 5) _buttonState[9] = true;             // Down
				if(v >= 5 && v <= 7) _buttonState[10] = true;            // Left
			}
		}
	}

	static const int btnMap[12] = { 0, 1, 2, 3, 4, 5, 12, 13, 7, 6, 14, 15 };
	for(size_t b = 0; b < _hidButtons.size() && b < 12; b++) {
		IOHIDValueRef valRef = nullptr;
		IOReturn ret = IOHIDDeviceGetValue(_hidDevice, _hidButtons[b], &valRef);
		if(ret == kIOReturnNotOpen || ret == kIOReturnNotAttached) {
			return false;
		}
		if(ret == kIOReturnSuccess && valRef && IOHIDValueGetIntegerValue(valRef) != 0) {
			_buttonState[btnMap[b]] = true;
		}
	}

	return true;
}

void MacOSGameController::HandleDpad(GCControllerDirectionPad* dpad)
{
	_buttonState[8] = [[dpad up] isPressed];
	_buttonState[9] = [[dpad down] isPressed];
	_buttonState[10] = [[dpad left] isPressed];
	_buttonState[11] = [[dpad right] isPressed];
}

void MacOSGameController::HandleThumbstick(GCControllerDirectionPad* stick, int stickNumber)
{
	double deadZoneRatio = _emu->GetSettings()->GetControllerDeadzoneRatio() * 0.4;

	float xAxis = [[stick xAxis] value];
	float yAxis = [[stick yAxis] value];
	_buttonState[(stickNumber * 4) + 16] = xAxis > deadZoneRatio;
	_buttonState[(stickNumber * 4) + 17] = xAxis < -deadZoneRatio;
	_buttonState[(stickNumber * 4) + 18] = yAxis > deadZoneRatio;
	_buttonState[(stickNumber * 4) + 19] = yAxis < -deadZoneRatio;
	_axisState[(stickNumber * 2) + 0] = INT16_MAX * xAxis;
	_axisState[(stickNumber * 2) + 1] = INT16_MAX * yAxis;
}

bool MacOSGameController::IsGameController(GCController* controller)
{
	return _controller == controller;
}

bool MacOSGameController::IsButtonPressed(int buttonNumber)
{
	if(buttonNumber < 0 || buttonNumber >= 24) {
		return false;
	}
	return _buttonState[buttonNumber];
}

std::optional<int16_t> MacOSGameController::GetAxisPosition(int axis)
{
	axis -= 24;
	if(axis < 0 || axis >= 4) {
		return std::nullopt;
	}
	return _axisState[axis];
}

void MacOSGameController::SetForceFeedback(uint16_t magnitudeRight, uint16_t magnitudeLeft)
{
	NSError* error = nil;

	if(_haptics == nil) {
		return;
	}

	if(_player != nil) {
		[_player stopAtTime:0.0 error:&error];
		if(error) {
			MessageManager::Log("[Input Device] Failed to stop force feedback effect");
			return;
		}
		[_player release];
		_player = nil;
	}

	//If magnitude is zero, only stop current effect
	if(magnitudeRight == 0 && magnitudeLeft == 0) {
		return;
	}

	double strength = (magnitudeRight < magnitudeLeft ? magnitudeLeft : magnitudeRight) / (double)UINT16_MAX;
	CHHapticEventParameter* intensityPar = [[CHHapticEventParameter alloc] initWithParameterID:CHHapticEventParameterIDHapticIntensity value:strength];
	CHHapticEventParameter* sharpnessPar = [[CHHapticEventParameter alloc] initWithParameterID:CHHapticEventParameterIDHapticSharpness value:0.6];
	CHHapticEvent* event = [[CHHapticEvent alloc] initWithEventType:CHHapticEventTypeHapticContinuous parameters:@[intensityPar, sharpnessPar] relativeTime:0.0 duration:GCHapticDurationInfinite];

	CHHapticPattern* pattern = [[CHHapticPattern alloc] initWithEvents:@[event] parameters:@[] error:&error];
	[intensityPar release];
	[sharpnessPar release];
	[event release];
	if(error) {
		MessageManager::Log("[Input Device] Failed to create force feedback pattern");
		[pattern release];
		return;
	}

	_player = [_haptics createPlayerWithPattern:pattern error:&error];
	[pattern release];
	if(error) {
		MessageManager::Log("[Input Device] Failed to create force feedback effect");
		_player = nil;
		return;
	}
	[_player retain];

	[_player startAtTime:0.0 error:&error];
	if(error) {
		MessageManager::Log("[Input Device] Failed to start force feedback effect");
	}
}
