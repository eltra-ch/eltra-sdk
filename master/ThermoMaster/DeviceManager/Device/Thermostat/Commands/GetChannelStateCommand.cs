using System;
using EltraCloudContracts.Contracts.CommandSets;

namespace ThermoMaster.DeviceManager.Device.Thermostat.Commands
{
    public class GetChannelStateCommand : DeviceCommand
    {
        public GetChannelStateCommand()
        {
        }

        public GetChannelStateCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
            : base(device)
        {
            Name = "GetChannelState";

            //In
            AddParameter("Index",TypeCode.Byte);
            
            //Out
            AddParameter("State", TypeCode.Byte);

            //Result
            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
            AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out GetChannelStateCommand result);
            
            return result;
        }

        public override bool Execute(string source)
        {
            bool result = false;
            var device = Device as ThermoDeviceBase;
            var communication = device?.Communication;
            ushort index = 0;
            
            GetParameterValue("Index", ref index);
            
            if (communication != null)
            {
                var commandResult = communication.GetChannelState(index, out var state);

                SetParameterValue("State", state);
                SetParameterValue("ErrorCode", communication.LastErrorCode);
                SetParameterValue("Result", commandResult);

                result = true;
            }

            return result;
        }
    }
}
