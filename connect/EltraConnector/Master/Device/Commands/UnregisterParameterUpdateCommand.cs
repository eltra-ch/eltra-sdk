﻿using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;
using EltraCommon.Contracts.Parameters;

#pragma warning disable 1591

namespace EltraConnector.Master.Device.Commands
{
    public class UnregisterParameterUpdateCommand : DeviceCommand
    {
        public UnregisterParameterUpdateCommand()
        { }

        public UnregisterParameterUpdateCommand(EltraDevice device)
            : base(device)
        {
            Name = "UnregisterParameterUpdate";

            //In
            AddParameter("Index", TypeCode.UInt16);
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

        public override bool Execute(string sourceChannelId, string sourceLoginName)
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
                    commandResult = manager.UnregisterParameter(sourceChannelId, index, subIndex, (ParameterUpdatePriority)priority);
                }

                SetParameterValue("Result", commandResult);

                result = true;
            }

            return result;
        }
    }
}
