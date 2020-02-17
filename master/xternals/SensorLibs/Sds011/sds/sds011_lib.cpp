#include "sds011_lib.h"

uint8_t SDS011_Packet[SDS011_SENDPACKET_LEN];

bool    _PendingConfReq = false;    // indicate configuration request pending
bool    _TryConnect = false;        // when try to connect no display
uint8_t _dev_id[2]= {0xff,0xff};    // device ID
float   _RelativeHumidity = 0;      // for humidity correction
int     _fd;                        // file description to use

uint8_t CalcChecksum(const uint8_t *packet, uint8_t length)
{
    uint8_t checksum = 0, counter;

    for (counter=0; counter<length; counter++) 
    {
        checksum += packet[counter];
    }

    return checksum;
}

uint8_t SdsProcessResponse( const uint8_t *packet, uint8_t length, sds011_response_t *ret)
{
    if (length != SDS011_PACKET_LEN || packet[0] != SDS011_BYTE_BEGIN || packet[length - 1] != SDS011_BYTE_END)
    {
        return SDS011_ERROR;
    }

    // check CRC
    if (packet[8] != CalcChecksum(packet + 2, 6))
    {
        return SDS011_ERROR;
    }

    // set device ID
    ret->devid = (packet[7] << 8) + packet[6];

    if (packet[1] == SDS011_DATA) 
    {
        ret->cmd_id = SDS011_DATA;
        ret->pm25 = (float)(((packet[3] * 256) + packet[2]) / 10.0);
        ret->pm10 = (float)(((packet[5] * 256) + packet[4]) / 10.0);

        /* Humidity correction factor to apply (see detailed document) */
        if (_RelativeHumidity)
        {
            ret->pm25 = (float)(ret->pm25 * 2.8 * pow((100 - _RelativeHumidity), -0.3745));
        }

    } 
    else if (packet[1] == SDS011_CONF) 
    {
        ret->cmd_id = packet[2];

        switch (packet[2])
        {
            case SDS011_SLEEP:
            case SDS011_MODE:
                ret->type = packet[3];
                ret->mode = packet[4];

            case SDS011_DEVID:
                break;

            case SDS011_PERIOD:
                ret->type = packet[3];
                ret->value = packet[4];
                break;

            case SDS011_FWVER:
                ret->year = packet[3];
                ret->month = packet[4];
                ret->day = packet[5];
                break;

            default: return SDS011_ERROR;
        }
    } 
    else 
    {
        return SDS011_ERROR;
    }

    return SDS011_OK;
}

int Wait_For_answer(CSdsState& state)
{
    int i = 0;      
    
    if (_PendingConfReq) 
    {
        while (_PendingConfReq)
        {
            if (i++ > 20)
            {
                return(SDS011_ERROR);
            }
            
            SdsRead(&state);
        }
    }

    return(SDS011_OK);
}

int GetReportingMode(ESdsReportingMode& p_eMode)
{
    int lResult = EXIT_FAILURE;
    CSdsState state;
    SdsPreparePacket(SDS011_MODE);

    if (SdsSend(SDS011_Packet) == SDS011_ERROR)
    {
        return (SDS011_ERROR);
    }

    lResult = Wait_For_answer(state);

    p_eMode = state.m_eReportingMode;
        
    return lResult;
}

void SdsPreparePacket(uint8_t data1)
{
    int i;
    for (i = 0; i < SDS011_SENDPACKET_LEN; i++) SDS011_Packet[i]=0x0;

    SDS011_Packet[0] = SDS011_BYTE_BEGIN;
    SDS011_Packet[1] = SDS011_BYTE_CMD;
    SDS011_Packet[2] = data1;
    SDS011_Packet[18] = SDS011_BYTE_END;
}

int SetReportingMode(ESdsReportingMode p_eMode)
{
    SdsPreparePacket(SDS011_MODE);

    int lResult = EXIT_FAILURE;    
    CSdsState state;
    uint8_t rmode;

    switch (p_eMode)
    {
        case SRM_Query: rmode = REPORT_QUERY; break;
        case SRM_Stream: rmode = REPORT_STREAM; break;
        default: rmode = REPORT_QUERY; break;
    }

    SDS011_Packet[3] = 1;
    SDS011_Packet[4] = rmode;

    if (SdsSend(SDS011_Packet) == SDS011_ERROR)
    {
        return (SDS011_ERROR);
    }

    lResult = Wait_For_answer(state);

    return lResult;
}

