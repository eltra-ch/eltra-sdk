﻿using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class GetObjectCommand : DeviceCommand
    {
        public GetObjectCommand()
        {
        }

        public GetObjectCommand(EltraDevice device)
            : base(device)
        {
            Name = "GetObject";

            //In
            AddParameter("Index",TypeCode.UInt16);
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

        public override bool Execute(string sourceChannelId, string sourceLoginName)
        {
            bool result = false;
            var eposDevice = Device as EposDevice;
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

            var deviceCommunication = eposDevice?.Communication;

            if (deviceCommunication is Epos4DeviceCommunication communication)
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
