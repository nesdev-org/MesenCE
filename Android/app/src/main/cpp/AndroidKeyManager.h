#pragma once

#include <mutex>
#include <optional>
#include <string>
#include <unordered_map>
#include <vector>

#include "Core/Shared/Interfaces/IKeyManager.h"
#include "Core/Shared/KeyDefinitions.h"

// Minimal input backend for the first Android UI. Java touch controls call
// SetKeyState; the emulator reads the same key codes as the desktop builds.
class AndroidKeyManager final : public IKeyManager
{
private:
	std::mutex _mutex;
	std::vector<KeyDefinition> _keyDefinitions;
	std::unordered_map<uint16_t, std::string> _keyNames;
	std::unordered_map<std::string, uint16_t> _keyCodes;
	bool _keyState[0x205] = {};
	bool _disabled = false;

public:
	AndroidKeyManager();

	void RefreshState() override {}
	void UpdateDevices() override {}
	bool IsMouseButtonPressed(MouseButton button) override;
	bool IsKeyPressed(uint16_t keyCode) override;
	std::optional<int16_t> GetAxisPosition(uint16_t keyCode) override;
	std::vector<uint16_t> GetPressedKeys() override;
	std::string GetKeyName(uint16_t keyCode) override;
	uint16_t GetKeyCode(std::string keyName) override;

	bool SetKeyState(uint16_t scanCode, bool state) override;
	void ResetKeyState() override;
	void SetDisabled(bool disabled) override;
};
