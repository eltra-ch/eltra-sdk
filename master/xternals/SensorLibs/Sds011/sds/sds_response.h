#pragma once

#include <stdint.h>

struct sds011_response_t
{
    uint8_t cmd_id; // Command ID
    uint8_t type;   // 0=Query current mode, 1=Set mode
    uint8_t mode;   //
    uint8_t value;  // 0=Continous, 1-30 (mins) [work 30 sec and sleep n*60-30 sec]
    uint16_t devid; // Device ID
    uint8_t year;   // Year
    uint8_t month;  // Month
    uint8_t day;    // Day
    float pm25;     // PM 2.5
    float pm10;     // PM 10
};