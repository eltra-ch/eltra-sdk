using EltraCommon.Logger;
using EltraMaster.Dll.Wrapper;
using System;
using System.Runtime.InteropServices;

namespace ThermoMaster.DeviceManager.Wrapper
{
    public class EltraRelayWrapper : EltraDllWrapper
    {
        #region Private fields

        private static IntPtr _relayInit;
        private static IntPtr _relayRelease;
        private static IntPtr _relayRead;
        private static IntPtr _relayWrite;
        private static IntPtr _relayPinMode;
        private static IntPtr _relayDll;

        #endregion

        #region Enums

        public enum GPIOpinmode
        {
            Input = 0,
            Output = 1,
            PWMOutput = 2,
            GPIOClock = 3,
            SoftPWMOutput = 4,
            SoftToneOutput = 5,
            PWMToneOutput = 6
        }

        #endregion

        #region Methods

        #region Wrapper

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalRelayInitialize();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalRelayRelease();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalRelayRead(ushort pinNumber, ref int value);
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalRelayWrite(ushort pinNumber, int value);
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalRelayPinMode(ushort pinNumber, int value);

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
                    if (_relayInit == IntPtr.Zero)
                    {
                        throw new Exception("Function InitRelay not supported");
                    }

                    var func = (InternalRelayInitialize)Marshal.GetDelegateForFunctionPointer(_relayInit, typeof(InternalRelayInitialize));

                    result = func();
                }
            }
            catch(Exception e)
            {
                MsgLogger.Exception("EltraThermoSensors- Initialize", e);
            }
            
            return result;
        }

        private static int ReleaseRelay()
        {
            int result = 0;

            if (_relayRelease == IntPtr.Zero)
            {
                throw new Exception("Function ReleaseRelay not supported");
            }

            var func = (InternalRelayRelease)Marshal.GetDelegateForFunctionPointer(_relayRelease, typeof(InternalRelayRelease));

            result = func();

            return result;
        }

        public static int RelayRead(ushort pinNumber, ref int value)
        {
            int result = 0;

            if (!IsLoaded())
            {
                Load();
            }

            if (_relayRead == IntPtr.Zero)
            {
                throw new Exception("Function DigitalRead not supported");
            }

            var func = (InternalRelayRead)Marshal.GetDelegateForFunctionPointer(_relayRead, typeof(InternalRelayRead));

            result = func(pinNumber, ref value);

            return result;
        }

        public static int RelayWrite(ushort pinNumber, int value)
        {
            int result = 0;

            if (!IsLoaded())
            {
                Load();
            }

            if (_relayWrite == IntPtr.Zero)
            {
                throw new Exception("Function DigitalWrite not supported");
            }

            var func = (InternalRelayWrite)Marshal.GetDelegateForFunctionPointer(_relayWrite, typeof(InternalRelayWrite));

            result = func(pinNumber, value);

            return result;
        }

        public static int RelayPinMode(ushort pinNumber, GPIOpinmode mode)
        {
            int result = 0;

            if (!IsLoaded())
            {
                Load();
            }

            if (_relayPinMode == IntPtr.Zero)
            {
                throw new Exception("Function PinMode not supported");
            }

            var func = (InternalRelayPinMode)Marshal.GetDelegateForFunctionPointer(_relayPinMode, typeof(InternalRelayPinMode));

            result = func(pinNumber, (int)mode);

            return result;
        }

        private static bool IsLoaded()
        {
            bool result;

            result = _relayDll != IntPtr.Zero &&
                    _relayInit != IntPtr.Zero &&
                    _relayRelease != IntPtr.Zero &&
                    _relayRead != IntPtr.Zero &&
                    _relayWrite != IntPtr.Zero &&
                    _relayPinMode != IntPtr.Zero;

            return result;
        }

        private static int Load()
        {
			int result = 1;
            string libName = "EltraRelay";

            try
            {
                if (!IsLoaded())
                {
                    _relayDll = GetDllInstance(libName);

                    _relayInit = GetProcAddress(_relayDll, "relay_initialize");
                    _relayRelease = GetProcAddress(_relayDll, "relay_release");
                    _relayRead = GetProcAddress(_relayDll, "relay_read");
                    _relayWrite = GetProcAddress(_relayDll, "relay_write");
                    _relayPinMode = GetProcAddress(_relayDll, "relay_pin_mode");
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
                ReleaseRelay();

                FreeLibrary(_relayDll);

                result = true;
			}
			
			return result;
		}
				
        #endregion
    }
}