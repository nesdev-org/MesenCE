cmake_minimum_required(VERSION 3.22.1)

project(MesenCEAndroid LANGUAGES C CXX)

if(NOT ANDROID)
    message(FATAL_ERROR "The Android target must be configured with the Android NDK toolchain")
endif()

if(NOT ANDROID_ABI STREQUAL "armeabi-v7a")
    message(FATAL_ERROR "MesenCE Android currently supports only the 32-bit armeabi-v7a ABI (got ${ANDROID_ABI})")
endif()

set(MESEN_ROOT "${CMAKE_CURRENT_LIST_DIR}/../../../../..")
set(SDL_SOURCE_DIR "${MESEN_ROOT}/Android/third_party/SDL")

if(NOT EXISTS "${SDL_SOURCE_DIR}/CMakeLists.txt")
    message(FATAL_ERROR "SDL2 submodule is missing. Run: git submodule update --init --recursive")
endif()

# SDL's Android activity and native video/audio backends are built from the
# same pinned submodule that supplies the Java activity in the Gradle project.
set(SDL_SHARED ON CACHE BOOL "" FORCE)
set(SDL_STATIC OFF CACHE BOOL "" FORCE)
set(SDL_TESTS OFF CACHE BOOL "" FORCE)
set(SDL_TEST_LIBRARY OFF CACHE BOOL "" FORCE)
add_subdirectory("${SDL_SOURCE_DIR}" "${CMAKE_CURRENT_BINARY_DIR}/SDL" EXCLUDE_FROM_ALL)

file(GLOB_RECURSE MESEN_CORE_SOURCES CONFIGURE_DEPENDS
    "${MESEN_ROOT}/Core/*.cpp"
    "${MESEN_ROOT}/Core/*.c"
)
file(GLOB_RECURSE MESEN_UTIL_SOURCES CONFIGURE_DEPENDS
    "${MESEN_ROOT}/Utilities/*.cpp"
    "${MESEN_ROOT}/Utilities/*.c"
)
file(GLOB_RECURSE MESEN_LUA_SOURCES CONFIGURE_DEPENDS "${MESEN_ROOT}/Lua/*.c")
file(GLOB_RECURSE MESEN_7ZIP_SOURCES CONFIGURE_DEPENDS "${MESEN_ROOT}/SevenZip/*.c")
file(GLOB_RECURSE MESEN_SDL_SOURCES CONFIGURE_DEPENDS "${MESEN_ROOT}/Sdl/*.cpp")

add_library(mesen-core STATIC
    ${MESEN_CORE_SOURCES}
    ${MESEN_UTIL_SOURCES}
    ${MESEN_LUA_SOURCES}
    ${MESEN_7ZIP_SOURCES}
    ${MESEN_SDL_SOURCES}
)
set_target_properties(mesen-core PROPERTIES POSITION_INDEPENDENT_CODE ON)

target_include_directories(mesen-core PRIVATE
    "${MESEN_ROOT}"
    "${MESEN_ROOT}/Core"
    "${MESEN_ROOT}/Utilities"
    "${MESEN_ROOT}/Sdl"
    "${MESEN_ROOT}/Lua"
    "${MESEN_ROOT}/SevenZip"
)
target_compile_definitions(mesen-core PRIVATE MESEN_ANDROID=1)
target_compile_options(mesen-core PRIVATE
    -fexceptions
    -frtti
    -mthumb
    -march=armv7-a
)
target_link_libraries(mesen-core PRIVATE SDL2::SDL2 log)

# SDLActivity looks for libmain.so and calls its SDL_main entry point.
add_library(main SHARED main.cpp)
target_include_directories(main PRIVATE
    "${MESEN_ROOT}"
    "${MESEN_ROOT}/Core"
    "${MESEN_ROOT}/Utilities"
    "${MESEN_ROOT}/Sdl"
)
target_compile_definitions(main PRIVATE MESEN_ANDROID=1)
target_compile_options(main PRIVATE -fexceptions -frtti -mthumb -march=armv7-a)
target_link_libraries(main PRIVATE mesen-core SDL2::SDL2 log android)

set_target_properties(main PROPERTIES OUTPUT_NAME "main")
