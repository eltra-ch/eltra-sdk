#pragma once

#include "common.h"

DLL_EXPORT int fswebcam_initialize();
DLL_EXPORT int fswebcam_release();
DLL_EXPORT int fswebcam_take_picture(unsigned short p_usIndex, char* p_pFileName);
