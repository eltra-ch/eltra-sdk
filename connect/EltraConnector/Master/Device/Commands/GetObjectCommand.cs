using System;
using EltraCommon.Contracts.CommandSets;

namespace EltraConnector.Master.Device.Commands
{
    public class GetObjectCommand : DeviceCommand
    {
        public GetObjectCommand()
        {
        }

        public GetObjectCommand(EltraCommon.Contracts.Devices.EltraDevice device)
            : base(device)
        {
            Name = "GetObject";

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
            Clone(out GetObjectCommand result);

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

            GetParameterDataType("Data", out TypeCode typeCode);

            if (typeCode != TypeCode.Object)
            {
                SetParameterDataType("Data", TypeCode.Object);
            }

            GetParameterValue("Data", ref data);

            if (communication != null)
            {
                var commandResult = communication.GetObject(index, subIndex, ref data);

                if (typeCode != TypeCode.Object)
                {
                    SetParameterDataType("Data", typeCode);
                }

                SetParameterValue("Data", data);
                SetParameterValue("ErrorCode", communication.LastErrorCode);
                SetParameterValue("Result", commandResult);

                result = true;
            }

            return result;
        }
    }
}
