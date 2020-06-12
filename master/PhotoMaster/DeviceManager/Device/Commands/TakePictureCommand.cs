using System;
using EltraCloudContracts.Contracts.CommandSets;
using EltraCloudContracts.Contracts.Devices;

namespace PhotoMaster.DeviceManager.Device.Commands
{
    public class TakePictureCommand : DeviceCommand
    {
        public TakePictureCommand()
        {
        }

        public TakePictureCommand(EltraDevice device)
            : base(device)
        {
            Name = "TakePicture";

            //In
            AddParameter("Index", TypeCode.Byte);

            //Result
            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
            AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out TakePictureCommand result);

            return result;
        }

        public override bool Execute(string source)
        {
            bool result = false;
            var eposDevice = Device as PhotoDevice;
            var communication = eposDevice?.Communication;
            ushort index = 0;
            
            GetParameterValue("Index", ref index);

            if (communication is PhotoDeviceCommunication deviceCommunication)
            {
                var commandResult = deviceCommunication.TakePicture(index);

                SetParameterValue("ErrorCode", communication.LastErrorCode);
                SetParameterValue("Result", commandResult);

                result = true;
            }

            return result;
        }
    }
}
