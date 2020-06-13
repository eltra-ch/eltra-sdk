#include <stdio.h>

#ifdef __arm__
    #include <unistd.h>
    #include <sys/types.h>
    #include <sys/wait.h>
    #include <string.h>
    #include <stdlib.h>
#else
    #include <windows.h>
    #include <opencv2/videoio.hpp>
    #include <opencv2/core.hpp>
    #include <opencv2/highgui.hpp>
#endif

#include <mutex>
#include <stdlib.h>
#include "common.h"
#include "fs_web_cam.h"

using namespace cv;
using namespace std;

DLL_EXPORT int fswebcam_initialize()
{
    int lResult = 0;
#ifdef __arm__

#endif    
    return lResult;
}

DLL_EXPORT int fswebcam_release()
{
    return 0;
}

int ReadFile(char* p_pFileName)
{
    char* pBuffer = 0;
    long lBufferLength = 0;
    FILE* fileHandler = 0; 
    
    int err = fopen_s(&fileHandler, p_pFileName, "rb");

    if (err == 0 && fileHandler)
    {
        fseek(fileHandler, 0, SEEK_END);

        lBufferLength = ftell(fileHandler);

        fseek(fileHandler, 0, SEEK_SET);

        pBuffer = (char*)malloc(lBufferLength);

        if (pBuffer)
        {
            fread(pBuffer, sizeof(char), lBufferLength, fileHandler);
        }

        fclose(fileHandler);
    }

    if (pBuffer)
    {
        free(pBuffer);
    }

    return 0;
}

DLL_EXPORT int fswebcam_take_picture(unsigned short p_usIndex, char* p_pFileName)
{
    int lResult = 0;

#ifdef __arm__
        char* pCmd = new char[255];

        memset(pCmd, 0, 255);

        sprintf(pCmd, "fswebcam --input %d %s", p_usIndex, p_pFileName);
        
        system(pCmd);
        
        delete[] pCmd;

#else
    Mat frame;
    VideoCapture cap;
    vector<uchar> buf;
    FILE* fileHandler = 0;
    int deviceID = 0;             // 0 = open default camera
    int apiID = cv::CAP_ANY;      // 0 = autodetect default API
    
    cap.open(deviceID, apiID);
    
    if (cap.isOpened()) 
    {
        cap.read(frame);
                
        imencode(".jpg", frame, buf);

        int err = fopen_s(&fileHandler, p_pFileName, "wb");
        if (err == 0 && fileHandler)
        {
            fwrite(buf.data(), sizeof(uchar), buf.size(), fileHandler);

            fclose(fileHandler);
        }
        else
        {

        }
    }
    
#endif

    return lResult;
}
