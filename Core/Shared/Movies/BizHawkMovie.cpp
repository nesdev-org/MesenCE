#include "pch.h"
#include "Shared/Movies/BizHawkMovie.h"
#include "Shared/Emulator.h"
#include "Shared/NotificationManager.h"
#include "Shared/MessageManager.h"
#include "Utilities/StringUtilities.h"
#include "Utilities/VirtualFile.h"
#include "Utilities/ZipReader.h"
#include "Utilities/magic_enum.hpp"

bool BizHawkMovie::Play(VirtualFile& file)
{
	_movieFile = file;

	std::stringstream ss;
	file.ReadFile(ss);

	_reader.reset(new ZipReader());
	_reader->LoadArchive(ss);

	stringstream inputData;
	if(!_reader->GetStream("Header.txt", _settingsData)) {
		MessageManager::Log("[Movie] File not found: Header.txt");
		return false;
	}

	while(!_settingsData.eof()) {
		string line;
		std::getline(_settingsData, line);
		line = StringUtilities::Trim(line);

		if(!line.empty()) {
			if(StringUtilities::StartsWith(line, "Platform ")) {
				string platform = line.substr(9);

				if(platform == "NES") {
					_consoleType = ConsoleType::Nes;
				} else if(platform == "SNES") {
					_consoleType = ConsoleType::Snes;
				} else if(platform == "GBA") {
					_consoleType = ConsoleType::Gba;
				}
			} else if(StringUtilities::StartsWith(line, "MovieVersion GBAHawk")) {
				_isGbaHawk = true;
			}
		}
	}

	if(_consoleType != _emu->GetConsoleType()) {
		MessageManager::DisplayMessage("Movies", "MovieIncorrectConsole", string(magic_enum::enum_name<ConsoleType>(_consoleType)));
		return false;
	}

	if(!_reader->GetStream("Input Log.txt", inputData)) {
		MessageManager::Log("[Movie] File not found: Input Log.txt");
		return false;
	}

	while(inputData) {
		string line;
		std::getline(inputData, line);
		line = StringUtilities::Trim(line);

		if(line.substr(0, 1) == "|") {
			vector<string> lineInputData = StringUtilities::Split(line.substr(1), '|');
			if(_consoleType == ConsoleType::Snes) {
				ConvertSnesInput(lineInputData);
			} else if(_consoleType == ConsoleType::Gba) {
				ConvertGbaInput(lineInputData);
			}

			_inputData.push_back(lineInputData);
		}
	}

	_deviceIndex = 0;

	auto emuLock = _emu->AcquireLock(false);

	_emu->GetBatteryManager()->SetBatteryProvider(shared_from_this());
	_emu->GetNotificationManager()->RegisterNotificationListener(shared_from_this());
	_emu->PowerCycle();
	_emu->GetBatteryManager()->SetBatteryProvider(nullptr);

	_playing = !_loadFailure;
	return _playing;
}

vector<uint8_t> BizHawkMovie::LoadBattery(string extension)
{
	vector<uint8_t> batteryData;
	_reader->ExtractFile("MovieSaveRam.bin", batteryData);
	return batteryData;
}

void BizHawkMovie::ConvertSnesInput(vector<string>& data)
{
	auto convertPortData = [](string& input) {
		if(input.size() >= 12) {
			input = string() +
				input[9] + input[7] + input[8] + input[6] + //ABXY
				input[10] + input[11] + //LR
				input[4] + input[5] + //Select + Start
				input.substr(0, 4); //D-Pad
		}
	};

	if(data.size() > 1) {
		convertPortData(data[1]);
		if(data.size() > 2) {
			convertPortData(data[2]);
		}
	}
}

void BizHawkMovie::ConvertGbaInput(vector<string>& data)
{
	if(data.empty()) {
		return;
	}

	if(!_isGbaHawk) {
		//Ignore the tilt/light sensor data
		size_t pos = data[0].find_last_of(',');
		if(pos != string::npos) {
			data[0] = data[0].substr(pos + 1);
		}
	}

	if(data[0].size() >= 11) {
		data.insert(data.begin(), data[0][10] + string("."));
		data[1] = data[1].substr(0, 10);
	}
}

bool BizHawkMovie::ApplySettings(istream& settingsData)
{
	return true;
}
