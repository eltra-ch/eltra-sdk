using System;
using System.Threading.Tasks;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.Logger;
using PhotoMaster.Settings;
using EltraMaster.Device;
using PhotoMaster.DeviceManager.Wrapper;
using System.IO;
using PhotoMaster.DeviceManager.Device.Commands;
using System.Diagnostics;

namespace PhotoMaster.DeviceManager.Device
{
    class PhotoDeviceCommunication : MasterDeviceCommunication
    {
        #region Private fields

        private static readonly object SyncObject = new object();

        private readonly MasterSettings _settings;
                
        private Parameter _statusWordParameter;
        private Parameter _internalRecorderBufferVariable1DataParameter;

        #endregion

        #region Enums

        private enum StatusWordValues
        {
            Undefined,
            Operational,
            Pending,
            Failure
        }

        #endregion

        #region Constructors

        public PhotoDeviceCommunication(PhotoDevice device, MasterSettings settings)
            : base(device, settings.UpdateInterval, settings.Timeout)
        {
            _settings = settings;
        }

        #endregion

        #region Events handler

        protected override void OnInitialized()
        {
            var updateTask = Task.Run(async () =>
            {
                if (Vcs != null)
                {
                    await Vcs.UpdateParameterValue("PARAM_StatusWord");
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - OnDeviceStatusChanged", "Vcs not defined!");
                }
            });

            updateTask.Wait();

            _statusWordParameter = Device?.SearchParameter("PARAM_StatusWord") as Parameter;

            if(_statusWordParameter != null)
            {
                _statusWordParameter.SetValue((ushort)StatusWordValues.Operational);
            }

            _internalRecorderBufferVariable1DataParameter = Device?.SearchParameter("PARAM_InternalRecorderBufferVariable1Data") as Parameter;            
        }

        #endregion

        #region Methods

        public override bool GetObject(ushort objectIndex, byte objectSubindex, ref byte[] data)
        {
            bool result = false;

            MsgLogger.WriteFlow($"Get value, parameter 0x{objectIndex:X4}, 0x{objectSubindex:X4}");

            switch (objectIndex)
            {
                case PhotoDeviceParameters.StatusWord_Index:
                    {
                        if (_statusWordParameter.GetValue(out ushort statusWord))
                        {
                            data = BitConverter.GetBytes(statusWord);
                            result = true;
                        }
                    }
                    break;
                case PhotoDeviceParameters.InternalRecorderBufferVariable1:
                    {
                        if (_internalRecorderBufferVariable1DataParameter != null)
                        {
                            switch (objectSubindex)
                            {
                                case PhotoDeviceParameters.InternalRecorderBufferVariable1MaxSubIdx:
                                    data = BitConverter.GetBytes(PhotoDeviceParameters.InternalRecorderBufferVariable1MaxSubIdxValue);
                                    result = true;
                                    break;

                                case PhotoDeviceParameters.InternalRecorderBufferVariable1Data:
                                    result = _internalRecorderBufferVariable1DataParameter.GetValue(out data);
                                    break;                                
                            }
                        }
                                
                    } break;
            }

            return result;
        }

        private byte[] TakePicture1()
        {
            byte[] bytes = null;
            string tempFileName = Path.GetTempFileName();
            int callResult;
            
            if ((callResult = EltraFsWebCamWrapper.TakePicture(tempFileName)) == 0)
            {
                bytes = File.ReadAllBytes(tempFileName);

                if (File.Exists(tempFileName))
                {
                    File.Delete(tempFileName);
                }
            }

            LastErrorCode = (uint)callResult;

            return bytes;
        }

        private byte[] TakePicture0()
        {
            byte[] bytes = null;
            int callResult;
            int bufferSize = 0;

            if ((callResult = EltraFsWebCamWrapper.TakePictureBufferSize(ref bufferSize)) == 0)
            {
                bytes = new byte[bufferSize];

                callResult = EltraFsWebCamWrapper.TakePictureBuffer(bytes, bufferSize);
            }

            LastErrorCode = (uint)callResult;

            return bytes;
        }

        public bool TakePicture(ushort index)
        {
            bool result = false;
            int lastErrorCode = 0;

            lock (SyncObject)
            {
                MsgLogger.WriteFlow($"take picture ...");

                try
                {
                    if (_statusWordParameter != null)
                    {
                        _statusWordParameter.SetValue((ushort)StatusWordValues.Pending);

                        byte[] bytes = TakePicture0();

                        if (bytes == null)
                        {
                            bytes = TakePicture1();
                        }

                        if (bytes != null && bytes.Length > 0)
                        {
                            if (_internalRecorderBufferVariable1DataParameter != null)
                            {
                                result = _internalRecorderBufferVariable1DataParameter.SetValue(bytes);
                            }

                            if (result)
                            {
                                _statusWordParameter.SetValue((ushort)StatusWordValues.Operational);
                            }
                            else
                            {
                                _statusWordParameter.SetValue((ushort)StatusWordValues.Failure);
                            }
                        }
                        else
                        {
                            _statusWordParameter.SetValue((ushort)StatusWordValues.Failure);
                        }
                    }
                }
                catch (Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - TakePicture", e);
                }
            }

            LastErrorCode = (uint)lastErrorCode;

            return result;
        }

        private bool RecordVideoAsync(int durationInSec)
        {
            bool result = false;
            var duration = new Stopwatch();
            double rate = 1000 / 24;
            long lastSnap = 0;
            int counter = 0;
            duration.Start();

            _statusWordParameter?.SetValue((ushort)StatusWordValues.Pending);

            do
            {
                if (duration.ElapsedMilliseconds - lastSnap > rate || lastSnap == 0)
                {
                    byte[] bytes = TakePicture0();

                    if (bytes == null)
                    {
                        bytes = TakePicture1();
                    }

                    if (bytes != null && bytes.Length > 0)
                    {
                        if (_internalRecorderBufferVariable1DataParameter != null)
                        {
                            result = _internalRecorderBufferVariable1DataParameter.SetValue(bytes);
                        }

                        lastSnap = duration.ElapsedMilliseconds;
                        counter++;
                    }
                    else
                    {
                        Task.Delay(250);
                    }
                }
                else
                {
                    Task.Delay((int)rate/2);
                }
            } while (duration.ElapsedMilliseconds <= durationInSec * 1000);

            _statusWordParameter?.SetValue(result ? (ushort)StatusWordValues.Operational : (ushort)StatusWordValues.Failure);

            return result;
        }

        public bool RecordVideo(int durationInSec)
        {
            bool result = false;
            int lastErrorCode = 0;

            lock (SyncObject)
            {
                MsgLogger.WriteFlow($"record video ...");

                try
                {
                    Task.Run(()=>
                    {
                        RecordVideoAsync(durationInSec);
                    });
                }
                catch (Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - TakePicture", e);
                }
            }

            LastErrorCode = (uint)lastErrorCode;

            return result;
        }

        #endregion
    }
}
