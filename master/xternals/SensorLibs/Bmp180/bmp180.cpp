/*  (Dev)   -  (Pi)
    SDA     -  SDA
    SCL     -  SCL
    GND     -  GND
    VCC     -  3.3V

   based on code: Eric Maasdorp
*/

#include <stdio.h>
#include <stdint.h>
#include <fcntl.h>
#include <stdlib.h>
#include <errno.h>
#ifdef __arm__
#include <unistd.h>
#include <linux/i2c-dev.h>
#include <linux/i2c.h>
#include <sys/ioctl.h>
#endif
#include <math.h>
#include "bmp180.h"

// BMP085 & BMP180 Specific code
#define BMPx8x_I2CADDR           0x77
#define BMPx8x_CtrlMeas          0xF4
#define BMPx8x_TempConversion    0x2E
#define BMPx8x_PresConversion0   0x34
#define BMPx8x_Results           0xF6
#define BMPx8x_minDelay          4     //require 4.5ms *1000/700 'turbo mode fix'= 6.4-Retry =4.4
#define BMPx8x_RetryDelay        2     //min delay for temp 4+2=6ms, max 4+2*20=44ms for pressure
                                       //Will stop waiting if conversion is complete

const unsigned char BMPx8x_OverSampling = 3;

// Calibration values - These are stored in the BMP085/180
short int            bmp_ac1;
short int            bmp_ac2;
short int            bmp_ac3;
unsigned short int   bmp_ac4;
unsigned short int   bmp_ac5;
unsigned short int   bmp_ac6;
short int            bmp_b1;
short int            bmp_b2;
int                  bmp_b5;
short int            bmp_mb;
short int            bmp_mc;
short int            bmp_md;

#ifdef __arm__
#define sleepms(ms)  usleep((ms)*1000)
#endif

#define I2CBusName           "/dev/i2c-1"    //New Pi's
//#define I2CBusName         "/dev/i2c-0"    //Old, but not stale Pi's

int      bmp_read_int(int p_handle, uint8_t* p_dev_values, uint8_t p_start_reg, uint8_t p_bytes_to_read);
int      bmp_write_cmd(int p_handle, uint8_t p_dev_action);
int      bmp_Calibration(int p_handle);
int      bmp_WaitForConversion(int p_handle);
int      bmp_get_pressure(int p_handle, double* p_pPressure);
int      bmp_get_temperature(int p_handle, double* p_pTemperature);
double   bmp_get_altitude(double p_pressure);
double   bmp_get_qnh(double p_pressure, double p_station_altitude);
double   ppl_get_density_alt(double p_station_altitude, double p_temperature);

#ifdef __arm__

int i2c_open(const char *p_bus_name)
{
  int handle = -1;
  
  handle = open(p_bus_name, O_RDWR);

  return handle;
}

// BMP085 & BMP180 Specific code

int bmp_read_int(int p_handle, uint8_t *p_dev_values,uint8_t p_start_reg,uint8_t p_bytes_to_read)
{
  int rc;
  struct i2c_rdwr_ioctl_data messagebuffer;

  //Build a register read command
  //Requires a one complete message containing a command
  //and anaother complete message for the reply
  struct i2c_msg read_reg[2]={
    {BMPx8x_I2CADDR,0,1,&p_start_reg},
    {BMPx8x_I2CADDR,I2C_M_RD,p_bytes_to_read,p_dev_values}
  };

  messagebuffer.nmsgs = 2;                  //Two message/action
  messagebuffer.msgs = read_reg;            //load the 'read__reg' message into the buffer
  
  rc = ioctl(p_handle, I2C_RDWR, &messagebuffer); //Send the buffer to the bus and returns a send status
  
  if (rc < 0 )
  {
    return rc;       //Use this line if it must return to the caller for processing
  }
  
  //note that the return data is contained in the array pointed to by devValues (passed by-ref)
  return 0;
}

