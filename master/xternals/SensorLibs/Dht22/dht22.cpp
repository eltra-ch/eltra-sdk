#include <stdio.h>
#ifdef __arm__
    #include <wiringPi.h>
    #include <unistd.h>
#endif
#include <mutex>
#include <stdlib.h>
#include "dht22.h"

short dht22_read_data(unsigned short p_usPin, unsigned short* pData);
int dht22_read_sensor_data(unsigned short p_usPin, int p_lDelay, int p_lRetryCount, unsigned short* pData, double* p_pfTemperature, double* p_fpHumidity);

extern "C" int dht22_initialize()
{
    int lResult = 1;
#ifdef __arm__
    if (wiringPiSetupGpio() == -1)
    {
        lResult = 0;
    }
#endif    
    return lResult;
}

extern "C" int dht22_release()
{
    return 1;
}

extern "C" int digital_io_read(unsigned short p_usPin, int* p_plValue)
{
    int lResult = 0;
        
    if (p_plValue)
    {
#ifdef __arm__
        *p_plValue = digitalRead((int)p_usPin);
#endif
        lResult = 1;
    }
    
    return lResult;
}

extern "C" int digital_io_write(unsigned short p_usPin, int p_lValue)
{
    int lResult = 1;

#ifdef __arm__
    digitalWrite((int)p_usPin, p_lValue);
#endif

    return lResult;
}

extern "C" int set_digital_io_pin_mode(unsigned short p_usPin, int p_lValue)
{
    int lResult = 1;

#ifdef __arm__
    pinMode((int)p_usPin, p_lValue);
#endif

    return lResult;
}

extern "C" int dht22_read(unsigned short p_usPin, int p_lDelay, int p_lRetryCount, double* p_pfTemperature, double* p_fpHumidity)
{
    int result = 0;
    unsigned short data[5] = { 0, 0, 0, 0, 0 };

    result = dht22_read_sensor_data(p_usPin, p_lDelay, p_lRetryCount, data, p_pfTemperature, p_fpHumidity);

    return result;
}

int dht22_read_sensor_data(unsigned short p_usPin, int p_lDelay, int p_lRetryCount, unsigned short* pData, double* p_pfTemperature, double* p_fpHumidity)
{
    int result = 0;

#ifdef __arm__
    for (int lRetryCount = 0; lRetryCount<p_lRetryCount + 1; lRetryCount++)
    {
        short checksum = 0;

        result = 0;

        pinMode(p_usPin, OUTPUT);

        // Send out start signal
        digitalWrite(p_usPin, LOW);
        delay(20);                  // Stay LOW for 5~30 milliseconds
        pinMode(p_usPin, INPUT);        // 'INPUT' equals 'HIGH' level. And signal read mode

        dht22_read_data(p_usPin, pData);      // Read DHT22 signal

                                // The sum is maybe over 8 bit like this: '0001 0101 1010'.
                                // Remove the '9 bit' data using AND operator.
        checksum = (pData[0] + pData[1] + pData[2] + pData[3]) & 0xFF;

        // If Check-sum data is correct (NOT 0x00), display humidity and temperature
        if (pData[4] == checksum && checksum != 0x00)
        {
            // * 256 is the same thing '<< 8' (shift).
            *p_fpHumidity = ((pData[0] * 256) + pData[1]) / 10.0;

            // found that with the original code at temperatures > 25.4 degrees celsius
            // the temperature would print 0.0 and increase further from there.
            // Eventually when the actual temperature drops below 25.4 again
            // it would print the temperature as expected.
            // Some research and comparisin with other C implementation suggest a
            // different calculation of celsius.
            //celsius = data[3] / 10.0; //original
            *p_pfTemperature = (((pData[2] & 0x7F) * 256) + pData[3]) / 10.0; //Juergen Wolf-Hofer

                                                                            // If 'data[2]' data like 1000 0000, It means minus temperature
            if (pData[2] == 0x80)
            {
                *p_pfTemperature *= -1;
            }

            result = 1;
            break;
        }
        else
        {
            result = 0;
        }

        // Initialize data array for next loop
        for (unsigned char i = 0; i < 5; i++)
        {
            pData[i] = 0;
        }

        delay(p_lDelay);
    }
#else

	*p_pfTemperature = (double)(60 - rand() % 60 + 1);
    *p_fpHumidity = (double)(rand() % 100 + 1);

    result = 1;
#endif

    return result;
}

short dht22_read_data(unsigned short p_usPin, unsigned short* pData)
{
    unsigned short val = 0x00;
    unsigned short signal_length = 0;
    unsigned short val_counter = 0;
    unsigned short loop_counter = 0;

#ifdef __arm__

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wconversion"

    while (1)
    {
        // Count only HIGH signal
        while (digitalRead(p_usPin) == HIGH)
        {
            signal_length++;

            // When sending data ends, high signal occur infinite.
            // So we have to end this infinite loop.
            if (signal_length >= 200)
            {
                return -1;
            }

            delayMicroseconds(1);
        }

        // If signal is HIGH
        if (signal_length > 0)
        {
            loop_counter++; // HIGH signal counting

            // The DHT22 sends a lot of unstable signals.
            // So extended the counting range.
            if (signal_length < 10)
            {
                // Unstable signal
                val <<= 1;      // 0 bit. Just shift left
            }

            else if (signal_length < 30)
            {
                // 26~28us means 0 bit
                val <<= 1;      // 0 bit. Just shift left
            }

            else if (signal_length < 85)
            {
                // 70us means 1 bit
                // Shift left and input 0x01 using OR operator
                val <<= 1;
                val |= 1;
            }

            else
            {
                // Unstable signal
                return -1;
            }

            signal_length = 0;  // Initialize signal length for next signal
            val_counter++;      // Count for 8 bit data
        }

        // The first and second signal is DHT22's start signal.
        // So ignore these data.
        if (loop_counter < 3)
        {
            val = 0x00;
            val_counter = 0;
        }

        // If 8 bit data input complete
        if (val_counter >= 8)
        {
            // 8 bit data input to the data array
			pData[(loop_counter / 8) - 1] = val;

            val = 0x00;
            val_counter = 0;
        }
    }

#pragma GCC diagnostic pop

#endif

	return 1;
}
