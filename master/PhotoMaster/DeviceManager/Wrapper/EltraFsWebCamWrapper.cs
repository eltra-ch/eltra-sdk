using EltraCommon.Logger;
using EltraMaster.Dll;
using System;
using System.Runtime.InteropServices;

namespace PhotoMaster.DeviceManager.Wrapper
{
    public class EltraFsWebCamWrapper : EltraDllWrapper
    {
        #region Private fields

        private static IntPtr _init;
        private static IntPtr _release;
        private static IntPtr _takePicture;
        private static IntPtr _dll;

        #endregion

        #region Methods

        #region Wrapper

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalFsWebCamInitialize();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalFsWebCamRelease();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalFsWebCamTakePicture(ushort index, string fileName);
        
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
                    if (_init == IntPtr.Zero)
                    {
                        throw new Exception("Function Init not supported");
                    }

                    var func = (InternalFsWebCamInitialize)Marshal.GetDelegateForFunctionPointer(_init, typeof(InternalFsWebCamInitialize));

                    result = func();
                }
            }
            catch(Exception e)
            {
                MsgLogger.Exception("EltraFsWebCamWrapper - Initialize", e);
            }
            
            return result;
        }

        private static int ReleaseFsWebCam()
        {
            int result = 0;

            if (_release == IntPtr.Zero)
            {
                throw new Exception("Function Release not supported");
            }

            var func = (InternalFsWebCamRelease)Marshal.GetDelegateForFunctionPointer(_release, typeof(InternalFsWebCamRelease));

            result = func();

            return result;
        }

        public static int TakePicture(ushort index, string fileName)
        {
            int result = 0;

            if (!IsLoaded())
            {
                Load();
            }

            if (_takePicture == IntPtr.Zero)
            {
                throw new Exception("Function TakePicture not supported");
            }

            var func = (InternalFsWebCamTakePicture)Marshal.GetDelegateForFunctionPointer(_takePicture, typeof(InternalFsWebCamTakePicture));

            result = func(index, fileName);

            return result;
        }

        private static bool IsLoaded()
        {
            bool result;

            result = _dll != IntPtr.Zero &&
                    _init != IntPtr.Zero &&
                    _release != IntPtr.Zero &&
                    _takePicture != IntPtr.Zero;

            return result;
        }

        private static int Load()
        {
			int result = 1;
            string libName = "EltraFsWebCam";

            try
            {
                if (!IsLoaded())
                {
                    _dll = GetDllInstance(libName);

                    _init = GetProcAddress(_dll, "fswebcam_initialize");
                    _release = GetProcAddress(_dll, "fswebcam_release");
                    _takePicture = GetProcAddress(_dll, "fswebcam_take_picture");
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
                ReleaseFsWebCam();

                FreeLibrary(_dll);

                result = true;
			}
			
			return result;
		}
				
        #endregion
    }
}