int bmp_write_cmd(int p_handle, uint8_t p_dev_action)
{
  int rc;
  struct i2c_rdwr_ioctl_data messagebuffer;
  uint8_t datatosend[2];

  datatosend[0]=BMPx8x_CtrlMeas;
  datatosend[1]=p_dev_action;
  //Build a register write command
  //Requires one complete message containing a reg address and command
  struct i2c_msg write_reg[1]=
  {
    {BMPx8x_I2CADDR,0,2,datatosend}
  };

  messagebuffer.nmsgs = 1;                  //One message/action
  messagebuffer.msgs = write_reg;           //load the 'write__reg' message into the buffer
  
  rc = ioctl(p_handle, I2C_RDWR, &messagebuffer); //Send the buffer to the bus and returns a send status
  
  if (rc < 0 ) {
    return rc;       //Use this line if it must return to the caller for processing
  }

  return 0;
}

int bmp_Calibration(int p_handle)
{
  uint8_t rValue[21];
  
  if (bmp_read_int(p_handle, rValue, 0xAA, 22) == 0)
  {
    bmp_ac1=((rValue[0]<<8)|rValue[1]);
    bmp_ac2=((rValue[2]<<8)|rValue[3]);
    bmp_ac3=((rValue[4]<<8)|rValue[5]);
    bmp_ac4=((rValue[6]<<8)|rValue[7]);
    bmp_ac5=((rValue[8]<<8)|rValue[9]);
    bmp_ac6=((rValue[10]<<8)|rValue[11]);
    bmp_b1=((rValue[12]<<8)|rValue[13]);
    bmp_b2=((rValue[14]<<8)|rValue[15]);
    bmp_mb=((rValue[16]<<8)|rValue[17]);
    bmp_mc=((rValue[18]<<8)|rValue[19]);
    bmp_md=((rValue[20]<<8)|rValue[21]);

    return 0;	
  }

  return -1;
}

int bmp_WaitForConversion(int p_handle)
{
  uint8_t rValues[3];
  int counter=0;
  //Delay can now be reduced by checking that bit 5 of Ctrl_Meas(0xF4) == 0
  do
  {
    sleepms (BMPx8x_RetryDelay);

    if (bmp_read_int(p_handle,rValues,BMPx8x_CtrlMeas,1) != 0 ) return -1;
    
    counter++;
    //printf("GetPressure:\t Loop:%i\trValues:0x%0x\n",counter,rValues[0]);
  } while ( ((rValues[0] & 0x20) != 0)  &&  counter < 20 );  

  return 0;
}

// Calculate calibrated pressure 
// Value returned will be in hPa
int bmp_get_pressure(int p_handle, double *p_pPressure)
{
  unsigned int up;  
  uint8_t rValues[3];
    
  // Pressure conversion with oversampling 0x34+ BMPx8x_OverSampling 'bit shifted'
  if (bmp_write_cmd(p_handle, (BMPx8x_PresConversion0+(BMPx8x_OverSampling<<6)))!=0) return -1;
  
  //Delay gets longer the higher the oversampling must be at least 26 ms plus a bit for turbo 
  //clock error ie 26 * 1000/700 or 38 ms
  //sleepms (BMPx8x_minDelay + (4<<BMPx8x_OverSampling));  //39ms at oversample = 3
  
  //Code is now 'turbo' overclock independent 
  sleepms (BMPx8x_minDelay);
  
  if (bmp_WaitForConversion(p_handle) !=0 ) return -1;  
  
  //printf ("\nDelay:%i\n",(BMPx8x_minDelay+(4<<BMPx8x_OverSampling))); 
  if (bmp_read_int(p_handle, rValues, BMPx8x_Results,3) !=0 ) return -1;
  
  up = (((unsigned int) rValues[0] << 16) | ((unsigned int) rValues[1] << 8) | (unsigned int) rValues[2]) >> (8-BMPx8x_OverSampling);
  
  int x1, x2, x3, b3, b6, p;
  unsigned int b4, b7;
  
  b6 = bmp_b5 - 4000;
  x1 = (bmp_b2 * (b6 * b6)>>12)>>11;
  x2 = (bmp_ac2 * b6)>>11;
  x3 = x1 + x2;
  b3 = (((((int)bmp_ac1)*4 + x3)<<BMPx8x_OverSampling) + 2)>>2;
  
  x1 = (bmp_ac3 * b6)>>13;
  x2 = (bmp_b1 * ((b6 * b6)>>12))>>16;
  x3 = ((x1 + x2) + 2)>>2;
  b4 = (bmp_ac4 * (unsigned int)(x3 + 32768))>>15;
  
  b7 = ((unsigned int)(up - b3) * (50000>>BMPx8x_OverSampling));
  
  if (b7 < 0x80000000)
    p = (b7<<1)/b4;
  else
    p = (b7/b4)<<1;
	
  x1 = (p>>8) * (p>>8);
  x1 = (x1 * 3038)>>16;
  x2 = (-7357 * p)>>16;
  
  p += (x1 + x2 + 3791)>>4;
  
  *p_pPressure = ((double)p/100);  

  return 0;
}

