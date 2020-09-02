using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraMaster.DeviceManager.Events;
using System;
using System.Threading.Tasks;
using EltraCommon.Contracts.Devices;
using EltraCommon.Logger;
using EltraConnector.Master.Device;

namespace TestMaster
{
    public class TestDeviceCommunication : MasterDeviceCommunication
    {
        private bool _countingRunning;
        private Task _countingTask;
        private Parameter _counterParameter;
        private Parameter _statusWordParameter;
        private Parameter _controlWordParameter;
        int _counterValue;

        private byte _byteValue;
        private ushort _ushortValue;
        private uint _uintValue;
        private ulong _ulongValue;
        private sbyte _sbyteValue;
        private short _shortValue;
        private int _intValue;
        private long _longValue;
        private double _doubleValue;

        public TestDeviceCommunication(MasterDevice device)
            : base(device)
        {
        }

        protected override void OnInitialized()
        {
            Console.WriteLine($"device (node id={Device.NodeId}) initialized, processing ...");

            _controlWordParameter = Vcs.SearchParameter(0x6040, 0x00) as Parameter;
            _statusWordParameter = Vcs.SearchParameter(0x6041, 0x00) as Parameter;
            _counterParameter = Vcs.SearchParameter(0x3000, 0x00) as Parameter;

            base.OnInitialized();
        }

        public override bool GetObject(ushort objectIndex, byte objectSubindex, ref byte[] data)
        {
            bool result = false;

            switch(objectIndex)
            {
                case 0x6040:
                    {
                        if (_controlWordParameter.GetValue(out byte[] v))
                        {
                            data = v;
                            result = true;
                        }
                    } break;
                case 0x6041:
                    {
                        if (_statusWordParameter.GetValue(out byte[] v))
                        {
                            data = v;
                            result = true;
                        }
                    } break;
                case 0x3000:
                    {
                        if (_counterParameter.GetValue(out byte[] v))
                        {
                            data = v;
                            result = true;
                        }
                    } break;
                case 0x4000:
                    {
                        switch(objectSubindex)
                        {
                            case 0x01:
                                data = BitConverter.GetBytes((byte)(object)_byteValue);
                                result = true;
                                break;
                            case 0x02:
                                data = BitConverter.GetBytes((ushort)(object)_ushortValue);
                                result = true;
                                break;
                            case 0x03:
                                data = BitConverter.GetBytes((uint)(object)_uintValue);
                                result = true;
                                break;
                            case 0x04:
                                data = BitConverter.GetBytes((ulong)(object)_ulongValue);
                                result = true;
                                break;
                        }
                        
                    } break;
            }
            
            return result;
        }

        public override bool SetObject(ushort objectIndex, byte objectSubindex, byte[] data)
        {
            bool result = false;

            switch(objectIndex)
            {
                case 0x6040:
                    {
                        var controlWordValue = BitConverter.ToUInt16(data, 0);
                        Console.WriteLine($"new controlword value = {controlWordValue}");
                        result = _controlWordParameter.SetValue(controlWordValue);
                    } break;
                case 0x3000:
                    {
                        var counterValue = BitConverter.ToInt32(data, 0);
                        Console.WriteLine($"new counter value = {counterValue}");
                        result = _counterParameter.SetValue(counterValue);
                    } break;
                case 0x4000:
                    {
                        switch(objectSubindex)
                        {
                            case 0x01:
                            {
                                _byteValue = data[0];
                                result = true;
                            }
                            break;
                            case 0x02:
                            {
                                _ushortValue = BitConverter.ToUInt16(data, 0);
                                    result = true;
                                }
                            break;
                            case 0x03:
                            {
                                _uintValue = BitConverter.ToUInt32(data, 0);
                                    result = true;
                                }
                            break;
                            case 0x04:
                            {
                                _ulongValue = BitConverter.ToUInt64(data, 0);
                                    result = true;
                                }
                            break;
                        }                        
                    }
                    break;
            }
            
            return result;
        }

        protected override void OnStatusChanged(DeviceCommunicationEventArgs e)
        {
            Console.WriteLine($"status changed, status = {e.Device.Status}, error code = {e.LastErrorCode}");

            if(e.Device.Status == DeviceStatus.Disconnected)
            {
                StopCounting();
            }

            base.OnStatusChanged(e);
        }

        internal bool StartCounting(int step, int delay)
        {
            Console.WriteLine($"start counting (node id = {Device.NodeId}), step = {step}, delay = {delay}");

            StopCounting();

            _countingTask = Task.Run(async () => {

                _countingRunning = true;
                _counterValue = 0;

                do
                {
                    _counterValue = _counterValue + step;

                    if(!_counterParameter.SetValue(_counterValue))
                    {
                        MsgLogger.WriteError($"{GetType().Name} - DummyDeviceCommunication","set counter parameter value failed!");
                    }

                    await Task.Delay(delay);
                }
                while (_countingRunning);
                
            });

            return true;
        }

        internal bool StopCounting()
        {
            if (_countingRunning)
            {
                Console.WriteLine($"stop counting (node id = {Device.NodeId})");

                _countingRunning = false;

                _countingTask.Wait();
            }

            return true;
        }
    }
}
