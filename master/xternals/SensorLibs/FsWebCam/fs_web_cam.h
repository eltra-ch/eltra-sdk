#pragma once

#include "common.h"

DLL_EXPORT int fswebcam_initialize(int p_deviceId, int p_apiID);
DLL_EXPORT int fswebcam_release();
DLL_EXPORT int fswebcam_take_picture(char* p_pFileName);
DLL_EXPORT int fswebcam_take_picture_buffer_size(unsigned int* p_pBufferSize);
DLL_EXPORT int fswebcam_take_picture_buffer(unsigned char* p_Buffer, unsigned int p_BufferSize);