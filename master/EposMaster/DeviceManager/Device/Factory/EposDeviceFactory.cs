namespace EposMaster.DeviceManager.Device.Factory
{
    static class EposDeviceFactory
    {
        private static string GetDeviceDescriptionFile(string deviceName, string interfaceName, string protocolStackName, string portName)
        {
            uint errorCode = 0;
            ushort nodeId = 0;
            ushort hardwareVersion = 0;
            ushort softwareVersion = 0;
            ushort applicationNumber = 0;
            ushort applicationVersion = 0;
            string result = string.Empty;

            int keyHandle = VcsWrapper.Device.VcsOpenDevice(deviceName, protocolStackName, interfaceName, portName, ref errorCode);

            if (keyHandle != 0)
            {
                VcsWrapper.Device.VcsGetVersion(keyHandle, nodeId, ref hardwareVersion, ref softwareVersion, ref applicationNumber, ref applicationVersion, ref errorCode);

                result = $"{deviceName}_{softwareVersion:X4}h_{hardwareVersion:X4}h_{applicationNumber:X4}h_{applicationVersion:X4}h.xdd";
            }

            return result;
        }


        public static EposDevice CreateDevice(string deviceName, string interfaceName, string protocolStackName, string portName, uint updateInterval, uint timeout)
        {
            EposDevice result = null;
            string deviceDescriptionFile = GetDeviceDescriptionFile(deviceName, interfaceName, protocolStackName, portName);

            switch (deviceName)
            {
                case "EPOS2":
                    result = new Epos2Device(deviceDescriptionFile, updateInterval, timeout) { InterfaceName = interfaceName, ProtocolStackName = protocolStackName, PortName = portName};
                    break;
                case "EPOS4":
                    result = new Epos4Device(deviceDescriptionFile, updateInterval, timeout) { InterfaceName = interfaceName, ProtocolStackName = protocolStackName, PortName = portName };
                    break;
            }

            return result;
        }
    }
}
