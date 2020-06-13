#include <stdio.h>
#include <mutex>
#include <stdlib.h>
#include <string.h>

#ifdef __arm__
    #include <unistd.h>
    #include <sys/types.h>
    #include <sys/wait.h>
#else
    #include <windows.h>
    #include <opencv2/videoio.hpp>
    #include <opencv2/core.hpp>
    #include <opencv2/highgui.hpp>
#endif

#include "common.h"
#include "fs_web_cam.h"

#define FSWEBCAM_SUCCESS 0
#define FSWEBCAM_FAILURE 1

#ifndef __arm__

using namespace cv;
using namespace std;

VideoCapture* g_pCapture = 0;
int g_deviceID = 0;        // 0 = open default camera
int g_apiID = cv::CAP_ANY; // 0 = autodetect default API
vector<uchar> g_buffer;

#endif

#ifndef __arm__

int OpenVideoCaptureDevice()
{
    int lResult = FSWEBCAM_FAILURE;

    if (g_pCapture == 0)
    {
        g_pCapture = new VideoCapture();

        g_pCapture->open(g_deviceID, g_apiID);

        if (g_pCapture->isOpened())
        {
            lResult = FSWEBCAM_SUCCESS;
        }
        else
        {
            lResult = FSWEBCAM_FAILURE;

            printf("ERROR: camera device id = '%d', app id = %d cannot be opened\n", g_deviceID, g_apiID);
        }
    }
    else
    {
        lResult = FSWEBCAM_SUCCESS;
    }

    return lResult;
}

#endif

DLL_EXPORT int fswebcam_initialize(int p_deviceId, int p_apiID)
{
    int lResult = FSWEBCAM_SUCCESS;

#ifndef __arm__

    if (g_pCapture)
    {
        delete g_pCapture;
        g_pCapture = 0;
    }

    g_deviceID = p_deviceId;
    g_apiID = p_apiID;

    lResult = OpenVideoCaptureDevice();

#endif

    return lResult;
}

DLL_EXPORT int fswebcam_release()
{
    int lResult = FSWEBCAM_SUCCESS;

#ifndef __arm__

    if (g_pCapture)
    {
        delete g_pCapture;
        g_pCapture = 0;
        lResult = FSWEBCAM_SUCCESS;
    }
    
#endif    

    return 0;
}

DLL_EXPORT int fswebcam_take_picture_buffer_size(int* p_pBufferSize)
{
    int lResult = FSWEBCAM_FAILURE;

#ifdef __arm__
    
#else
    Mat frame;

    lResult = OpenVideoCaptureDevice();

    if (lResult == FSWEBCAM_SUCCESS)
    {
        g_buffer.clear();

        if (g_pCapture && g_pCapture->read(frame))
        {
            if (imencode(".jpg", frame, g_buffer))
            {
                *p_pBufferSize = g_buffer.size();
            }
            else
            {
                lResult == FSWEBCAM_FAILURE;
                printf("ERROR: camera device id = '%d', app id = %d frame cannot be encoded\n", g_deviceID, g_apiID);
            }
        }
        else
        {
            lResult == FSWEBCAM_FAILURE;
            printf("ERROR: camera device id = '%d', app id = %d frame cannot be read\n", g_deviceID, g_apiID);
        }
    }

#endif

    return lResult;
}

DLL_EXPORT int fswebcam_take_picture_buffer(unsigned char* p_Buffer, int p_BufferSize)
{
    int lResult = FSWEBCAM_FAILURE;

#ifdef __arm__
    
#else
    if (p_BufferSize >= g_buffer.size())
    {
        memcpy(p_Buffer, g_buffer.data(), g_buffer.size());
        lResult = FSWEBCAM_SUCCESS;
    }
#endif

    return lResult;
}

DLL_EXPORT int fswebcam_take_picture(char* p_pFileName)
{
    int lResult = FSWEBCAM_FAILURE;

#ifdef __arm__
        char* pCmd = new char[255];

        memset(pCmd, 0, 255);

        sprintf(pCmd, "fswebcam --input %d %s", p_usIndex, p_pFileName);
        
        system(pCmd);

        lResult = FSWEBCAM_SUCCESS;
        
        delete[] pCmd;

#else
    Mat frame;
    vector<uchar> buf;
    FILE* fileHandler = 0;
    
    lResult = OpenVideoCaptureDevice();

    if (lResult == FSWEBCAM_SUCCESS)
    {
        if (g_pCapture && g_pCapture->read(frame))
        {
            if (imencode(".jpg", frame, buf))
            {
                int err = fopen_s(&fileHandler, p_pFileName, "wb");

                if (err == 0 && fileHandler)
                {
                    fwrite(buf.data(), sizeof(uchar), buf.size(), fileHandler);

                    lResult = FSWEBCAM_SUCCESS;

                    fclose(fileHandler);
                }
                else
                {
                    printf("ERROR: file '%s' cannot be opened, error code = %d\n", p_pFileName, err);
                }
            }
            else
            {
                printf("ERROR: camera device id = '%d', app id = %d frame cannot be encoded\n", g_deviceID, g_apiID);
            }
        }
        else
        {
            printf("ERROR: camera device id = '%d', app id = %d frame cannot be read\n", g_deviceID, g_apiID);
        }
    }
    
#endif

    return lResult;
}
