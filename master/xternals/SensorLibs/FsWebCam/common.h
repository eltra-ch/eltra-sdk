#pragma once

#include <stdint.h>
#include <sys/types.h>

#ifdef __linux__
#define DLL_EXPORT extern "C" 
#elif _WIN32
#define DLL_EXPORT extern "C" __declspec(dllexport)
#endif