int GetWorkingMode(ESdsWorkMode& p_eWorkMode)
{
    int lResult = EXIT_FAILURE;
    CSdsState state;

    SdsPreparePacket(SDS011_SLEEP);

    if (SdsSend(SDS011_Packet) == SDS011_ERROR)
    {
        return (SDS011_ERROR);
    }

    lResult = Wait_For_answer(state);

    p_eWorkMode = state.m_eWorkMode;
    
    return lResult;
}

int SetWorkingMode(ESdsWorkMode p_eWorkMode)
{
    int lResult = EXIT_FAILURE;
    CSdsState state;
    uint8_t rmode;

    switch (p_eWorkMode)
    {
        case WM_Working: rmode = MODE_WORK; break;
        case WM_Sleep: rmode = MODE_SLEEP; break;
        default: rmode = MODE_WORK; break;
    }

    SdsPreparePacket(SDS011_SLEEP);

    SDS011_Packet[3] = 1;
    SDS011_Packet[4] = rmode;

    if (SdsSend(SDS011_Packet) == SDS011_ERROR)
    {
        return (SDS011_ERROR);
    }

    lResult = Wait_For_answer(state);

    return lResult;
}

int GetWorkingPeriod(uint8_t& p_rWorkingPeriod)
{
    int lResult = EXIT_FAILURE;
    CSdsState state;

    SdsPreparePacket(SDS011_PERIOD);

    if (SdsSend(SDS011_Packet) == SDS011_ERROR)
    {
        return (SDS011_ERROR);
    }

    lResult = Wait_For_answer(state);

    p_rWorkingPeriod = state.m_sWorkingPeriod;

    return lResult;
}

int SetWorkingPeriod(uint8_t p_WorkingPeriod) 
{
    int lResult = EXIT_FAILURE;
    CSdsState state;

    if (p_WorkingPeriod < 0 || p_WorkingPeriod > 30) 
    {
        printf(" %d is invalid period, must be 0 to 30 minutes\n", p_WorkingPeriod);
        return(SDS011_ERROR);
    }

    SdsPreparePacket(SDS011_PERIOD);

    SDS011_Packet[3] = 1;
    SDS011_Packet[4] = p_WorkingPeriod;

    if (SdsSend(SDS011_Packet) == SDS011_ERROR)
    {
        return (SDS011_ERROR);
    }

    lResult = Wait_For_answer(state);

    return lResult;
}

int QueryData(float& p_rPm25, float& p_rPm10)
{
    CSdsState state;
    
    SdsPreparePacket(SDS011_QDATA);

    if (SdsSend(SDS011_Packet) == SDS011_ERROR)
    {
        goto q_error;
    }

    // read / parse response from sds
    if (SdsRead(&state) == SDS011_ERROR)
    {
        goto q_error;
    }

    p_rPm25 = state.m_pm25;
    p_rPm10 = state.m_pm10;

    return(SDS011_OK);

q_error:
    
    return (SDS011_ERROR);
}

int GetFirmwareVersion()
{
    int lResult = SDS011_ERROR;
    CSdsState state;

    SdsPreparePacket(SDS011_FWVER);

    if (SdsSend(SDS011_Packet) == SDS011_ERROR)
    {
        return (SDS011_ERROR);
    }

    lResult = Wait_For_answer(state);

    return lResult;
}

int SdsSetHumidityCorrection (float h)
{
    if (h < 1 || h > 100)
    {
        return(SDS011_ERROR);
    }
   
   _RelativeHumidity = h;

   return(SDS011_OK);
}

int SdsConnect(int fd)
{
    _fd = fd;
    SdsPreparePacket(SDS011_FWVER);

    if (SdsSend(SDS011_Packet) == SDS011_ERROR)
    {
        return (SDS011_ERROR);
    }

    _TryConnect = true;      // prevent display results
    int i = 0;              // prevent deadlock
    int j = 0;

    while (_PendingConfReq)  // as long as no answer
    {
#ifdef __arm__
        usleep(10000);      // wait
#endif
        SdsRead(NULL);  // check for answer

        if(i++ == 2 && _PendingConfReq) 
        {
            i = 0;
            // resubmit
            _PendingConfReq = false;      // enable resend

            if (SdsSend(SDS011_Packet) == SDS011_ERROR || j++ > 10)
            {
                 _TryConnect = false;     // enable display results
                 return(SDS011_ERROR);
            }
        }
    }

    _TryConnect = false;     // enable display results
    
    return(SDS011_OK);
}

