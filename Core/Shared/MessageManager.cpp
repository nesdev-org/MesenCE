#include "pch.h"
#include "Shared/MessageManager.h"

std::unordered_map<string, string> MessageManager::_enResources = {
	{ "Cheats", u8"作弊" },
	{ "Debug", u8"调试" },
	{ "EmulationSpeed", u8"模拟速度" },
	{ "ClockRate", u8"时钟频率" },
	{ "Error", u8"错误" },
	{ "GameInfo", u8"游戏信息" },
	{ "GameLoaded", u8"游戏已加载" },
	{ "Input", u8"输入" },
	{ "Patch", u8"补丁" },
	{ "Movies", u8"录像" },
	{ "NetPlay", u8"联机" },
	{ "Overclock", u8"超频" },
	{ "Region", u8"区域" },
	{ "SaveStates", u8"存档" },
	{ "ScreenshotSaved", u8"截图已保存" },
	{ "SoundRecorder", u8"录音器" },
	{ "Test", u8"测试" },
	{ "VideoRecorder", u8"录像器" },

	{ "ApplyingPatch", u8"正在应用补丁：%1" },
	{ "PatchFailed", u8"应用补丁失败：%1" },
	{ "CheatApplied", u8"已应用 1 个作弊码。" },
	{ "CheatsApplied", u8"已应用 %1 个作弊码。" },
	{ "CheatsDisabled", u8"已禁用所有作弊码。" },
	{ "CoinInsertedSlot", u8"已投币（槽位 %1）" },
	{ "ConnectedToServer", u8"已连接到服务器。" },
	{ "ConnectionLost", u8"与服务器的连接已断开。" },
	{ "CouldNotConnect", u8"无法连接到服务器。" },
	{ "CouldNotInitializeAudioSystem", u8"无法初始化音频系统" },
	{ "CouldNotFindRom", u8"找不到匹配的游戏 ROM。（%1）" },
	{ "CouldNotWriteToFile", u8"无法写入文件：%1" },
	{ "CouldNotLoadFile", u8"无法加载文件：%1" },
	{ "EmulationMaximumSpeed", u8"最大速度" },
	{ "EmulationSpeedPercent", u8"%1%" },
	{ "FdsDiskInserted", u8"已插入磁盘 %1 第 %2 面。" },
	{ "Frame", u8"帧" },
	{ "GameCrash", u8"游戏已崩溃（%1）" },
	{ "KeyboardModeDisabled", u8"键盘模式已禁用。" },
	{ "KeyboardModeEnabled", u8"已连接键盘——快捷键已禁用。" },
	{ "Lag", u8"延迟" },
	{ "Mapper", u8"Mapper：%1，子 Mapper：%2" },
	{ "MovieEnded", u8"录像已结束。" },
	{ "MovieStopped", u8"录像已停止。" },
	{ "MovieInvalid", u8"无效的录像文件。" },
	{ "MovieMissingRom", u8"缺少播放录像所需的 ROM（%1）。" },
	{ "MovieNewerVersion", u8"无法加载由更新版本 Mesen 创建的录像，请下载最新版本。" },
	{ "MovieIncompatibleVersion", u8"此录像与当前版本的 Mesen 不兼容。" },
	{ "MovieIncorrectConsole", u8"此录像是在其他主机（%1）上录制的，无法加载。" },
	{ "MoviePlaying", u8"正在播放录像：%1" },
	{ "MovieRecordingTo", u8"正在录制：%1" },
	{ "MovieSaved", u8"录像已保存到文件：%1" },
	{ "NetplayVersionMismatch", u8"联机客户端运行的 Mesen 版本不一致，已断开连接。" },
	{ "NetplayNotAllowed", u8"连接到服务器时不允许执行此操作。" },
	{ "OverclockEnabled", u8"已启用超频。" },
	{ "OverclockDisabled", u8"已禁用超频。" },
	{ "PrgSizeWarning", u8"PRG 大小小于 32KB" },
	{ "SaveStateEmpty", u8"槽位为空。" },
	{ "SaveStateIncompatibleVersion", u8"存档与当前版本的 Mesen 不兼容。" },
	{ "SaveStateInvalidFile", u8"无效的存档文件。" },
	{ "SaveStateWrongSystem", u8"错误：无法加载存档（主机类型不匹配）" },
	{ "SaveStateLoaded", u8"已加载存档 #%1。" },
	{ "SaveStateLoadedFile", u8"已加载存档：%1" },
	{ "SaveStateSavedFile", u8"已保存存档：%1" },
	{ "SaveStateMissingRom", u8"缺少加载存档所需的 ROM（%1）。" },
	{ "SaveStateNewerVersion", u8"无法加载由更新版本 Mesen 创建的存档，请下载最新版本。" },
	{ "SaveStateSaved", u8"已保存存档 #%1。" },
	{ "SaveStateSlotSelected", u8"已选择槽位 #%1。" },
	{ "ScanlineTimingWarning", u8"PPU 时序已更改。" },
	{ "ServerStarted", u8"服务器已启动（端口：%1）" },
	{ "ServerStopped", u8"服务器已停止" },
	{ "SoundRecorderStarted", u8"正在录制：%1" },
	{ "SoundRecorderStopped", u8"录音已保存到：%1" },
	{ "TestFileSavedTo", u8"测试文件已保存到：%1" },
	{ "UnexpectedError", u8"意外错误：%1" },
	{ "UnsupportedMapper", u8"不支持该 Mapper（%1），无法加载游戏。" },
	{ "VideoRecorderStarted", u8"正在录制：%1" },
	{ "VideoRecorderStopped", u8"录像已保存到：%1" },
};

