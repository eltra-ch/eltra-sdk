#ifndef _SDS011_H
#define _SDS011_H

#ifdef __cplusplus
extern "C" {
#endif

#include <errno.h>
#include <fcntl.h>
#include <string.h>
#ifdef __arm__
#include <termios.h>
#include <unistd.h>
#include <getopt.h>
#endif
#include <time.h>
#include <inttypes.h>
#include <stdlib.h>
#include <stdio.h>
#include <stdint.h>
#include <signal.h>
#include <stdbool.h>
#include <stdarg.h>
#include <math.h>
#include "sds_response.h"
#include "sds_state.h"

// Command IDs
#define SDS011_MODE   0x02 // Set data reporting mode (3rd byte)
#define SDS011_QDATA  0x04 // Get data if in Queary mode (3rd byte)
#define SDS011_DEVID  0x05 // Set device ID (3rd byte)
#define SDS011_SLEEP  0x06 // Set sleep and work (3rd byte)
#define SDS011_FWVER  0x07 // Get firmware version (3rd byte)
#define SDS011_PERIOD 0x08 // Set working period (3rd byte)
#define SDS011_BYTE_BEGIN 0xAA // 1st byte of message
#define SDS011_BYTE_END   0xAB // Last byte of message

//(µC/PC/ -> Sensor)
#define SDS011_BYTE_CMD   0xB4   // sending message (2nd byte)
#define SDS011_SENDPACKET_LEN 19 // Number of bytes per sending packet

//(Sensor -> µC/PC)
#define SDS011_DATA   0xC0       // Measured data (2nd byte)
#define SDS011_CONF   0xC5       // Configuration mode response (2nd byte)
#define SDS011_PACKET_LEN   10   // Number of bytes per response

// status
#define SDS011_OK    0x00
#define SDS011_ERROR 0xFF

// Set data reporting mode
#define REPORT_STREAM 0x00
#define REPORT_QUERY  0x01

// sleep / work mode
#define MODE_SLEEP    0x0
#define MODE_WORK     0x1

int GetFirmwareVersion();
int SetReportingMode(ESdsReportingMode p_eMode);
int GetReportingMode(ESdsReportingMode& p_reMode);
int SetWorkingMode(ESdsWorkMode p_eWorkMode) ;
int GetWorkingMode(ESdsWorkMode& p_reWorkMode);
int SetWorkingPeriod(uint8_t period);
int GetWorkingPeriod(uint8_t& p_rWorkingPeriod);
int SetNewDevId(uint8_t * newid);
int QueryData(float& p_rPm25, float& p_rPm10);

void SdsPreparePacket(uint8_t data1);
int SdsRead(CSdsState* p_pState);
int SdsSend(uint8_t * packet);
int SdsSetHumidityCorrection(float h);
int SdsConnect(int fd);
uint16_t SdsGetDeviceID();

#ifdef __cplusplus
}
#endif

#endif /* _SDS011_H */
