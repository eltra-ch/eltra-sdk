using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;
using System;

namespace TestMaster
{
    public class StartCountingCommand : DeviceCommand
    {
        public StartCountingCommand()
        {
            Name = "StartCounting";

            //In
            AddParameter("Step", TypeCode.Int32);
            AddParameter("Delay", TypeCode.Int32);

            //Result
            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
            AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public StartCountingCommand(EltraDevice device)
            : base(device)
        {
            Name = "StartCounting";

            //In
            AddParameter("Step", TypeCode.Int32);
            AddParameter("Delay", TypeCode.Int32);

            //Result
            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
            AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out StartCountingCommand result);

            return result;
        }

        public override bool Execute(string source)
        {
            bool result = false;
            var eposDevice = Device as TestDevice;
            var communication = eposDevice?.Communication;
            int delay = 0;
            int step = 0;

            GetParameterValue("Step", ref step);
            GetParameterValue("Delay", ref delay);

            if (communication is TestDeviceCommunication deviceCommunication)
            {
                var commandResult = deviceCommunication.StartCounting(eposDevice.CloudAgent, source, step, delay);

                SetParameterValue("ErrorCode", communication.LastErrorCode);
                SetParameterValue("Result", commandResult);

                result = true;
            }

            return result;
        }
    }
}
