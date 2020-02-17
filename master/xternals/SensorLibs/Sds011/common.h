#pragma once

#include <stdint.h>
#include <sys/types.h>

#ifdef __arm__
#define DLL_EXPORT extern "C" 
#else
#define DLL_EXPORT extern "C" __declspec(dllexport)
#endif