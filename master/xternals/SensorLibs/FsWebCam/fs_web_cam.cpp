#include <stdio.h>
#include <mutex>
#include <stdlib.h>
#include <string.h>
#include <opencv2/videoio.hpp>
#include <opencv2/core.hpp>
#include <opencv2/highgui.hpp>

#ifdef __arm__
    #include <unistd.h>
    #include <sys/types.h>
    #include <sys/wait.h>
#else
    #include <windows.h>
#endif

#include "common.h"
#include "fs_web_cam.h"

#define FSWEBCAM_SUCCESS 0
#define FSWEBCAM_FAILURE 1

using namespace cv;
using namespace std;

VideoCapture* g_pCapture = 0;
int g_deviceID = 0;        // 0 = open default camera
int g_apiID = cv::CAP_ANY; // 0 = autodetect default API
vector<uchar> g_buffer;

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

DLL_EXPORT int fswebcam_initialize(int p_deviceId, int p_apiID)
{
    int lResult = FSWEBCAM_SUCCESS;

    if (g_pCapture)
    {
        delete g_pCapture;
        g_pCapture = 0;
    }

    g_deviceID = p_deviceId;
    g_apiID = p_apiID;

    lResult = OpenVideoCaptureDevice();

    return lResult;
}

DLL_EXPORT int fswebcam_release()
{
    int lResult = FSWEBCAM_SUCCESS;

    if (g_pCapture)
    {
        delete g_pCapture;
        g_pCapture = 0;
        lResult = FSWEBCAM_SUCCESS;
    }
    
    return 0;
}

int fswebcam_try_recover()
{
    int lResult = FSWEBCAM_FAILURE;

    fswebcam_release();

    lResult = OpenVideoCaptureDevice();

    return lResult;
}

DLL_EXPORT int fswebcam_take_picture_buffer_size(unsigned int* p_pBufferSize)
{
    int lResult = FSWEBCAM_FAILURE;
    Mat frame;

    lResult = OpenVideoCaptureDevice();

    if (lResult == FSWEBCAM_SUCCESS)
    {
        g_buffer.clear();

        if (g_pCapture && g_pCapture->read(frame))
        {
            if (imencode(".jpg", frame, g_buffer))
            {
                *p_pBufferSize = (unsigned int)g_buffer.size();
            }
            else
            {
                lResult = FSWEBCAM_FAILURE;
                printf("ERROR: camera device id = '%d', app id = %d frame cannot be encoded\n", g_deviceID, g_apiID);
            }
        }
        else
        {
            lResult = FSWEBCAM_FAILURE;
            printf("ERROR: camera device id = '%d', app id = %d frame cannot be read, try recover...\n", g_deviceID, g_apiID);

            if (fswebcam_try_recover() == FSWEBCAM_SUCCESS)
            {
                lResult = fswebcam_take_picture_buffer_size(p_pBufferSize);
            }
        }
    }

    return lResult;
}

DLL_EXPORT int fswebcam_take_picture_buffer(unsigned char* p_Buffer, unsigned int p_BufferSize)
{
    int lResult = FSWEBCAM_FAILURE;

    if (p_BufferSize >= g_buffer.size())
    {
        memcpy(p_Buffer, g_buffer.data(), g_buffer.size());
        lResult = FSWEBCAM_SUCCESS;
    }

    return lResult;
}

DLL_EXPORT int fswebcam_take_picture(char* p_pFileName)
{
    int lResult = FSWEBCAM_FAILURE;
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
                fileHandler = fopen(p_pFileName, "wb");

                if (fileHandler)
                {
                    fwrite(buf.data(), sizeof(uchar), buf.size(), fileHandler);

                    lResult = FSWEBCAM_SUCCESS;

                    fclose(fileHandler);
                }
                else
                {
                    printf("ERROR: file '%s' cannot be opened, error code = %d\n", p_pFileName);
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

    return lResult;
}
