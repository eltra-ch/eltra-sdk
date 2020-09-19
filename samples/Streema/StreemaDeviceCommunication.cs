using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraMaster.DeviceManager.Events;
using System;
using EltraConnector.Master.Device;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters.Events;
using System.Collections.Generic;

namespace StreemaMaster
{
    public class StreemaDeviceCommunication : MasterDeviceCommunication
    {
        #region Private fields

        private readonly List<Parameter> _urlParameters;

        private Parameter _activeStationParameter;
        private Parameter _volumeParameter;
        private Parameter _statusWordParameter;
        private Parameter _controlWordParameter;
        
        #endregion

        #region Constructors

        public StreemaDeviceCommunication(MasterDevice device)
            : base(device)
        {
            _urlParameters = new List<Parameter>();
        }

        #endregion

        #region Init

        protected override async void OnInitialized()
        {
            Console.WriteLine($"device (node id={Device.NodeId}) initialized, processing ...");

            _controlWordParameter = Vcs.SearchParameter(0x6040, 0x00) as Parameter;
            _statusWordParameter = Vcs.SearchParameter(0x6041, 0x00) as Parameter;
            
            _activeStationParameter = Vcs.SearchParameter(0x4001, 0x00) as Parameter;
            _volumeParameter = Vcs.SearchParameter(0x4002, 0x00) as Parameter;

            var maxUrlsCountParameter = Vcs.SearchParameter(0x4000, 0x00) as Parameter;

            if(maxUrlsCountParameter != null && maxUrlsCountParameter.GetValue(out byte maxCount))
            {
                for(byte i = 0; i < maxCount; i++)
                {
                    var urlParameter = Vcs.SearchParameter(0x4000, (byte)(i + 1)) as Parameter;

                    if (urlParameter != null)
                    {
                        _urlParameters.Add(urlParameter);
                    }
                }                
            }

            if(_activeStationParameter != null)
            {
                await _activeStationParameter.UpdateValue();

                _activeStationParameter.ParameterChanged += OnActiveStationParameterChanged;
            }

            base.OnInitialized();
        }

        #endregion

        #region Events

        private void OnActiveStationParameterChanged(object sender, ParameterChangedEventArgs e)
        {
            var parameterValue = e.NewValue;
            int activeStationValue = 0;

            if(parameterValue.GetValue(ref activeStationValue))
            {
                Console.WriteLine($"Active Station Changed = {activeStationValue}");
            }
        }

        #endregion

        #region SDO

        public override bool GetObject(ushort objectIndex, byte objectSubindex, ref byte[] data)
        {
            bool result = false;

            //PARAM_ControlWord
            if (objectIndex == 0x6040 && objectSubindex == 0x0)
            {
                if(_controlWordParameter.GetValue(out byte[] v))
                {
                    data = v;
                    result = true;
                }
            }
            else if (objectIndex == 0x6041)
            {
                if (_statusWordParameter.GetValue(out byte[] v))
                {
                    data = v;
                    result = true;
                }
            } 
            else if (objectIndex == 0x4001)
            {
                if (_activeStationParameter.GetValue(out byte[] v))
                {
                    data = v;
                    result = true;
                }
            }
            else if (objectIndex == 0x4002)
            {
                if (_volumeParameter.GetValue(out byte[] v))
                {
                    data = v;
                    result = true;
                }
            }
            else if (objectIndex == 0x4000)
            {
                if (_urlParameters.Count > objectSubindex)
                {
                    if (_urlParameters[objectSubindex - 1].GetValue(out byte[] d1))
                    {
                        data = d1;
                        result = true;
                    }
                }
            }

            return result;
        }

        public override bool SetObject(ushort objectIndex, byte objectSubindex, byte[] data)
        {
            bool result = false;

            //PARAM_ControlWord
            if (objectIndex == 0x6040 && objectSubindex == 0x0)
            {
                var controlWordValue = BitConverter.ToUInt16(data, 0);

                Console.WriteLine($"new controlword value = {controlWordValue}");

                result = _controlWordParameter.SetValue(controlWordValue);
            }
            else if (objectIndex == 0x4000)
            {
                if (_urlParameters.Count > objectSubindex)
                {
                    result = _urlParameters[objectSubindex - 1].SetValue(data);
                }
            }
            else if (objectIndex == 0x4001 && objectSubindex == 0x0)
            {
                var activeStationValue = BitConverter.ToInt32(data, 0);

                Console.WriteLine($"new active station value = {activeStationValue}");

                result = _activeStationParameter.SetValue(activeStationValue);
            }
            else if (objectIndex == 0x4002)
            {
                var volumeValue = BitConverter.ToInt32(data, 0);

                Console.WriteLine($"new volume value = {volumeValue}");

                result = _volumeParameter.SetValue(volumeValue);
            }

            return result;
        }

        #endregion

        #region Events

        protected override void OnStatusChanged(DeviceCommunicationEventArgs e)
        {
            Console.WriteLine($"status changed, status = {e.Device.Status}, error code = {e.LastErrorCode}");

            base.OnStatusChanged(e);
        }

        #endregion
    }
}
