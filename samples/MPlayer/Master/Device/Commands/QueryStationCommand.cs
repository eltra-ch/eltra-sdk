using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace MPlayerMaster.Device.Commands
{
    public class QueryStationCommand : DeviceCommand
    {
        public QueryStationCommand()
        {
        }

        public QueryStationCommand(EltraDevice device)
            : base(device)
        {
            Name = "QueryStation";

            //In
            AddParameter("Query", TypeCode.String);

            //Result
            AddParameter("Result", TypeCode.String, ParameterType.Out);
            AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out QueryStationCommand result);

            return result;
        }

        public override bool Execute(string channelId, string loginName)
        {
            bool result = false;
            var eposDevice = Device as MPlayerDevice;
            var communication = eposDevice?.Communication;
            string query = string.Empty;
            
            GetParameterValue("Query", ref query);
           
            if (communication is MPlayerDeviceCommunication deviceCommunication)
            {
                var commandResult = deviceCommunication.QueryStation(query);

                SetParameterValue("ErrorCode", communication.LastErrorCode);
                SetParameterValue("Result", commandResult);

                result = true;
            }

            return result;
        }
    }
}
