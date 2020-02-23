using System;
using System.Runtime.InteropServices;

namespace EltraMaster.Os.Windows
{
#pragma warning disable CA2101 // Specify marshaling for P/Invoke string arguments
    public static class KernelDll
    {
        [DllImport("kernel32.dll")]

        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr module, string procedureName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr module);
    }

#pragma warning restore CA2101 // Specify marshaling for P/Invoke string arguments
}
