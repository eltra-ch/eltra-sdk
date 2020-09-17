using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraMaster.DeviceManager.Events;
using System;
using EltraConnector.Master.Device;

namespace StreemaMaster
{
    public class StreemaDeviceCommunication : MasterDeviceCommunication
    {
        #region Private fields

        private Parameter _activeStationParameter;
        private Parameter _statusWordParameter;
        private Parameter _controlWordParameter;
        private Parameter _url1Parameter;
        private Parameter _url2Parameter;
        private Parameter _url3Parameter;
        private Parameter _url4Parameter;
        private Parameter _url5Parameter;

        #endregion

        #region Constructors

        public StreemaDeviceCommunication(MasterDevice device)
            : base(device)
        {
        }

        #endregion

        #region Init

        protected override void OnInitialized()
        {
            Console.WriteLine($"device (node id={Device.NodeId}) initialized, processing ...");

            _controlWordParameter = Vcs.SearchParameter(0x6040, 0x00) as Parameter;
            _statusWordParameter = Vcs.SearchParameter(0x6041, 0x00) as Parameter;
            
            _activeStationParameter = Vcs.SearchParameter(0x4001, 0x00) as Parameter;

            _url1Parameter = Vcs.SearchParameter(0x4000, 0x01) as Parameter;
            _url2Parameter = Vcs.SearchParameter(0x4000, 0x02) as Parameter;
            _url3Parameter = Vcs.SearchParameter(0x4000, 0x03) as Parameter;
            _url4Parameter = Vcs.SearchParameter(0x4000, 0x04) as Parameter;
            _url5Parameter = Vcs.SearchParameter(0x4000, 0x05) as Parameter;

            base.OnInitialized();
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

            //PARAM_StatusWord
            if (objectIndex == 0x6041)
            {
                if (_statusWordParameter.GetValue(out byte[] v))
                {
                    data = v;
                    result = true;
                }
            }

            //PARAM_Counter
            if (objectIndex == 0x4001)
            {
                if (_activeStationParameter.GetValue(out byte[] v))
                {
                    data = v;
                    result = true;
                }
            }

            if (objectIndex == 0x4000)
            {
                switch(objectSubindex)
                {
                    case 0x01:
                        if (_url1Parameter.GetValue(out byte[] d1))
                        {
                            data = d1;
                            result = true;
                        }
                        break;
                    case 0x02:
                        if (_url2Parameter.GetValue(out byte[] d2))
                        {
                            data = d2;
                            result = true;
                        }
                        break;
                    case 0x03:
                        if (_url3Parameter.GetValue(out byte[] d3))
                        {
                            data = d3;
                            result = true;
                        }
                        break;
                    case 0x04:
                        if (_url4Parameter.GetValue(out byte[] d4))
                        {
                            data = d4;
                            result = true;
                        }
                        break;
                    case 0x05:
                        if (_url4Parameter.GetValue(out byte[] d5))
                        {
                            data = d5;
                            result = true;
                        }
                        break;
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
            else if (objectIndex == 0x4001 && objectSubindex == 0x0)
            {
                var activeStationValue = BitConverter.ToInt32(data, 0);

                Console.WriteLine($"new active station value = {activeStationValue}");

                result = _activeStationParameter.SetValue(activeStationValue);
            }
            else if (objectIndex == 0x4000)
            {
                switch(objectSubindex)
                {
                    case 0x01:
                        result = _url1Parameter.SetValue(data);
                        break;
                    case 0x02:
                        result = _url2Parameter.SetValue(data);
                        break;
                    case 0x03:
                        result = _url3Parameter.SetValue(data);
                        break;
                    case 0x04:
                        result = _url4Parameter.SetValue(data);
                        break;
                    case 0x05:
                        result = _url5Parameter.SetValue(data);
                        break;
                }                
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
