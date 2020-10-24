using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraMaster.DeviceManager.Events;
using System;
using System.Threading.Tasks;
using EltraCommon.Contracts.Devices;
using EltraCommon.Logger;
using EltraConnector.Master.Device;
using EltraCommon.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.Contracts.Channels;
using EltraConnector.SyncAgent;
using System.Diagnostics;

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

        private XddParameter _byteParameter;
        private XddParameter _ushortParameter;
        private XddParameter _uintParameter;
        private XddParameter _ulongParameter;
        private XddParameter _sbyteParameter;
        private XddParameter _shortParameter;
        private XddParameter _intParameter;
        private XddParameter _longParameter;
        private XddParameter _doubleParameter;
        private XddParameter _stringParameter;
        private XddParameter _objectParameter;
        private XddParameter _dateTimeParameter;
        private XddParameter _booleanParameter;
        private XddParameter _identityParameter;

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

            _byteParameter = Vcs.SearchParameter(0x4000, 0x01) as XddParameter;
            _ushortParameter = Vcs.SearchParameter(0x4000, 0x02) as XddParameter;
            _uintParameter = Vcs.SearchParameter(0x4000, 0x03) as XddParameter;
            _ulongParameter = Vcs.SearchParameter(0x4000, 0x04) as XddParameter;

            _sbyteParameter = Vcs.SearchParameter(0x4000, 0x05) as XddParameter;
            _shortParameter = Vcs.SearchParameter(0x4000, 0x06) as XddParameter;
            _intParameter = Vcs.SearchParameter(0x4000, 0x07) as XddParameter;
            _longParameter = Vcs.SearchParameter(0x4000, 0x08) as XddParameter;
            _doubleParameter = Vcs.SearchParameter(0x4000, 0x09) as XddParameter;
            _stringParameter = Vcs.SearchParameter(0x4000, 0x0A) as XddParameter;
            _objectParameter = Vcs.SearchParameter(0x4000, 0x0B) as XddParameter;
            _dateTimeParameter = Vcs.SearchParameter(0x4000, 0x0C) as XddParameter;
            _booleanParameter = Vcs.SearchParameter(0x4000, 0x0D) as XddParameter;
            _identityParameter = Vcs.SearchParameter(0x4000, 0x0E) as XddParameter;

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
                                {
                                    if (_byteParameter.GetValue(out byte[] d))
                                    {
                                        data = d;
                                        result = true;
                                    }
                                }
                                break;
                            case 0x02:
                                {
                                    if (_ushortParameter.GetValue(out byte[] d))
                                    {
                                        data = d;
                                        result = true;
                                    }
                                }
                                break;
                            case 0x03:
                                {
                                    if (_uintParameter.GetValue(out byte[] d))
                                    {
                                        data = d;
                                        result = true;
                                    }
                                }
                                break;
                            case 0x04:
                                {
                                    if (_ulongParameter.GetValue(out byte[] d))
                                    {
                                        data = d;
                                        result = true;
                                    }
                                }
                                break;
                            case 0x05:
                                {
                                    if (_sbyteParameter.GetValue(out byte[] d))
                                    {
                                        data = d;
                                        result = true;
                                    }
                                }
                                break;
                            case 0x06:
                                {
                                    if (_shortParameter.GetValue(out byte[] d))
                                    {
                                        data = d;
                                        result = true;
                                    }
                                }
                                break;
                            case 0x07:
                                {
                                    if (_intParameter.GetValue(out byte[] d))
                                    {
                                        data = d;
                                        result = true;
                                    }
                                }
                                break;
                            case 0x08:
                                {
                                    if (_longParameter.GetValue(out byte[] d))
                                    {
                                        data = d;
                                        result = true;
                                    }
                                }
                                break;
                            case 0x09:
                                {
                                    if (_doubleParameter.GetValue(out byte[] d))
                                    {
                                        data = d;
                                        result = true;
                                    }
                                }
                                break;
                            case 0x0A:
                                {
                                    if (_stringParameter.GetValue(out byte[] d))
                                    {
                                        data = d;
                                        result = true;
                                    }
                                }
                                break;
                            case 0x0B:
                                {
                                    if (_objectParameter.GetValue(out byte[] d))
                                    {
                                        data = d;
                                        result = true;
                                    }
                                }
                                break;
                            case 0x0C:
                                {
                                    if (_dateTimeParameter.GetValue(out byte[] d))
                                    {
                                        data = d;
                                        result = true;
                                    }
                                }
                                break;
                            case 0x0D:
                                {
                                    if (_booleanParameter.GetValue(out byte[] d))
                                    {
                                        data = d;
                                        result = true;
                                    }
                                }
                                break;
                            case 0x0E:
                                {
                                    if (_identityParameter.GetValue(out byte[] d))
                                    {
                                        data = d;
                                        result = true;
                                    }
                                }
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
                                Console.WriteLine($"new byte value = {data[0]}");
                                result = _byteParameter.SetValue(data[0]);
                            }
                            break;
                            case 0x02:
                            {
                                var v = BitConverter.ToUInt16(data, 0);
                                Console.WriteLine($"new ushort value = {v}");
                                result = _ushortParameter.SetValue(v);
                            }
                            break;
                            case 0x03:
                            {
                                var v = BitConverter.ToUInt32(data, 0);
                                Console.WriteLine($"new uint value = {v}");
                                result = _uintParameter.SetValue(v);                              
                            }
                            break;
                            case 0x04:
                            {
                                var v = BitConverter.ToUInt64(data, 0);
                                Console.WriteLine($"new uint64 value = {v}");
                                result = _ulongParameter.SetValue(v);
                            }
                            break;
                            case 0x05:
                                {
                                    Console.WriteLine($"new sbyte value = {data[0]}");
                                    result = _sbyteParameter.SetValue((sbyte)data[0]);
                                }
                                break;
                            case 0x06:
                                {
                                    var v = BitConverter.ToInt16(data, 0);
                                    Console.WriteLine($"new short value = {v}");
                                    result = _shortParameter.SetValue(v);
                                }
                                break;
                            case 0x07:
                                {
                                    var v = BitConverter.ToInt32(data, 0);
                                    Console.WriteLine($"new int value = {v}");
                                    result = _intParameter.SetValue(v);
                                }
                                break;
                            case 0x08:
                                {
                                    var v = BitConverter.ToInt64(data, 0);
                                    Console.WriteLine($"new int64 value = {v}");
                                    result = _longParameter.SetValue(v);
                                }
                                break;
                            case 0x09:
                                {
                                    var v = BitConverter.ToDouble(data, 0);
                                    Console.WriteLine($"new double value = {v}");
                                    result = _doubleParameter.SetValue(v);
                                }
                                break;
                            case 0x0A:
                                {
                                    result = _stringParameter.SetValue(data);

                                    if (result)
                                    {
                                        result = _stringParameter.GetValue(out string v);

                                        if (result)
                                        {
                                            Console.WriteLine($"new string value '{v}'");
                                        }
                                    }
                                }
                                break;
                            case 0x0B:
                                {                                    
                                    Console.WriteLine($"new object /PARAM_OCTET_STRING/ value");
                                    result = _objectParameter.SetValue(data);
                                }
                                break;
                            case 0x0C:
                                {
                                    result = _dateTimeParameter.SetValue(data);

                                    _dateTimeParameter.GetValue(out DateTime newVal);

                                    Console.WriteLine($"new date time value {newVal}");
                                }
                                break;
                            case 0x0D:
                                {
                                    result = _booleanParameter.SetValue(data);

                                    if (result)
                                    {
                                        if(_booleanParameter.GetValue(out bool newVal))
                                        {
                                            Console.WriteLine($"new boolean value {newVal}");
                                        }
                                    }
                                }
                                break;
                            case 0x0E:
                                {
                                    Console.WriteLine($"new identity value");

                                    result = _identityParameter.SetValue(data);
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

        internal bool StartCounting(SyncCloudAgent agent, string source, int step, int delay)
        {
            Console.WriteLine($"start counting (node id = {Device.NodeId}), step = {step}, delay = {delay}");

            StopCounting();

            _countingTask = Task.Run(async () => {

                var stopWatch = new Stopwatch();
                const double maxRunTimeInMs = 60000;

                _countingRunning = true;
                _counterValue = 0;

                agent.RemoteChannelStatusChanged += (a, o) => 
                { 
                    if(o.Id == source && o.Status == ChannelStatus.Offline)
                    {
                        _countingRunning = false;
                    }
                };

                stopWatch.Start();

                do
                {
                    _counterValue = _counterValue + step;

                    if(!_counterParameter.SetValue(_counterValue))
                    {
                        MsgLogger.WriteError($"{GetType().Name} - DummyDeviceCommunication","set counter parameter value failed!");
                    }

                    await Task.Delay(delay);
                }
                while (_countingRunning && stopWatch.ElapsedMilliseconds < maxRunTimeInMs);
                
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
