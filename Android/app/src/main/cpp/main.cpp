#include <SDL.h>
#include <SDL_system.h>

#ifdef __ANDROID__
#include <jni.h>
#endif

#include <chrono>
#include <condition_variable>
#include <mutex>

#include "AndroidKeyManager.h"
#include "Core/Shared/Emulator.h"
#include "Core/Shared/EmuSettings.h"
#include "Core/Shared/KeyManager.h"
#include "Core/Shared/MessageManager.h"
#include "Core/Shared/SettingTypes.h"
#include "Sdl/SdlRenderer.h"
#include "Sdl/SdlSoundManager.h"
#include "Utilities/FolderUtilities.h"

namespace
{
std::mutex g_emuMutex;
std::condition_variable g_emuReady;
Emulator* g_emu = nullptr;
AndroidKeyManager* g_inputManager = nullptr;

void Log(const char* message)
{
	MessageManager::Log(message);
}

bool LoadRomFromArguments(Emulator& emu, int argc, char** argv)
{
	if(argc < 2 || argv[1] == nullptr || argv[1][0] == '\0') {
		return false;
	}

	return emu.LoadRom((VirtualFile)argv[1], VirtualFile());
}

void ConfigureTouchInput(Emulator& emu)
{
	// Keep the first Android mapping close to the common Xbox/keyboard layout
	// used by the desktop presets. The same mapping works for NES, SNES, GB,
	// GBA, PCE, SMS and WonderSwan standard controllers.
	KeyMapping& mapping = emu.GetSettings()->GetNesConfig().Port1.Keys.Mapping1;
	mapping.A = KeyManager::GetKeyCode("X");
	mapping.B = KeyManager::GetKeyCode("Z");
	mapping.X = KeyManager::GetKeyCode("S");
	mapping.Y = KeyManager::GetKeyCode("A");
	mapping.L = KeyManager::GetKeyCode("Q");
	mapping.R = KeyManager::GetKeyCode("W");
	mapping.Up = KeyManager::GetKeyCode("Up Arrow");
	mapping.Down = KeyManager::GetKeyCode("Down Arrow");
	mapping.Left = KeyManager::GetKeyCode("Left Arrow");
	mapping.Right = KeyManager::GetKeyCode("Right Arrow");
	mapping.Select = KeyManager::GetKeyCode("E");
	mapping.Start = KeyManager::GetKeyCode("R");

	// SNES and other consoles have their own ControllerConfig instances.
	ControllerConfig& snes = emu.GetSettings()->GetSnesConfig().Port1;
	snes.Keys.Mapping1 = mapping;
	ControllerConfig& gameboy = emu.GetSettings()->GetGameboyConfig().Controller;
	gameboy.Keys.Mapping1 = mapping;
	ControllerConfig& gba = emu.GetSettings()->GetGbaConfig().Controller;
	gba.Keys.Mapping1 = mapping;
	ControllerConfig& pce = emu.GetSettings()->GetPcEngineConfig().Port1;
	pce.Keys.Mapping1 = mapping;
	ControllerConfig& sms = emu.GetSettings()->GetSmsConfig().Port1;
	sms.Keys.Mapping1 = mapping;
	ControllerConfig& ws = emu.GetSettings()->GetWsConfig().ControllerHorizontal;
	ws.Keys.Mapping1 = mapping;
}
}

#ifdef __ANDROID__
extern "C" JNIEXPORT jboolean JNICALL Java_ca_mesen_android_MesenActivity_nativeLoadRom(JNIEnv* env, jclass, jstring path)
{
	if(path == nullptr) {
		return JNI_FALSE;
	}

	const char* pathChars = env->GetStringUTFChars(path, nullptr);
	if(pathChars == nullptr) {
		return JNI_FALSE;
	}

	bool loaded = false;
	{
		std::unique_lock<std::mutex> lock(g_emuMutex);
		// SDL starts its native thread from SDLActivity.onCreate(). The Java
		// document picker can be opened immediately, so wait for Emulator to be
		// published instead of silently dropping the first ROM load request.
		g_emuReady.wait_for(lock, std::chrono::seconds(10), [] { return g_emu != nullptr; });
		if(g_emu != nullptr) {
			loaded = g_emu->LoadRom((VirtualFile)pathChars, VirtualFile());
		}
	}
	env->ReleaseStringUTFChars(path, pathChars);
	return loaded ? JNI_TRUE : JNI_FALSE;
}

extern "C" JNIEXPORT void JNICALL Java_ca_mesen_android_MesenActivity_nativeSetKeyState(JNIEnv*, jclass, jint keyCode, jboolean pressed)
{
	std::lock_guard<std::mutex> lock(g_emuMutex);
	if(g_inputManager != nullptr && keyCode >= 0 && keyCode < 0x205) {
		g_inputManager->SetKeyState(static_cast<uint16_t>(keyCode), pressed == JNI_TRUE);
	}
}
#endif

// SDLActivity resolves this symbol from libmain.so.
extern "C" int SDL_main(int argc, char** argv)
{
	if(SDL_Init(SDL_INIT_VIDEO | SDL_INIT_AUDIO | SDL_INIT_GAMECONTROLLER) != 0) {
		Log(SDL_GetError());
		return 1;
	}

	#ifdef __ANDROID__
	const char* internalStorage = SDL_AndroidGetInternalStoragePath();
	#else
	const char* internalStorage = nullptr;
	#endif
	if(internalStorage != nullptr) {
		FolderUtilities::SetHomeFolder(internalStorage);
	}

	SDL_Window* window = SDL_CreateWindow(
		"MesenCE Android",
		SDL_WINDOWPOS_UNDEFINED,
		SDL_WINDOWPOS_UNDEFINED,
		256,
		240,
		SDL_WINDOW_SHOWN | SDL_WINDOW_ALLOW_HIGHDPI | SDL_WINDOW_RESIZABLE);
	if(window == nullptr) {
		Log(SDL_GetError());
		SDL_Quit();
		return 1;
	}

	Emulator emu;
	emu.Initialize(false);
	AndroidKeyManager inputManager;
	KeyManager::SetSettings(emu.GetSettings());
	KeyManager::RegisterKeyManager(&inputManager);
	ConfigureTouchInput(emu);
	{
		std::lock_guard<std::mutex> lock(g_emuMutex);
		g_emu = &emu;
		g_inputManager = &inputManager;
	}
	g_emuReady.notify_all();
	SdlRenderer renderer(&emu, window);
	SdlSoundManager soundManager(&emu);

	if(!LoadRomFromArguments(emu, argc, argv)) {
		Log("No ROM supplied. Use the Open ROM button to select a game.");
	}

	bool running = true;
	SDL_Event event = {};
	while(running) {
		while(SDL_PollEvent(&event) != 0) {
			switch(event.type) {
				case SDL_QUIT:
					running = false;
					break;
				case SDL_APP_WILLENTERBACKGROUND:
					emu.Pause();
					break;
				case SDL_APP_DIDENTERFOREGROUND:
					emu.Resume();
					break;
				default:
					break;
			}
		}
		SDL_Delay(8);
	}

	{
		std::lock_guard<std::mutex> lock(g_emuMutex);
		g_emu = nullptr;
		g_inputManager = nullptr;
	}
	emu.Stop(true);
	emu.Release();
	SDL_DestroyWindow(window);
	SDL_Quit();
	return 0;
}
