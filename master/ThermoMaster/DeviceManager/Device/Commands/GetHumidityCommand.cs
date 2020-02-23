using System;
using EltraCloudContracts.Contracts.CommandSets;

namespace ThermoMaster.DeviceManager.Device.Commands
{
    public class GetHumidityCommand : DeviceCommand
    {
        public GetHumidityCommand()
        {
        }

        public GetHumidityCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
            : base(device)
        {
            Name = "GetHumidity";

            //Out
            AddParameter("Value", TypeCode.Double, ParameterType.Out);
            AddParameter("Unit", TypeCode.String, ParameterType.Out);
            AddParameter("Timestamp", TypeCode.DateTime, ParameterType.Out);

            //Result
            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
            AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out GetHumidityCommand result);

            return result;
        }

        public override bool Execute(string source)
        {
            bool result = false;
            var device = Device as ThermoDevice;

            if (device != null)
            {
                var commandResult = device.GetInternalSample(out var sample);

                SetParameterValue("Value", sample.Humidity);
                SetParameterValue("Unit", "%");
                SetParameterValue("Timestamp", sample.Timestamp);

                SetParameterValue("ErrorCode", 0);
                SetParameterValue("Result", commandResult);

                result = true;
            }

            return result;
        }
    }
}
