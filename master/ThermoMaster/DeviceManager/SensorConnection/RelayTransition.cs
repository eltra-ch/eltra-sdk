using System;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.Logger;
using ThermoMaster.DeviceManager.Device;

namespace ThermoMaster.DeviceManager.SensorConnection
{
    class RelayTransition
    {
        public static bool MakeTransition(ThermoDeviceBase device, Parameter parameter, ushort from, ushort to)
        {
            bool result = false;

            try
            {
                if (device != null && device.ReadParameter(parameter))
                {
                    if (parameter.GetValue(out ushort relayState1))
                    {
                        if (relayState1 == from)
                        {
                            MsgLogger.WriteLine($"set channel '{parameter.UniqueId}' to={to}...");

                            relayState1 = to;

                            if (parameter.SetValue(relayState1))
                            {
                                if (device.WriteParameter(parameter))
                                {
                                    MsgLogger.WriteLine($"Channel '{parameter.UniqueId}' set successfully to {to}");
                                    result = true;
                                }
                                else
                                {
                                    MsgLogger.WriteError("MakeTransition", $"cannot write parameter, set channel '{parameter.UniqueId}' state failed!");
                                }
                            }
                            else
                            {
                                MsgLogger.WriteError("MakeTransition", $"cannot set value, set channel '{parameter.UniqueId}' state failed!");
                            }
                        }
                        else
                        {
                            MsgLogger.WriteLine($"channel '{parameter.UniqueId}' already is {to}");
                            result = true;
                        }
                    }
                    else
                    {
                        MsgLogger.WriteError("MakeTransition", $"Cannot get value from parameter '{parameter.UniqueId}'");
                    }
                }
                else
                {
                    MsgLogger.WriteError("MakeTransition", $"get channel '{parameter.UniqueId}' state failed!");
                }
            }
            catch(Exception e)
            {
                MsgLogger.Exception("MakeTransition", e);
            }

            return result;
        }
    }
}
