using System;
using System.Runtime.InteropServices;
using ThermoMaster.Os.Linux;

namespace ThermoMaster.DeviceManager.Wrapper
{
    public class EltraWrapper
    {
        #region Constants

        protected const int RtldNow = 2;
        protected const int RtldGlobal = 8;

        #endregion

        #region System

        [DllImport("libdl.so")]
        protected static extern IntPtr dlopen(string fileName, int flags);

        [DllImport("libdl.so")]
        protected static extern IntPtr dlsym(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)] string symbol);

        [DllImport("libdl.so")]
        protected static extern int dlclose(IntPtr handle);

        [DllImport("libdl.so")]
        protected static extern IntPtr dlerror();

        protected static IntPtr GetDllInstance(string libName)
        {
            IntPtr dll;
            string fileName;

            if (SystemHelper.IsLinux)
            {
                fileName = $"lib{libName}.so";

                dll = dlopen(fileName, RtldNow | RtldGlobal);

                if (dll == IntPtr.Zero)
                {
                    var errPtr = dlerror();

                    throw new Exception($"{fileName} not found: " + Marshal.PtrToStringAnsi(errPtr));
                }
            }
            else
            {
                fileName = $"{libName}.dll";

                dll = ThermoMaster.Os.Windows.KernelDll.LoadLibrary(fileName);

                if (dll == IntPtr.Zero)
                {
                    throw new Exception($"{fileName} not found!");
                }
            }

            return dll;
        }

        protected static IntPtr GetProcAddress(IntPtr dllHandle, string funcName)
        {
            IntPtr result = IntPtr.Zero;

            if (SystemHelper.IsLinux)
            {
                dlerror();

                result = dlsym(dllHandle, funcName);
            }
            else
            {
                result = ThermoMaster.Os.Windows.KernelDll.GetProcAddress(dllHandle, funcName);
            }

            return result;
        }

        protected static bool FreeLibrary(IntPtr dllHandle)
        {
            bool result;

            if (SystemHelper.IsLinux)
            {
                result = dlclose(dllHandle) == 0;
            }
            else
            {
                result = ThermoMaster.Os.Windows.KernelDll.FreeLibrary(dllHandle);
            }

            return result;
        }

        #endregion
    }
}
