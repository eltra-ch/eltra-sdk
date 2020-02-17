using EltraCommon.Logger;
using System;
using System.Runtime.InteropServices;

namespace ThermoMaster.DeviceManager.Wrapper
{
    public class EltraSds011Wrapper : EltraWrapper
    {
        #region Enums

        public enum ESdsReportingMode
        {
            Undefined = 0xff,
            Query = 0,
            Stream
        };

        public enum ESdsWorkMode
        {
            Undefined = 0xff,
            Working = 0,
            Sleep
        };

        #endregion

        #region Private fields

        private static IntPtr _sds_pm_read;
        private static IntPtr _sds_get_working_period;
        private static IntPtr _sds_set_working_period;
        private static IntPtr _sds_get_reporting_mode;
        private static IntPtr _sds_set_reporting_mode;
        private static IntPtr _sds_get_work_mode;
        private static IntPtr _sds_set_work_mode;
        private static IntPtr _sdsDll;

        #endregion

        #region Enums

        #endregion

        #region Methods

        #region Wrapper

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSdsPmRead(string port, ref float pm25, ref float pm10);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSdsGetWorkingPeriod(string port, ref int workingPeriod);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSdsSetWorkingPeriod(string port, int workingPeriod);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSdsGetReportingMode(string port, ref int reportingMode);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSdsSetReportingMode(string port, int reportingMode);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSdsGetWorkMode(string port, ref int workMode);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSdsSetWorkMode(string port, int workMode);

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
                MsgLogger.Exception("EltraThermoSensors- Initialize", e);
            }
            
            return result;
        }

        public static int SdsPmRead(string port, ref float pm25, ref float pm10)
        {
            int result = 0;

            if (!IsLoaded())
            {
                Load();
            }

            if (_sds_pm_read == IntPtr.Zero)
            {
                throw new Exception("Function SdsPmRead not supported");
            }

            var func = (InternalSdsPmRead)Marshal.GetDelegateForFunctionPointer(_sds_pm_read, typeof(InternalSdsPmRead));

            result = func(port, ref pm25, ref pm10);

            return result;
        }

        public static int SdsGetWorkingPeriod(string port, ref int workingPeriod)
        {
            int result = 0;

            if (!IsLoaded())
            {
                Load();
            }

            if (_sds_get_working_period == IntPtr.Zero)
            {
                throw new Exception("Function SdsGetWorkingPeriod not supported");
            }

            var func = (InternalSdsGetWorkingPeriod)Marshal.GetDelegateForFunctionPointer(_sds_get_working_period, typeof(InternalSdsGetWorkingPeriod));

            result = func(port, ref workingPeriod);

            return result;
        }

        public static int SdsSetWorkingPeriod(string port, int workingPeriod)
        {
            int result = 0;

            if (!IsLoaded())
            {
                Load();
            }

            if (_sds_set_working_period == IntPtr.Zero)
            {
                throw new Exception("Function SdsSetWorkingPeriod not supported");
            }

            var func = (InternalSdsSetWorkingPeriod)Marshal.GetDelegateForFunctionPointer(_sds_set_working_period, typeof(InternalSdsSetWorkingPeriod));

            result = func(port, workingPeriod);

            return result;
        }

        public static int SdsGetReportingMode(string port, ref ESdsReportingMode reportingMode)
        {
            int result = 0;

            if (!IsLoaded())
            {
                Load();
            }

            if (_sds_get_reporting_mode == IntPtr.Zero)
            {
                throw new Exception("Function SdsGetReportingMode not supported");
            }

            var func = (InternalSdsGetReportingMode)Marshal.GetDelegateForFunctionPointer(_sds_get_reporting_mode, typeof(InternalSdsGetReportingMode));
            int mode = (int)ESdsReportingMode.Undefined;
            
            result = func(port, ref mode);

            reportingMode = (ESdsReportingMode)mode;

            return result;
        }

        public static int SdsSetReportingMode(string port, ESdsReportingMode reportingMode)
        {
            int result = 0;

            if (!IsLoaded())
            {
                Load();
            }

            if (_sds_set_reporting_mode == IntPtr.Zero)
            {
                throw new Exception("Function SdsSetReportingMode not supported");
            }

            var func = (InternalSdsSetReportingMode)Marshal.GetDelegateForFunctionPointer(_sds_set_reporting_mode, typeof(InternalSdsSetReportingMode));

            result = func(port, (int)reportingMode);

            return result;
        }

        public static int SdsGetWorkMode(string port, ref ESdsWorkMode workMode)
        {
            int result = 0;

            if (!IsLoaded())
            {
                Load();
            }

            if (_sds_get_work_mode == IntPtr.Zero)
            {
                throw new Exception("Function SdsGetWorkMode not supported");
            }

            var func = (InternalSdsGetWorkMode)Marshal.GetDelegateForFunctionPointer(_sds_get_work_mode, typeof(InternalSdsGetWorkMode));
            int mode = (int)ESdsWorkMode.Undefined;

            result = func(port, ref mode);

            workMode = (ESdsWorkMode)mode;

            return result;
        }

        public static int SdsSetWorkMode(string port, ESdsWorkMode workMode)
        {
            int result = 0;

            if (!IsLoaded())
            {
                Load();
            }

            if (_sds_set_work_mode == IntPtr.Zero)
            {
                throw new Exception("Function SdsSetWorkMode not supported");
            }

            var func = (InternalSdsSetWorkMode)Marshal.GetDelegateForFunctionPointer(_sds_set_work_mode, typeof(InternalSdsSetWorkMode));

            result = func(port, (int)workMode);

            return result;
        }

        private static bool IsLoaded()
        {
            bool result;

            result = _sdsDll != IntPtr.Zero &&
                    _sds_pm_read != IntPtr.Zero &&
                    _sds_get_working_period != IntPtr.Zero &&
                    _sds_set_working_period != IntPtr.Zero &&
                    _sds_get_reporting_mode != IntPtr.Zero &&
                    _sds_set_reporting_mode != IntPtr.Zero &&
                    _sds_get_work_mode != IntPtr.Zero &&
                    _sds_set_work_mode != IntPtr.Zero;

            return result;
        }

        private static int Load()
        {
			int result = 1;
            string libName = "EltraSds011";

            try
            {
                if (!IsLoaded())
                {
                    _sdsDll = GetDllInstance(libName);

                    _sds_pm_read = GetProcAddress(_sdsDll, "sds_pm_read");
                    _sds_get_working_period = GetProcAddress(_sdsDll, "sds_get_working_period");
                    _sds_set_working_period = GetProcAddress(_sdsDll, "sds_set_working_period");
                    _sds_get_reporting_mode = GetProcAddress(_sdsDll, "sds_get_reporting_mode");
                    _sds_set_reporting_mode = GetProcAddress(_sdsDll, "sds_set_reporting_mode");
                    _sds_get_work_mode = GetProcAddress(_sdsDll, "sds_get_work_mode");
                    _sds_set_work_mode = GetProcAddress(_sdsDll, "sds_set_work_mode");
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
                FreeLibrary(_sdsDll);

                result = true;
			}
			
			return result;
		}
				
        #endregion
    }
}