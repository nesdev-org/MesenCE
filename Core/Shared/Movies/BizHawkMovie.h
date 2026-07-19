#pragma once
#include "pch.h"
#include "Shared/Movies/MesenMovie.h"
#include "Shared/SettingTypes.h"

class BizHawkMovie : public MesenMovie
{
private:
	ConsoleType _consoleType = {};
	bool _isGbaHawk = false;

	void ConvertSnesInput(vector<string>& data);
	void ConvertGbaInput(vector<string>& data);

	bool ApplySettings(istream& settingsData) override;

public:
	using MesenMovie::MesenMovie;

	bool Play(VirtualFile& file) override;
	vector<uint8_t> LoadBattery(string extension) override;
};