using System;
using EltraCloudContracts.Contracts.CommandSets;
using EltraCloudContracts.Contracts.Devices;

namespace PhotoMaster.DeviceManager.Device.Commands
{
    public class RecordVideoCommand : DeviceCommand
    {
        public RecordVideoCommand()
        {
        }

        public RecordVideoCommand(EltraDevice device)
            : base(device)
        {
            Name = "RecordVideo";

            //In
            AddParameter("DurationInSec", TypeCode.Int32);

            //Result
            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
            AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out RecordVideoCommand result);

            return result;
        }

        public override bool Execute(string source)
        {
            bool result = false;
            var eposDevice = Device as PhotoDevice;
            var communication = eposDevice?.Communication;
            int durationInSec = 0;
            
            GetParameterValue("DurationInSec", ref durationInSec);

            if (communication is PhotoDeviceCommunication deviceCommunication)
            {
                var commandResult = deviceCommunication.RecordVideo(durationInSec);

                SetParameterValue("ErrorCode", communication.LastErrorCode);
                SetParameterValue("Result", commandResult);

                result = true;
            }

            return result;
        }
    }
}
