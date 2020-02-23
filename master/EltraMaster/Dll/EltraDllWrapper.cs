using System;
using System.Runtime.InteropServices;
using EltraMaster.Os.Linux;

namespace EltraMaster.Dll.Wrapper
{
#pragma warning disable CA1401 // P/Invokes should not be visible
#pragma warning disable CA2101 // Specify marshaling for P/Invoke string arguments
#pragma warning disable IDE1006 // Naming Styles

    public class EltraDllWrapper
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

                dll = EltraMaster.Os.Windows.KernelDll.LoadLibrary(fileName);

                if (dll == IntPtr.Zero)
                {
                    throw new Exception($"{fileName} not found!");
                }
            }

            return dll;
        }

        protected static IntPtr GetProcAddress(IntPtr dllHandle, string funcName)
        {
            IntPtr result;

            if (SystemHelper.IsLinux)
            {
                dlerror();

                result = dlsym(dllHandle, funcName);
            }
            else
            {
                result = Os.Windows.KernelDll.GetProcAddress(dllHandle, funcName);
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
                result = EltraMaster.Os.Windows.KernelDll.FreeLibrary(dllHandle);
            }

            return result;
        }

        #endregion
    }

#pragma warning restore CA1401 // P/Invokes should not be visible
#pragma warning restore CA2101 // Specify marshaling for P/Invoke string arguments
#pragma warning restore IDE1006 // Naming Styles
}
