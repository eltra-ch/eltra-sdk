#include <stdio.h>
#ifdef __arm__
    #include <wiringPi.h>
    #include <unistd.h>
#endif
#include <mutex>
#include <stdlib.h>
#include "relay.h"

extern "C" int relay_initialize()
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

extern "C" int relay_release()
{
    return 1;
}

extern "C" int relay_read(unsigned short p_usPin, int* p_plValue)
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

extern "C" int relay_write(unsigned short p_usPin, int p_lValue)
{
    int lResult = 1;

#ifdef __arm__
    digitalWrite((int)p_usPin, p_lValue);
#endif

    return lResult;
}

extern "C" int relay_pin_mode(unsigned short p_usPin, int p_lValue)
{
    int lResult = 1;

#ifdef __arm__
    pinMode((int)p_usPin, p_lValue);
#endif

    return lResult;
}
