using System;

namespace ThermoMaster.Os.Interface
{
    internal interface ISystemHelper
    {
        IntPtr GetDllInstance();

        IntPtr GetProcAddress(IntPtr dllHandle, string funcName);

        bool FreeLibrary(IntPtr dllHandle);
    }
}
