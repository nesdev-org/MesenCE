#include <SDL.h>
#include <SDL_system.h>

#include "Core/Shared/Emulator.h"
#include "Core/Shared/MessageManager.h"
#include "Sdl/SdlRenderer.h"
#include "Sdl/SdlSoundManager.h"
#include "Utilities/FolderUtilities.h"

namespace
{
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
}

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
	SdlRenderer renderer(&emu, window);
	SdlSoundManager soundManager(&emu);

	if(!LoadRomFromArguments(emu, argc, argv)) {
		Log("No ROM supplied. The Android shell is ready; ROM selection is the next milestone.");
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

	emu.Stop(true);
	emu.Release();
	SDL_DestroyWindow(window);
	SDL_Quit();
	return 0;
}
