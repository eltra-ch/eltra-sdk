using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using ThermoMaster.Os.Interface;

namespace ThermoMaster.Os.Linux
{
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
    public class SystemHelper : ISystemHelper
    {
        #region Constants

        private const int RtldNow = 2;
        private const int RtldGlobal = 8;

        #endregion

        #region Properties

        public static bool IsLinux
        {
            get
            {
                return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            }
        }

        #endregion

        #region Methods

        [DllImport("libdl.so")]
        private static extern IntPtr dlopen(string fileName, int flags);

        [DllImport("libdl.so")]
        private static extern IntPtr dlsym(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)] string symbol);

        [DllImport("libdl.so")]
        private static extern int dlclose(IntPtr handle);

        [DllImport("libdl.so")]
        private static extern IntPtr dlerror();

        public IntPtr GetDllInstance()
        {
            string fileName = "libEposCmd.so";

            IntPtr dll = dlopen(fileName, RtldNow | RtldGlobal);
            
            if (dll == IntPtr.Zero)
            {
                var errPtr = dlerror();
                throw new Exception("libEposCmd.so not found: " + Marshal.PtrToStringAnsi(errPtr));
            }

            return dll;
        }

        private static string GetLinuxFuncName(string funcName)
        {
            string result = string.Empty;
            const int PrefixOffset = 1;

            int postfix = funcName.IndexOf("@", StringComparison.Ordinal);

            if (postfix > PrefixOffset)
            {
                result = funcName.Substring(0, postfix);
                result = result.Substring(PrefixOffset);
            }
            
            return result;
        }

        public IntPtr GetProcAddress(IntPtr dllHandle, string funcName)
        {
            dlerror();
            
            string linuxFuncName = GetLinuxFuncName(funcName);

            var res = dlsym(dllHandle, linuxFuncName);           

            return res;
        }

        public bool FreeLibrary(IntPtr dllHandle)
        {
            var result = dlclose(dllHandle) == 0;

            return result;
        }

        #endregion
    }
}
