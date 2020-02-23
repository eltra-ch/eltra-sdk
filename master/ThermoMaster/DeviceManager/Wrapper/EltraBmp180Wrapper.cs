using EltraCommon.Logger;
using EltraMaster.Dll.Wrapper;
using System;
using System.Runtime.InteropServices;

namespace ThermoMaster.DeviceManager.Wrapper
{
    public class EltraBmp180Wrapper : EltraDllWrapper
    {
        #region Private fields

        private static IntPtr _bmpRead;
        private static IntPtr _bmp180Dll;

        #endregion

        #region Methods

        #region Wrapper

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalBmpRead(ref double temperature, ref double pressure, ref double altitude);

        #endregion

        public static int Initialize()
        {
            int result = 1;

            try
            {
                if (!IsLoaded())
                {
                    result = Load();
                }                
            }
            catch(Exception e)
            {
                MsgLogger.Exception("Initialize", e);
            }
            
            return result;
        }
        
        public static int BmpRead(ref double temperature, ref double pressure, ref double altitude)
        {
            int result = 0;

            if (!IsLoaded())
            {
                Load();
            }

            if (_bmpRead == IntPtr.Zero)
            {
                throw new Exception("Function ReadBmp180 not supported");
            }

            var func = (InternalBmpRead)Marshal.GetDelegateForFunctionPointer(_bmpRead, typeof(InternalBmpRead));

            result = func(ref temperature, ref pressure, ref altitude);
            
            return result;
        }

        private static bool IsLoaded()
        {
            bool result;

            result = _bmp180Dll != IntPtr.Zero &&
                    _bmpRead != IntPtr.Zero;

            return result;
        }

        private static int Load()
        {
			int result = 1;
            string libName = "EltraBmp180";

            try
            {
                if (!IsLoaded())
                {
                    _bmp180Dll = GetDllInstance(libName);
    
                    _bmpRead = GetProcAddress(_bmp180Dll, "bmp_read");
                }
            }
            catch(Exception e)
            {
                MsgLogger.Exception("Load", e);
            }
			
			return result;
		}
		
		public static bool Release()
		{
			bool result = false;
			
			if(IsLoaded())
			{
                FreeLibrary(_bmp180Dll);

                result = true;
			}
			
			return result;
		}
				
        #endregion
    }
}