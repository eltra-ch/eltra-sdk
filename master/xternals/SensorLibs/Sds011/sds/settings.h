#ifndef _SETTINGS_H
#define _SETTINGS_H

#include <stdint.h>
#include "sds_def.h"

struct settings
{
    bool        m_oSetupSerial;
    bool        m_oGetFirmwareVersion;     // get firmware
    bool        m_oGetDeviceId;            // get device id
    bool        m_oGetWorkingMode;         // get working mode (sleep/working)
    bool        m_oGetWorkingPeriod;       // get working period (continuous or interval)
    bool        m_oGetReportingMode;       // get reporting mode (reporting/ query)

    bool              m_oGetData;          // get data
    int               m_lQueryCount;       // how many reads in query mode
    
    bool              m_oChangeDeviceId;   // change devid
    uint8_t           m_NewDeviceId[2];    // hold new device id
    ESdsReportingMode m_eSetReportingMode; // set working mode
    ESdsWorkMode      m_eSetWorkingMode;   // set working mode
    uint8_t           m_bSetWorkingPeriod; // set working period

    float m_fPm25;
    float m_fPm10;
};

#endif