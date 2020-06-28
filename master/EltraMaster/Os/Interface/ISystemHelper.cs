using System;

namespace EltraMaster.Os.Interface
{
    public interface ISystemHelper
    {
        IntPtr GetDllInstance(string dllName);
        
        bool Is64BitProcess();
        bool IsWindows();

        IntPtr GetProcAddress(IntPtr dllHandle, string funcName);

        bool FreeLibrary(IntPtr dllHandle);
    }
}
