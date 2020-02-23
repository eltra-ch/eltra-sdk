using System;
using EltraCloudContracts.Contracts.CommandSets;
using EltraCloudContracts.Contracts.Devices;

namespace ThermoMaster.DeviceManager.Device.Commands
{
    public class SetChannelStateCommand : DeviceCommand
    {
        public SetChannelStateCommand()
        {
        }

        public SetChannelStateCommand(EltraDevice device)
            : base(device)
        {
            Name = "SetChannelState";

            //In
            AddParameter("Index", TypeCode.Byte);
            AddParameter("State", TypeCode.Byte);

            //Result
            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
            AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out SetChannelStateCommand result);

            return result;
        }

        public override bool Execute(string source)
        {
            bool result = false;
            var eposDevice = Device as ThermoDevice;
            var communication = eposDevice?.Communication;
            ushort index = 0;
            byte state = 0;

            GetParameterValue("Index", ref index);
            GetParameterValue("State", ref state);

            if (communication != null)
            {
                ushort channelState = state;

                var commandResult = communication.SetObject(ThermoDeviceParameters.RelayState_Index,
                                                           (byte)(ThermoDeviceParameters.RelayState1_SubIndex + index),
                                                            BitConverter.GetBytes(channelState));

                SetParameterValue("ErrorCode", communication.LastErrorCode);
                SetParameterValue("Result", commandResult);

                result = true;
            }

            return result;
        }
    }
}
