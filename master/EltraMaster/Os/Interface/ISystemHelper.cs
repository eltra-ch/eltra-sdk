using System;

namespace EltraMaster.Os.Interface
{
    public interface ISystemHelper
    {
        IntPtr GetDllInstance();

        IntPtr GetProcAddress(IntPtr dllHandle, string funcName);

        bool FreeLibrary(IntPtr dllHandle);
    }
}
