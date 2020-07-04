using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraMaster.DeviceManager.Events;
using System;
using System.Threading.Tasks;
using EltraCommon.Contracts.Devices;
using EltraCommon.Logger;
using EltraConnector.Master.Device;

namespace ConsoleApp1
{
    public class DummyDeviceCommunication : MasterDeviceCommunication
    {
        private bool _countingRunning;
        private Task _countingTask;
        private Parameter _counterParameter;
        private Parameter _statusWordParameter;
        private Parameter _controlWordParameter;
        private int _counterValue;

        public DummyDeviceCommunication(MasterDevice device, uint updateInterval, uint timeout)
            : base(device, updateInterval, timeout)
        {
        }

        protected override void OnInitialized()
        {
            Console.WriteLine("device initialized, processing ...");

            _controlWordParameter = Vcs.SearchParameter(0x6040, 0x00) as Parameter;
            _statusWordParameter = Vcs.SearchParameter(0x6041, 0x00) as Parameter;
            _counterParameter = Vcs.SearchParameter(0x3000, 0x00) as Parameter;

            base.OnInitialized();
        }

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
            if (objectIndex == 0x3000)
            {
                if (_counterParameter.GetValue(out byte[] v))
                {
                    data = v;
                    result = true;
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
            else if (objectIndex == 0x3000 && objectSubindex == 0x0)
            {
                var counterValue = BitConverter.ToInt32(data, 0);

                Console.WriteLine($"new counter value = {counterValue}");

                result = _counterParameter.SetValue(counterValue);
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
            Console.WriteLine($"start counting, step = {step}, delay = {delay}");

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
                Console.WriteLine($"stop counting");

                _countingRunning = false;

                _countingTask.Wait();
            }

            return true;
        }
    }
}
