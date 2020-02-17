#pragma once

#include "common.h"

DLL_EXPORT int dht22_initialize();
DLL_EXPORT int dht22_release();
DLL_EXPORT int dht22_read(unsigned short p_usPinNumber,int p_lDelay, int p_lRetryCount, double* p_pfTemperature, double* p_pfHumidity);
