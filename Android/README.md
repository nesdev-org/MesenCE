# MesenCE Android (32-bit ARM)

This directory is the first Android bootstrap for the MesenCE fork. It uses
SDL2's Android activity and builds the existing C++ emulator core with the
Android NDK. The initial target is intentionally limited to **32-bit ARM**:
`armeabi-v7a` is the only ABI accepted by Gradle and CMake.

## Requirements

- Android Studio or Gradle 8.1.1
- Android SDK platform 34 and build-tools 34.x
- Android NDK 27.2.12479018 and CMake 3.22.1
- A checkout with submodules enabled (`git clone --recurse-submodules ...`)

If the repository was cloned without submodules, run:

```sh
git submodule update --init --recursive
```

SDL2 is pinned as `Android/third_party/SDL` at the 2.30.9 release. Its Java
`SDLActivity` is compiled into this app and its native library is built by the
same CMake invocation as MesenCE.

## Build

Open `Android/` in Android Studio and select the `armeabi-v7a` variant, or run
the following from the repository root after installing Gradle:

```sh
gradle -p Android :app:assembleDebug
```

The APK is written to `Android/app/build/outputs/apk/debug/`.

## Current scope

The native shell initializes the MesenCE emulator, SDL video/audio devices and
Android lifecycle handling. The activity now includes an `ACTION_OPEN_DOCUMENT`
ROM picker (the selected `content://` file is copied into app-private storage)
and a basic NES/SNES-style touch layout. The Android settings UI and advanced
controller mappings remain future work.

Do not remove the ABI guard while adding other architectures: each ABI needs
its own validation pass for the emulator's JIT-sensitive code and third-party
libraries.