std::list<string> MessageManager::_log;
SimpleLock MessageManager::_logLock;
SimpleLock MessageManager::_messageLock;
bool MessageManager::_osdEnabled = true;
bool MessageManager::_outputToStdout = false;
IMessageManager* MessageManager::_messageManager = nullptr;

void MessageManager::RegisterMessageManager(IMessageManager* messageManager)
{
	auto lock = _messageLock.AcquireSafe();
	if(MessageManager::_messageManager == nullptr) {
		MessageManager::_messageManager = messageManager;
	}
}

void MessageManager::UnregisterMessageManager(IMessageManager* messageManager)
{
	auto lock = _messageLock.AcquireSafe();
	if(MessageManager::_messageManager == messageManager) {
		MessageManager::_messageManager = nullptr;
	}
}

void MessageManager::SetOptions(bool osdEnabled, bool outputToStdout)
{
	_osdEnabled = osdEnabled;
	_outputToStdout = outputToStdout;
}

string MessageManager::Localize(string key)
{
	std::unordered_map<string, string>* resources = &_enResources;
	/*switch(EmulationSettings::GetDisplayLanguage()) {
		case Language::English: resources = &_enResources; break;
		case Language::French: resources = &_frResources; break;
		case Language::Japanese: resources = &_jaResources; break;
		case Language::Russian: resources = &_ruResources; break;
		case Language::Spanish: resources = &_esResources; break;
		case Language::Ukrainian: resources = &_ukResources; break;
		case Language::Portuguese: resources = &_ptResources; break;
		case Language::Catalan: resources = &_caResources; break;
		case Language::Chinese: resources = &_zhResources; break;
	}*/
	if(resources) {
		if(resources->find(key) != resources->end()) {
			return (*resources)[key];
		} /* else if(EmulationSettings::GetDisplayLanguage() != Language::English) {
			 //Fallback on English if resource key is missing another language
			 resources = &_enResources;
			 if(resources->find(key) != resources->end()) {
				 return (*resources)[key];
			 }
		 }*/
	}

	return key;
}

void MessageManager::DisplayMessage(string title, string message, string param1, string param2)
{
	if(MessageManager::_messageManager) {
		auto lock = _messageLock.AcquireSafe();
		if(!MessageManager::_messageManager) {
			return;
		}

		title = Localize(title);
		message = Localize(message);

		size_t startPos = message.find(u8"%1");
		if(startPos != std::string::npos) {
			message.replace(startPos, 2, param1);
		}

		startPos = message.find(u8"%2");
		if(startPos != std::string::npos) {
			message.replace(startPos, 2, param2);
		}

		if(_osdEnabled) {
			MessageManager::_messageManager->DisplayMessage(title, message);
		} else {
			MessageManager::Log("[" + title + "] " + message);
		}
	}
}

void MessageManager::Log(string message)
{
	auto lock = _logLock.AcquireSafe();
	if(message.empty()) {
		message = "------------------------------------------------------";
	}
	if(_log.size() >= 1000) {
		_log.pop_front();
	}
	_log.push_back(message);

	if(_outputToStdout) {
		std::cout << message << std::endl;
	}
}

void MessageManager::ClearLog()
{
	auto lock = _logLock.AcquireSafe();
	_log.clear();
}

string MessageManager::GetLog()
{
	auto lock = _logLock.AcquireSafe();
	stringstream ss;
	for(string& msg : _log) {
		ss << msg << "\n";
	}
	return ss.str();
}
