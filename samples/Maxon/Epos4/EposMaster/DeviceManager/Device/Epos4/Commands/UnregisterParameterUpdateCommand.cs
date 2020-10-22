using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Parameters;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class UnregisterParameterUpdateCommand : DeviceCommand
    {
        public UnregisterParameterUpdateCommand()
        { }
        
        public UnregisterParameterUpdateCommand(EltraDevice device)
            :base(device)
        {
            Name = "UnregisterParameterUpdate";

            //In
            AddParameter("Index",TypeCode.UInt16);
            AddParameter("SubIndex", TypeCode.Byte);
            AddParameter("Priority", TypeCode.Int32);

            //Result
            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out UnregisterParameterUpdateCommand result);
            
            return result;
        }

        public override bool Execute(string source)
        {
            bool result = false;
            var eposDevice = Device as EposDevice;
            var communication = eposDevice?.Communication;
            ushort index = 0;
            byte subIndex = 0;
            int priority = 0;

            GetParameterValue("Index", ref index);
            GetParameterValue("SubIndex", ref subIndex);
            GetParameterValue("Priority", ref priority);

            if (communication != null)
            {
                var manager = eposDevice.ParameterConnectionManager;

                bool commandResult = false;

                if (manager != null)
                {
                    commandResult = manager.UnregisterParameter(source, index, subIndex, (ParameterUpdatePriority)priority);
                }

                SetParameterValue("Result", commandResult);

                result = true;
            }

            return result;
        }
    }
}
