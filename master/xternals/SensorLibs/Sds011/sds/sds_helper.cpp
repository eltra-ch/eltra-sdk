#include "sds011_lib.h"
#include "serial.h"
#include "../sys_helper.h"
#include "sds_helper.h"

int SdsAction(const char* p_pPort, settings& action)
{
    int lFd = 0xff;
    int lResult = EXIT_FAILURE;

    if (action.m_oSetupSerial)
    {
        SetupSerial();
    }

    if ((lResult = OpenSerial(p_pPort, lFd)) == EXIT_SUCCESS)
    {
        FixSerialConverter(lFd);

        if (SdsConnect(lFd) == SDS011_ERROR)
        {
            lResult = EXIT_FAILURE;
        }
        else
        {
            lResult = ExecuteAction(lFd, &action);
        }

        CloseSerial(lFd);
    }

    return lResult;
}

int CloseSerial(int p_lFd)
{
    int lResult = EXIT_SUCCESS;

    if (p_lFd != 0xff)
    {
        if (restore_ser(p_lFd) == EXIT_SUCCESS)
        {
#ifdef __arm__
            close(p_lFd);
#endif
        }
        else
        {
            lResult = EXIT_FAILURE;
        }        
    }

    return lResult;
}

void SetDefaultSettings(settings* p_pAction)
{
    if (p_pAction)
    {
        p_pAction->m_oSetupSerial = true;
        p_pAction->m_oGetFirmwareVersion = false;  // get firmware
        p_pAction->m_oGetDeviceId = false;         // get device id
        p_pAction->m_oGetWorkingMode = false;      // get working mode (sleep/working)
        p_pAction->m_oGetWorkingPeriod = false;    // get working period (continuous or interval)
        p_pAction->m_oGetReportingMode = false;    // get reporting mode (reporting/ query)

        p_pAction->m_oGetData = false;             // get data
        p_pAction->m_lQueryCount = 0xff;           // how many reads in query mode
        
        p_pAction->m_oChangeDeviceId = false;      // change device id

        p_pAction->m_eSetReportingMode = SRM_Undefined;     // set working mode
        p_pAction->m_eSetWorkingMode = WM_Undefined;       // set working mode
        p_pAction->m_bSetWorkingPeriod = 0xff;     // set working period
    }
}

int ExecuteAction(int p_lFd, settings* p_pAction)
{
    int lResult = EXIT_SUCCESS;

    if (p_pAction->m_oGetFirmwareVersion)
    {
        if (GetFirmwareVersion() == SDS011_ERROR)
        {            
            lResult = EXIT_FAILURE;
        }
    }

    if (p_pAction->m_oGetDeviceId)
    {
        printf("Current DeviceID: 0x%04x\n", SdsGetDeviceID());
    }

    if (p_pAction->m_oGetReportingMode)
    {        
        if (GetReportingMode(p_pAction->m_eSetReportingMode) == SDS011_ERROR)
        {            
            lResult = EXIT_FAILURE;
        }
    }

    if (p_pAction->m_oGetWorkingMode)
    {        
        if (GetWorkingMode(p_pAction->m_eSetWorkingMode) == SDS011_ERROR)
        {
            lResult = EXIT_FAILURE;
        }
    }

    if (p_pAction->m_oGetWorkingPeriod)
    {
        if (GetWorkingPeriod(p_pAction->m_bSetWorkingPeriod) == SDS011_ERROR)
        {
            lResult = EXIT_FAILURE;
        }
    }

    if (p_pAction->m_oChangeDeviceId)
    {
        if (SetNewDevId(p_pAction->m_NewDeviceId) == SDS011_ERROR)
        {
            lResult = EXIT_FAILURE;
        }
    }

    if (p_pAction->m_eSetWorkingMode != 0xff)
    {
        if (SetWorkingMode(p_pAction->m_eSetWorkingMode) == SDS011_ERROR)
        {
            lResult = EXIT_FAILURE;
        }
    }

    if (p_pAction->m_bSetWorkingPeriod != 0xff)
    {
        // set working period to every x minutes
        // 0 = set working period to continuous
        // needs 30 seconds before the first result will show

        if (SetWorkingPeriod(p_pAction->m_bSetWorkingPeriod) == SDS011_ERROR)
        {
            lResult = EXIT_FAILURE;
        }
    }

    if (p_pAction->m_eSetReportingMode != 0xff)
    {
        // set query mode  or reporting/streaming mode
        if (SetReportingMode(p_pAction->m_eSetReportingMode) == SDS011_ERROR)
        {
            lResult = EXIT_FAILURE;
        }
    }

    if (p_pAction->m_lQueryCount != 0xff)
    {
        float fPm25 = 0;
        float fPm10 = 0;

        if (QueryData(fPm25, fPm10) == SDS011_ERROR)
        {
            lResult = EXIT_FAILURE;
        }
        else
        {
            if (p_pAction)
            {
                p_pAction->m_fPm25 = fPm25;
                p_pAction->m_fPm10 = fPm10;
            }
            
            lResult = EXIT_SUCCESS;
        }
    }

    return lResult;
}

int OpenSerial(const char* p_pPort, int& p_rFd)
{
    int lResult = EXIT_FAILURE;
#ifdef __arm__
    p_rFd = open(p_pPort, O_RDWR | O_NOCTTY | O_SYNC);

    if (p_rFd >= 0)
    {
        if (ConfigureSerial(p_rFd, B9600) == EXIT_SUCCESS)
        {
            if (set_blocking(p_rFd, 0) == EXIT_SUCCESS)
            {
                lResult = EXIT_SUCCESS;
            }
        }
    }
    else
    {
        printf("could not open %s\n", p_pPort);
    }
#endif
    return lResult;
}

void SetupSerial()
{
    // you need the driver to be loaded before opening /dev/ttyUSBx
    // otherwise it will hang. The SDS-011 has an HL-341 chip, checked
    // with lsusb. The name of the driver is ch341.
    // One can change the system setup so this is done automatically
    // when reboot and then you can remove the call below. 
    
	if(!IsModuleLoaded("usbserial"))
	{
		system("modprobe usbserial");
	}
		
	if(!IsModuleLoaded("ch341"))
	{
		system("modprobe ch341");
	}
}

void FixSerialConverter(int p_lFd)
{
    // There is a problem with flushing buffers on a serial USB that can
   // not be solved. The only thing one can try is to flush any buffers
   // after some delay:
   //
   // https://bugzilla.kernel.org/show_bug.cgi?id=5730
   // https://stackoverflow.com/questions/13013387/clearing-the-serial-ports-buffer
   //
#ifdef __arm__
    usleep(10000);                      // required to make flush work, for some reason

    tcflush(p_lFd, TCIOFLUSH);
#endif
}