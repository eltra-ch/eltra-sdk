#include <stdio.h>
#ifdef __arm__
    #include <unistd.h>
    #include <sys/types.h>
    #include <sys/wait.h>
    #include <string.h>
#else 
    #include <windows.h>
#endif
#include <mutex>
#include <stdlib.h>
#include "common.h"
#include "fs_web_cam.h"

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

DLL_EXPORT int fswebcam_take_picture(unsigned short p_usIndex, char* p_pFileName)
{
    int lResult = 0;

#ifdef __arm__
    int child_ret;
    pid_t pid;
    
    pid = fork();

    if (pid == 0) 
    {
        char* pCmd = new char[255];

        memset(pCmd, 0, 255);

        sprintf(pCmd, "fswebcam --input %d %s", p_usIndex, p_pFileName);
        
        execl("/bin/bash", pCmd, NULL);
        
        delete[] pCmd;
    }
    else if(pid > 0)
    {
        wait(0);
    }

#else
    char* pBuffer = 0;
    long lBufferLength = 0;
    FILE* fileHandler = fopen(p_pFileName, "rb");

    if (fileHandler)
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
#endif

    return lResult;
}