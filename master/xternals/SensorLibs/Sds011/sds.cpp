#include <stdlib.h>
#include "sds/sds_helper.h"
#include "sds/sds011_lib.h"
#include "sds.h"

//port = "/dev/ttyUSB0";

DLL_EXPORT int sds_pm_read(char* p_pPort, float* p_pPm25, float* p_pPm10)
{
    int lResult = EXIT_FAILURE;

#ifdef __arm__
    settings action;
    
    SetDefaultSettings(&action);

    action.m_lQueryCount = 1;

    if ((lResult = SdsAction(p_pPort, action)) == EXIT_SUCCESS)
    {
        if (p_pPm25) *p_pPm25 = action.m_fPm25;
        if (p_pPm10) *p_pPm10 = action.m_fPm10;

        lResult = EXIT_SUCCESS;
    }
#else
    if (p_pPm25) *p_pPm25 = (float)(rand() % 80 + 1);
    if (p_pPm10) *p_pPm10 = (float)(rand() % 50 + 1);

    lResult = EXIT_SUCCESS;
#endif    
    
    return lResult;
}

DLL_EXPORT int sds_get_working_period(char* p_pPort, int* p_plWorkingPeriod)
{
    int lResult = EXIT_SUCCESS;
#ifdef __arm__
    settings action;

    SetDefaultSettings(&action);

    action.m_oGetWorkingPeriod = true;

    if ((lResult = SdsAction(p_pPort, action)) == EXIT_SUCCESS)
    {
        *p_plWorkingPeriod = action.m_bSetWorkingPeriod;
    }
#endif

    return lResult;
}

DLL_EXPORT int sds_set_working_period(char* p_pPort, int p_lWorkingPeriod)
{
    int lResult = EXIT_SUCCESS;

#ifdef __arm__
    settings action;

    SetDefaultSettings(&action);

    action.m_bSetWorkingPeriod = (uint8_t)p_lWorkingPeriod;

    lResult = SdsAction(p_pPort, action);
#endif    

    return lResult;
}

DLL_EXPORT int sds_get_reporting_mode(char* p_pPort, int* p_plReportingMode)
{
    int lResult = EXIT_SUCCESS;

#ifdef __arm__
    settings action;

    SetDefaultSettings(&action);

    action.m_oGetReportingMode = true;

    if ((lResult = SdsAction(p_pPort, action)) == EXIT_SUCCESS)
    {
        *p_plReportingMode = (int)action.m_eSetReportingMode;
    }
#endif    

    return lResult;
}

DLL_EXPORT int sds_set_reporting_mode(char* p_pPort, int p_lReportingMode)
{
    int lResult = EXIT_SUCCESS;

#ifdef __arm__
    settings action;

    SetDefaultSettings(&action);

    action.m_eSetReportingMode = (ESdsReportingMode)p_lReportingMode;

    lResult = SdsAction(p_pPort, action);
#endif    

    return lResult;
}

DLL_EXPORT int sds_get_work_mode(char* p_pPort, int* p_plWorkMode)
{
    int lResult = EXIT_SUCCESS;

#ifdef __arm__
    settings action;

    SetDefaultSettings(&action);

    action.m_oGetWorkingMode = true;

    if ((lResult = SdsAction(p_pPort, action)) == EXIT_SUCCESS)
    {
        *p_plWorkMode = (int)action.m_eSetWorkingMode;
    }
#endif    

    return lResult;
}

DLL_EXPORT int sds_set_work_mode(char* p_pPort, int p_lWorkMode)
{
    int lResult = EXIT_SUCCESS;

#ifdef __arm__
    settings action;

    SetDefaultSettings(&action);

    action.m_eSetWorkingMode = (ESdsWorkMode)p_lWorkMode;

    lResult = SdsAction(p_pPort, action);
#endif    

    return lResult;
}