// Calculate calibrated temperature
// Value returned will be in units of 0.1 deg C
int bmp_get_temperature(int p_handle, double *p_pTemperature)
{
  unsigned int ut;
  uint8_t rValues[2];
  
  if (bmp_write_cmd(p_handle, BMPx8x_TempConversion)!=0) return -1;
  
  //Code is now 'turbo' overclock independent 
  sleepms (BMPx8x_minDelay);
  
  if (bmp_WaitForConversion(p_handle) !=0 ) return -1;

  if (bmp_read_int(p_handle, rValues, BMPx8x_Results,2) !=0 ) return -1;
  
  ut=((rValues[0]<<8)|rValues[1]);  

  int x1, x2;
  
  x1 = (((int)ut - (int)bmp_ac6)*(int)bmp_ac5) >> 15;
  x2 = ((int)bmp_mc << 11)/(x1 + bmp_md);
  
  bmp_b5 = x1 + x2;

  double result = ((bmp_b5 + 8)>>4);  
  
  *p_pTemperature = result/10;

  return 0;
}

double bmp_get_altitude(double p_pressure)
{
  //return 145437.86*(1- pow((p/1013.25),0.190294496)); //return feet
  return 44330*(1- pow((p_pressure/1013.25),0.190294496)); //return meters
}

double bmp_get_qnh(double p_pressure,double p_station_alt)
{
  //return p / pow((1-(StationAlt/145437.86)),5.255) ; //return hPa based on feet
  return p_pressure / pow((1-(p_station_alt/44330)),5.255) ; //return hPa based on meters
}

double ppl_get_density_alt(double p_altitude,double p_temperature)
{
  double ISA = 15 - (1.98*(p_altitude/1000));

  return p_altitude+(120*(p_temperature-ISA)); //So,So density altitude
}

#endif

DLL_EXPORT int bmp_read(double* p_pTemperature, double* p_pPressure, double* p_pAltitude)
{
    int result = 0;
#ifdef __arm__
    int handle;
    double temperature, pressure;
    double altitude;

    handle = i2c_open(I2CBusName);

    if (handle >= 0)
    {
        if (bmp_Calibration(handle) == 0)
        {
            bmp_get_temperature(handle, &temperature);
            bmp_get_pressure(handle, &pressure);

            altitude = bmp_get_altitude(pressure);

            *p_pTemperature = temperature;
            *p_pPressure = pressure;
            *p_pAltitude = altitude;

            result = 1;
        }

        close(handle);
    }
#else
    *p_pTemperature = (double)(60 - rand() % 60 + 1);
    *p_pPressure = (double)(rand() % 100 + 1);
    *p_pAltitude = (double)(200 + rand() % 100 + 1);

    result = 1;
#endif 

    return result;
}