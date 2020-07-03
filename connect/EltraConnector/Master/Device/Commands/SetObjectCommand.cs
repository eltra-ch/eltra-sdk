using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EltraConnector.Master.Device.Commands
{
    public class SetObjectCommand : DeviceCommand
    {
        public SetObjectCommand()
        {
        }

        public SetObjectCommand(EltraDevice device)
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
            var device = Device as MasterDevice;
            var communication = device?.Communication;
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
