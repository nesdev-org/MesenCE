#include "AndroidKeyManager.h"

AndroidKeyManager::AndroidKeyManager()
	: _keyDefinitions(KeyDefinition::GetSharedKeyDefinitions())
{
	for(const KeyDefinition& keyDefinition : _keyDefinitions) {
		_keyNames[keyDefinition.keyCode] = keyDefinition.name;
		_keyCodes[keyDefinition.name] = static_cast<uint16_t>(keyDefinition.keyCode);
	}
}

bool AndroidKeyManager::IsMouseButtonPressed(MouseButton button)
{
	return false;
}

bool AndroidKeyManager::IsKeyPressed(uint16_t keyCode)
{
	if(_disabled || keyCode >= 0x205) {
		return false;
	}

	std::lock_guard<std::mutex> lock(_mutex);
	return _keyState[keyCode];
}

std::optional<int16_t> AndroidKeyManager::GetAxisPosition(uint16_t keyCode)
{
	return std::nullopt;
}

std::vector<uint16_t> AndroidKeyManager::GetPressedKeys()
{
	std::vector<uint16_t> pressedKeys;
	std::lock_guard<std::mutex> lock(_mutex);
	for(uint16_t keyCode = 0; keyCode < 0x205; keyCode++) {
		if(_keyState[keyCode]) {
			pressedKeys.push_back(keyCode);
		}
	}
	return pressedKeys;
}

std::string AndroidKeyManager::GetKeyName(uint16_t keyCode)
{
	const auto key = _keyNames.find(keyCode);
	return key != _keyNames.end() ? key->second : "";
}

uint16_t AndroidKeyManager::GetKeyCode(std::string keyName)
{
	const auto key = _keyCodes.find(keyName);
	return key != _keyCodes.end() ? key->second : 0;
}

bool AndroidKeyManager::SetKeyState(uint16_t scanCode, bool state)
{
	if(scanCode >= 0x205) {
		return false;
	}

	std::lock_guard<std::mutex> lock(_mutex);
	if(_keyState[scanCode] == state) {
		return false;
	}
	_keyState[scanCode] = state;
	return true;
}

void AndroidKeyManager::ResetKeyState()
{
	std::lock_guard<std::mutex> lock(_mutex);
	for(bool& state : _keyState) {
		state = false;
	}
}

void AndroidKeyManager::SetDisabled(bool disabled)
{
	_disabled = disabled;
}
