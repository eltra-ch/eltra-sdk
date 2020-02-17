#ifndef _SDS_HELPER
#define _SDS_HELPER

#include "settings.h"

int     SdsAction(const char* p_pPort, settings& action);
int     CloseSerial(int p_lFd);
void    SetDefaultSettings(settings* action);
int     ExecuteAction(int p_lFd, settings* action);
void    FixSerialConverter(int p_lFd);
void    SetupSerial();
int     OpenSerial(const char* p_pPort, int& p_rFd);

#endif