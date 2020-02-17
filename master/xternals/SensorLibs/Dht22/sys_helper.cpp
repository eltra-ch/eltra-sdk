#include <iostream>
#include <fstream>
#include "sys_helper.h"

using namespace std;

bool IsModuleLoaded(string p_moduleName)
{
    bool oResult = false;
    string line;
    ifstream modulesFile("/proc/modules");
    string delimiter = " ";
    string token;

    if (modulesFile.is_open())
    {
        while (getline(modulesFile, line))
        {
            token = line.substr(0, line.find(delimiter));

            if (token == p_moduleName)
            {
                oResult = true;
                break;
            }
        }

        modulesFile.close();
    }

    return oResult;
}
