using EltraCloudContracts.Contracts.CommandSets;
using EltraCloudContracts.Contracts.Devices;
using System;

namespace ConsoleApp1
{
    class StopCountingCommand : DeviceCommand
    {
        public StopCountingCommand(EltraDevice device)
            : base(device)
        {
            Name = "StopCounting";

            //Result
            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
            AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out StopCountingCommand result);

            return result;
        }

        public override bool Execute(string source)
        {
            bool result = false;
            var eposDevice = Device as DummyDevice;
            var communication = eposDevice?.Communication;
            
            if (communication is DummyDeviceCommunication deviceCommunication)
            {
                var commandResult = deviceCommunication.StopCounting();

                SetParameterValue("ErrorCode", communication.LastErrorCode);
                SetParameterValue("Result", commandResult);

                result = true;
            }

            return result;
        }
    }
}
