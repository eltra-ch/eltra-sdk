#pragma once

#include "common.h"

DLL_EXPORT int relay_initialize();
DLL_EXPORT int relay_release();
DLL_EXPORT int relay_read(unsigned short p_usPin, int* p_plValue); 
DLL_EXPORT int relay_write(unsigned short p_usPin, int p_lValue);
DLL_EXPORT int relay_pin_mode(unsigned short p_usPin, int p_lValue);