uint16_t SdsGetDeviceID()
{
    return((_dev_id[1]<<8) + _dev_id[0]);
}

int SetNewDevId(uint8_t *newid)
{
    CSdsState state;

    // create command
    SdsPreparePacket(SDS011_DEVID);
    
    SDS011_Packet[13] = newid[0];
    SDS011_Packet[14] = newid[1];

    // send it
    if (SdsSend(SDS011_Packet) == SDS011_ERROR)
    {
        printf((char *)"Error during sending\n");
        return (SDS011_ERROR);
    }

    return(Wait_For_answer(state));
}

int SdsSend(uint8_t *p_pDataPacket) 
{    
    CSdsState state;

    if (Wait_For_answer(state) == SDS011_ERROR)
    {
        return(SDS011_ERROR);
    }

    // add device ID
    p_pDataPacket[15] = _dev_id[0];
    p_pDataPacket[16] = _dev_id[1];

    // add crc
    p_pDataPacket[17] = CalcChecksum(p_pDataPacket+2, 15);

#ifdef __arm__
    // send command
    if (write(_fd, p_pDataPacket, SDS011_SENDPACKET_LEN) != SDS011_SENDPACKET_LEN)
    {
        return(SDS011_ERROR);
    }
#endif
    // indicate pending config request (except when requested data)
    if (p_pDataPacket[2] != SDS011_QDATA)
    {
        _PendingConfReq = true;
    }

    return(SDS011_OK);
}

int SdsRead(CSdsState* p_pState)
{
    uint8_t buf[20];
    int num = 0;
    sds011_response_t data;
    
#ifdef __arm__
    num = read(_fd, buf, sizeof(buf));
#endif

    if (SdsProcessResponse(buf, num, &data) == SDS011_OK)
    {
        switch (data.cmd_id)
        {
        case SDS011_DATA:
        {
            if (!_PendingConfReq && p_pState)
            {
                p_pState->m_pm25 = data.pm25;
                p_pState->m_pm10 = data.pm10;
            }

            goto add_dev;
            break;
        }
        case SDS011_MODE:
        {
            if (p_pState)
            {
                data.type ? p_pState->m_oModeSet = true : p_pState->m_oModeSet = false;
                data.mode ? p_pState->m_eReportingMode = SRM_Query : p_pState->m_eReportingMode = SRM_Stream;
            }

            goto res_pending;
            break;
        }
        case SDS011_DEVID:
        {
            if (p_pState)
            {
                p_pState->m_lDeviceId = data.devid;
            }

            goto res_pending;
            break;
        }
        case SDS011_SLEEP:
        {
            if (p_pState)
            {
                data.type ? p_pState->m_oModeSet = true : p_pState->m_oModeSet = false;
                data.mode ? p_pState->m_eWorkMode = WM_Working : p_pState->m_eWorkMode = WM_Sleep;
            }

            goto res_pending;
            break;
        }
        case SDS011_PERIOD:
        {
            if (p_pState)
            {
                data.type ? p_pState->m_oModeSet = true : p_pState->m_oModeSet = false;
                p_pState->m_sWorkingPeriod = data.value;
            }

            goto res_pending;
            break;
        }
        case SDS011_FWVER:
        {
            if (_TryConnect == false)
            {
                printf("Date: %" PRIu8 "-%" PRIu8 "-%" PRIu8 "\n", data.year, data.month, data.day);
            }

            goto res_pending;
            break;
        }
        default:
        {
            if (p_pState)
            {
                p_pState->m_oError = true;
            }
            return(SDS011_ERROR);
        }
        }
    }
res_pending:
    _PendingConfReq = false;
add_dev:
    _dev_id[0] = data.devid & 0xff;
    _dev_id[1] = (data.devid >> 8) & 0xff;
    
    return(SDS011_OK);
}
