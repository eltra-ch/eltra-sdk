using System;
using EltraMaster.Device;

namespace EposMaster.DeviceManager.Device
{
    class MotionControllerDevice : MasterDevice
    {
        #region Constructor

        public MotionControllerDevice(string name, string deviceDescriptionFilePath)
            : base(name, deviceDescriptionFilePath)
        {
            Created = DateTime.Now;
        }

        #endregion

        #region Properties
        
        public string InterfaceName { get; set; }
        public string ProtocolStackName { get; set; }
        public string PortName { get; set; }
        
        #endregion

        #region Methods

        public bool Equals(MotionControllerDevice motionControllerDevice)
        {
            bool result = Name == motionControllerDevice.Name;

            if (result && InterfaceName != motionControllerDevice.InterfaceName)
            {
                result = false;
            }

            if (result && ProtocolStackName != motionControllerDevice.ProtocolStackName)
            {
                result = false;
            }

            if (result && PortName != motionControllerDevice.PortName)
            {
                result = false;
            }

            return result;
        }

        public bool Equals(EposDevice eposDevice)
        {
            bool result = Name == eposDevice.Name;

            if (result && InterfaceName != eposDevice.InterfaceName)
            {
                result = false;
            }

            if (result && ProtocolStackName != eposDevice.ProtocolStackName)
            {
                result = false;
            }

            if (result && PortName != eposDevice.PortName)
            {
                result = false;
            }

            return result;
        }
        
        #endregion
    }
}
