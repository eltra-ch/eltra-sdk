using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using EltraCommon.Logger;
using EltraMaster.Os.Interface;

namespace EltraMaster.Os.Windows
{
    public class SystemHelper : ISystemHelper
    {
        #region Methods

        public IntPtr GetDllInstance()
        {
            string eposCmdFileName = "EltraCmd.dll";

            if (Is64BitProcess())
            {
                eposCmdFileName = "EltraCmd64.dll";
            }

            Assembly asm = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            string directory = Path.GetDirectoryName(asm.Location);

            // Working Directory
            FileInfo workingDirectory = new FileInfo(eposCmdFileName);
            if (!File.Exists(workingDirectory.FullName))
            {
                // Application Directory
                FileInfo applicationDirectory = new FileInfo(directory + "\\" + eposCmdFileName);
                if (!File.Exists(applicationDirectory.FullName))
                {
                    throw new Exception(string.Format("File {0} not found!\n\nWorking Directory: {1}\nApplication Directory: {2}", eposCmdFileName, workingDirectory.FullName, applicationDirectory.FullName));
                }

                eposCmdFileName = directory + "\\" + eposCmdFileName;
            }

            var _eposCmdDll = KernelDll.LoadLibrary(eposCmdFileName);

            if (_eposCmdDll == IntPtr.Zero)
            {
                throw new Exception(string.Format("{0} could not be loaded!", eposCmdFileName));
            }

            return _eposCmdDll;
        }

        public IntPtr GetProcAddress(IntPtr dllHandle, string funcName)
        {
            // Name "_VCM_xxxx@12"
            var result = KernelDll.GetProcAddress(dllHandle, funcName);

            if (result == IntPtr.Zero && funcName.Contains("@"))
            {
                funcName = funcName.Substring(1, funcName.IndexOf('@') - 1);

                // Name "VCM_xxxx"
                result = KernelDll.GetProcAddress(dllHandle, funcName);
            }

            return result;
        }

        private static bool Is64BitProcess()
        {
            if (IntPtr.Size == 8)
            {
                // 64-bit (IntPtr.Size = 8)
                return true;
            }

            return false;
        }

        public bool FreeLibrary(IntPtr dllHandle)
        {
            bool result;
            
            do
            {
                try
                {
                    result = KernelDll.FreeLibrary(dllHandle);
                }
                catch(SEHException se)
                {
                    MsgLogger.Exception($"{GetType().Name} - FreeLibrary", se.InnerException);
                    result = false;
                }
                catch(Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - FreeLibrary", e);
                    result = false;
                }                
            }
            while(result);

            return result;
        }

        #endregion
    }
}