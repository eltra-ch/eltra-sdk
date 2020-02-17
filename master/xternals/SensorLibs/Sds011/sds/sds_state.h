#pragma once

#include <stdint.h>
#include "sds_def.h"

class CSdsState
{
public:
    CSdsState()
    {
        m_pm25 = 0;
        m_pm10 = 0;
        m_eReportingMode = SRM_Query;
        m_oModeSet = false;
        m_lDeviceId = 0;
        m_eWorkMode = WM_Sleep;
        m_sWorkingPeriod = 30;
        m_oError = false;
    }

    float m_pm25;     // PM 2.5
    float m_pm10;     // PM 10
    ESdsReportingMode m_eReportingMode;
    bool m_oModeSet;
    int m_lDeviceId;
    ESdsWorkMode m_eWorkMode;
    uint8_t m_sWorkingPeriod;
    bool m_oError;
};