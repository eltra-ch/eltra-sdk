#pragma once

#include <stdlib.h>
#include "common.h"

DLL_EXPORT int sds_pm_read(char* p_pPort, float* p_pfPm25, float* p_pfPm10);
DLL_EXPORT int sds_get_working_period(char* p_pPort, int* p_plWorkingPeriod); 
DLL_EXPORT int sds_set_working_period(char* p_pPort, int p_lWorkingPeriod);
DLL_EXPORT int sds_get_reporting_mode(char* p_pPort, int* p_plReportingMode);
DLL_EXPORT int sds_set_reporting_mode(char* p_pPort, int p_lReportingMode);
DLL_EXPORT int sds_get_work_mode(char* p_pPort, int* p_plWorkMode);
DLL_EXPORT int sds_set_work_mode(char* p_pPort, int p_lWorkMode);