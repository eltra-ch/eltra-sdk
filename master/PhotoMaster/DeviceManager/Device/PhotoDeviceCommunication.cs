using System;
using System.Threading.Tasks;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.Logger;
using PhotoMaster.Settings;
using EltraMaster.Device;
using PhotoMaster.DeviceManager.Wrapper;
using System.IO;

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

        public bool TakePicture(ushort index)
        {
            bool result = false;
            int lastErrorCode = 0;

            lock (SyncObject)
            {
                MsgLogger.WriteFlow($"take picture - index: {index} ...");

                try
                {
                    int callResult = 0;

                    if (_statusWordParameter != null)
                    {
                        _statusWordParameter.SetValue((ushort)StatusWordValues.Pending);

                        string tempFileName = Path.GetTempFileName();

                        if ((callResult = EltraFsWebCamWrapper.TakePicture(index, tempFileName)) == 0)
                        {
                            byte[] bytes = File.ReadAllBytes(tempFileName);

                            if (bytes.Length > 0)
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
                        else
                        {
                            _statusWordParameter.SetValue((ushort)StatusWordValues.Failure);

                            LastErrorCode = (uint)callResult;
                        }

                        if (File.Exists(tempFileName))
                        {
                            File.Delete(tempFileName);
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

        #endregion
    }
}
