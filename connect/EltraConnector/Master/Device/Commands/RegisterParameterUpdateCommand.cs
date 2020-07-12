using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Node;
using EltraCommon.Contracts.Parameters;

namespace EltraConnector.Master.Device.Commands
{
    public class RegisterParameterUpdateCommand : DeviceCommand
    {
        public RegisterParameterUpdateCommand()
        { }

        public RegisterParameterUpdateCommand(EltraDeviceNode device)
            : base(device)
        {
            Name = "RegisterParameterUpdate";

            //In
            AddParameter("Index", TypeCode.UInt16);
            AddParameter("SubIndex", TypeCode.Byte);
            AddParameter("Priority", TypeCode.Int32);

            //Result
            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out RegisterParameterUpdateCommand result);

            return result;
        }

        public override bool Execute(string source)
        {
            bool result = false;
            var device = Device as MasterDevice;
            var communication = device?.Communication;
            ushort index = 0;
            byte subIndex = 0;
            int priority = 0;

            GetParameterValue("Index", ref index);
            GetParameterValue("SubIndex", ref subIndex);
            GetParameterValue("Priority", ref priority);

            if (communication != null)
            {
                var manager = device.ParameterConnectionManager;

                bool commandResult = false;

                if (manager != null)
                {
                    commandResult = manager.RegisterParameter(source, index, subIndex, (ParameterUpdatePriority)priority);
                }

                SetParameterValue("Result", commandResult);

                result = true;
            }

            return result;
        }
    }
}
