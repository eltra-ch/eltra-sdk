using EltraCommon.Logger;
using EltraMaster.Dll.Wrapper;
using System;
using System.Runtime.InteropServices;

namespace ThermoMaster.DeviceManager.Wrapper
{
    public class EltraDht22Wrapper : EltraDllWrapper
    {
        #region Private fields

        private static IntPtr _dhtRead;
        private static IntPtr _dhtInit;
        private static IntPtr _dhtRelease;
        private static IntPtr _dhtDll;

        #endregion

        #region Methods

        #region Wrapper

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalDhtRead(ushort pinNumber, int delay, int retryCount, ref double temperature, ref double humidity);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalDhtInitialize();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalDhtRelease();

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

                if (result == 1)
                {
                    if (_dhtInit == IntPtr.Zero)
                    {
                        throw new Exception("Function InitDht22 not supported");
                    }

                    var func = (InternalDhtInitialize)Marshal.GetDelegateForFunctionPointer(_dhtInit, typeof(InternalDhtInitialize));

                    result = func();
                }
            }
            catch(Exception e)
            {
                MsgLogger.Exception("EltraThermoSensors- Initialize", e);
            }
            
            return result;
        }

        private static int ReleaseDht22()
        {
            int result = 0;

            if (_dhtRelease == IntPtr.Zero)
            {
                throw new Exception("Function ReleaseDht22 not supported");
            }

            var func = (InternalDhtRelease)Marshal.GetDelegateForFunctionPointer(_dhtRelease, typeof(InternalDhtRelease));

            result = func();

            return result;
        }

        public static int DhtRead(ushort pinNumber, int delay, int retryCount, ref double temperature, ref double humidity)
        {
            int result = 0;

            if (!IsLoaded())
            {
                Load();
            }

            if (_dhtRead == IntPtr.Zero)
            {
                throw new Exception("Function ReadDht22 not supported");
            }

            var func = (InternalDhtRead)Marshal.GetDelegateForFunctionPointer(_dhtRead, typeof(InternalDhtRead));

            result = func(pinNumber, delay, retryCount, ref temperature, ref humidity);

            return result;
        }

        private static bool IsLoaded()
        {
            bool result;

            result = _dhtDll != IntPtr.Zero &&
                    _dhtInit != IntPtr.Zero &&
                    _dhtRelease != IntPtr.Zero &&
                    _dhtRead != IntPtr.Zero;

            return result;
        }

        private static int Load()
        {
			int result = 1;
            string libName = "EltraDht22";

            try
            {
                if (!IsLoaded())
                {
                    _dhtDll = GetDllInstance(libName);

                    _dhtInit = GetProcAddress(_dhtDll, "dht22_initialize");
                    _dhtRelease = GetProcAddress(_dhtDll, "dht22_release");
                    _dhtRead = GetProcAddress(_dhtDll, "dht22_read");
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
                ReleaseDht22();

                FreeLibrary(_dhtDll);

                result = true;
			}
			
			return result;
		}
				
        #endregion
    }
}