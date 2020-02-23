using System;
using EltraCloudContracts.Contracts.CommandSets;

namespace Thermometer.DeviceManager.Device.Commands
{
    public class SetObjectCommand : DeviceCommand
    {
        public SetObjectCommand()
        {
        }

        public SetObjectCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
            : base(device)
        {
            Name = "SetObject";

            //In
            AddParameter("Index", TypeCode.UInt16);
            AddParameter("SubIndex", TypeCode.Byte);

            //Out
            AddParameter("Data", TypeCode.Object);

            //Result
            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
            AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out SetObjectCommand result);
            
            return result;
        }

        public override bool Execute(string source)
        {
            bool result = false;
            var eposDevice = Device as ThermoDevice;
            var communication = eposDevice?.Communication;
            ushort index = 0;
            byte subIndex = 0;
            var data = new byte[] { };

            GetParameterValue("Index", ref index);
            GetParameterValue("SubIndex", ref subIndex);
            GetParameterValue("Data", ref data);

            if (communication != null)
            {
                var commandResult = communication.SetObject(index, subIndex, data);

                SetParameterValue("ErrorCode", communication.LastErrorCode);
                SetParameterValue("Result", commandResult);

                result = true;
            }

            return result;
        }
    }
}
