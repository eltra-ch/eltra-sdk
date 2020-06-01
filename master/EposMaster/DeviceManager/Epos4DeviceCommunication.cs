using System;
using EltraCommon.Logger;
using EltraMaster.Device;
using EltraMaster.DeviceManager.Events;
using EposMaster.DeviceManager.Device;
using EposMaster.DeviceManager.Events;
using EposMaster.DeviceManager.VcsWrapper;

namespace EposMaster.DeviceManager
{
    class Epos4DeviceCommunication : MasterDeviceCommunication
    {
        #region Private fields

        private readonly CommunicationStatusManager _statusManager;
        private static readonly object SyncObject = new object();
        
        #endregion

        #region Constructors

        public Epos4DeviceCommunication(EposDevice device, uint updateInterval, uint timeout)
            : base(device, updateInterval, timeout)
        {
            _statusManager = new CommunicationStatusManager(device);
        }

        #endregion

        #region Properties

        public int KeyHandle { get; private set; }

        public ushort NodeId { get; set; }
        
        public bool Connected
        {
            get
            {
                return _statusManager.IsConnected();
            }            
        }

        #endregion
        
        #region Methods
        
        public bool Connect()
        {
            bool result = false;
            
            MsgLogger.WriteDebug($"{GetType().Name} - method", "Connect ...");

            if (!Connected)
            {
                lock (this)
                {
                    try
                    {
                        uint lastErrorCode = 0;
                        var device = Device as EposDevice;

                        lock (SyncObject)
                        {
                            MsgLogger.WriteDebug($"{GetType().Name} - method", $"VcsOpenDevice - device name='{device.Family}', interface='{device.InterfaceName}', port='{device.PortName}'");

                            KeyHandle = VcsWrapper.Device.VcsOpenDevice(device.Family, device.ProtocolStackName,
                                device.InterfaceName, device.PortName, ref lastErrorCode);
                        }

                        var status = KeyHandle != 0
                            ? EposCommunicationStatus.Connected
                            : EposCommunicationStatus.Failed;

                        result = status == EposCommunicationStatus.Connected;

                        if (status == EposCommunicationStatus.Connected)
                        {
                            _statusManager.SetConnected();
                        }

                        MsgLogger.WriteDebug($"{GetType().Name} - method", $"Connect -  send new status = {status}");

                        OnStatusChanged(new EposCommunicationEventArgs
                            {Device = device, LastErrorCode = lastErrorCode, Status = status});
                    }
                    catch (Exception e)
                    {
                        MsgLogger.Exception($"{GetType().Name} - method", e);
                    }
                }
            }
            else
            {
                result = true;
            }

            MsgLogger.WriteDebug($"{GetType().Name} - method", $"Connect - result = {result}");

            return result;
        }
        
        public void Disconnect()
        {
            uint lastErrorCode = 0;
            var device = Device as EposDevice;

            lock (this)
            {
                if (KeyHandle != 0)
                {
                    VcsWrapper.Device.VcsCloseDevice(KeyHandle, ref lastErrorCode);

                    KeyHandle = 0;

                    _statusManager.SetDisconnected();
                    
                    OnStatusChanged(new EposCommunicationEventArgs { Device = device, LastErrorCode = lastErrorCode, Status = EposCommunicationStatus.Disconnected });
                }
            }
        }

        public override bool GetObject(ushort objectIndex, byte objectSubindex, ref byte[] data)
        {
            bool result;
            uint lastErrorCode = 0;
            uint numberOfBytesRead = 0;

            lock (SyncObject)
            {
                var numberOfBytesToRead = (uint)data.Length;

                MsgLogger.WriteDebug($"{GetType().Name} - method", $"VcsGetObject - 0x{objectIndex:X4}, 0x{objectSubindex}, length = {numberOfBytesToRead}");

                result = VcsWrapper.Device.VcsGetObject(KeyHandle, NodeId, objectIndex, objectSubindex, data, 
                             numberOfBytesToRead, ref numberOfBytesRead, ref lastErrorCode) > 0;

                if (numberOfBytesToRead != numberOfBytesRead)
                {
                    result = false;
                }
            }

            LastErrorCode = lastErrorCode;

            return result;
        }

        public override bool SetObject(ushort objectIndex, byte objectSubindex, byte[] data)
        {
            bool result;
            uint lastErrorCode = 0;
            uint numberOfBytesWritten = 0;

            lock (SyncObject)
            {
                uint numberOfBytesToWrite = (uint)data.Length;

                result = VcsWrapper.Device.VcsSetObject(KeyHandle, NodeId, objectIndex, objectSubindex, data, 
                             numberOfBytesToWrite, ref numberOfBytesWritten, ref lastErrorCode) > 0;

                if (numberOfBytesToWrite != numberOfBytesWritten)
                {
                    result = false;
                }
            }

            LastErrorCode = lastErrorCode;

            return result;
        }
        
        public bool GetEnableState(out bool enabled)
        {
            int isEnabled = 0;
            uint lastErrorCode = 0;

            enabled = false;

            var result = VcsWrapper.Device.VcsGetEnableState(KeyHandle, NodeId, ref isEnabled, ref lastErrorCode) > 0;

            if (isEnabled > 0)
            {
                enabled = isEnabled > 0;
            }

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool GetDisableState(out bool disabled)
        {
            int isDisabled = 0;
            uint lastErrorCode = 0;

            disabled = false;

            var result = VcsWrapper.Device.VcsGetDisableState(KeyHandle, NodeId, ref isDisabled, ref lastErrorCode) > 0;

            if (isDisabled > 0)
            {
                disabled = isDisabled > 0;
            }

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool SetEnableState()
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsSetEnableState(KeyHandle, NodeId, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool SetDisableState()
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsSetDisableState(KeyHandle, NodeId, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool ClearFault()
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsClearFault(KeyHandle, NodeId, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool GetFaultState(out bool fault)
        {
            uint lastErrorCode = 0;
            int isFault = 0;

            fault = false;

            var result = VcsWrapper.Device.VcsGetFaultState(KeyHandle, NodeId, ref isFault, ref lastErrorCode) > 0;

            if (result)
            {
                fault = isFault > 0;
            }

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool ActivateProfileVelocityMode()
        {
            uint lastErrorCode = 0;
            
            var result = VcsWrapper.Device.VcsActivateProfileVelocityMode(KeyHandle, NodeId, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool ActivateProfilePositionMode()
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsActivateProfilePositionMode(KeyHandle, NodeId, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool GetCurrentIs(out short current)
        {
            uint lastErrorCode = 0;

            current = 0;

            var result = VcsWrapper.Device.VcsGetCurrentIs(KeyHandle, NodeId, ref current, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool GetCurrentMust(out short current)
        {
            uint lastErrorCode = 0;

            current = 0;

            var result = VcsWrapper.Device.VcsGetCurrentMust(KeyHandle, NodeId, ref current, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool GetCurrentIsAveraged(out short current)
        {
            uint lastErrorCode = 0;

            current = 0;

            var result = VcsWrapper.Device.VcsGetCurrentIsAveraged(KeyHandle, NodeId, ref current, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool GetVelocityIs(out int velocity)
        {
            uint lastErrorCode = 0;

            velocity = 0;

            var result = VcsWrapper.Device.VcsGetVelocityIs(KeyHandle, NodeId, ref velocity, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool GetVelocityIsAveraged(out int velocity)
        {
            uint lastErrorCode = 0;

            velocity = 0;

            var result = VcsWrapper.Device.VcsGetVelocityIsAveraged(KeyHandle, NodeId, ref velocity, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool GetPositionIs(out int position)
        {
            uint lastErrorCode = 0;

            position = 0;

            var result = VcsWrapper.Device.VcsGetPositionIs(KeyHandle, NodeId, ref position, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool SetMotorType(EMotorType motorType)
        {
            uint lastErrorCode = 0;
            
            var result = VcsWrapper.Device.VcsSetMotorType(KeyHandle, NodeId, motorType, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool GetMotorType(ref EMotorType motorType)
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsGetMotorType(KeyHandle, NodeId, ref motorType, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool ActivateCurrentMode()
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsActivateCurrentMode(KeyHandle, NodeId, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool SetCurrentMust(short current)
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsSetCurrentMust(KeyHandle, NodeId, current, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool MoveWithVelocity(int velocity)
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsMoveWithVelocity(KeyHandle, NodeId, velocity, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool MoveToPosition(int position, bool absolute, bool immediately)
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsMoveToPosition(KeyHandle, NodeId, position, absolute ? 1:0, immediately ? 1 : 0, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool GetQuickStopState(ref int isQuickStopped)
        {
            uint lastErrorCode = 0;
            
            var result = VcsWrapper.Device.VcsGetQuickStopState(KeyHandle, NodeId, ref isQuickStopped, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool SetQuickStopState()
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsSetQuickStopState(KeyHandle, NodeId, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool SetOperationMode(EOperationMode mode)
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsSetOperationMode(KeyHandle, NodeId, mode, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool ActivatePositionMode()
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsActivatePositionMode(KeyHandle, NodeId, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool GetErrorInfo(uint errorCode, ref string errorInfo)
        {
            const int maxStringSize = 255;

            var result = VcsWrapper.Device.VcsGetErrorInfo(errorCode, ref errorInfo, maxStringSize) > 0;
            
            return result;
        }

        public bool GetNbOfDeviceError(ref byte numberOfDeviceError)
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsGetNbOfDeviceError(KeyHandle, NodeId,ref numberOfDeviceError, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool GetOperationMode(ref EOperationMode mode)
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsGetOperationMode(KeyHandle, NodeId, ref mode, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool ResetDevice()
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsResetDevice(KeyHandle, NodeId, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool Store()
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsStore(KeyHandle, NodeId, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool Restore()
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsRestore(KeyHandle, NodeId, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool GetPositionMust(out int position)
        {
            uint lastErrorCode = 0;

            position = 0;

            var result = VcsWrapper.Device.VcsGetPositionMust(KeyHandle, NodeId, ref position, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool SetPositionMust(int must)
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsSetPositionMust(KeyHandle, NodeId, must, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool GetVelocityMust(out int velocity)
        {
            uint lastErrorCode = 0;

            velocity = 0;

            var result = VcsWrapper.Device.VcsGetVelocityMust(KeyHandle, NodeId, ref velocity, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result; 
        }

        public bool SetVelocityMust(int must)
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsSetVelocityMust(KeyHandle, NodeId, must, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool SetPositionProfile(uint profileVelocity, uint profileAcceleration, uint profileDeceleration)
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsSetPositionProfile(KeyHandle, NodeId, profileVelocity, profileAcceleration, profileDeceleration, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool GetVelocityProfile(ref uint profileAcceleration, ref uint profileDeceleration)
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsGetVelocityProfile(KeyHandle, NodeId, ref profileAcceleration, ref profileDeceleration, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool SetVelocityProfile(uint profileAcceleration, uint profileDeceleration)
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsSetVelocityProfile(KeyHandle, NodeId, profileAcceleration, profileDeceleration, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool GetState(ref EStates state)
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsGetState(KeyHandle, NodeId, ref state, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool SetState(EStates state)
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsSetState(KeyHandle, NodeId, state, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool GetMaxAcceleration(ref uint maxAcceleration)
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsGetMaxAcceleration(KeyHandle, NodeId, ref maxAcceleration, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool SetMaxAcceleration(uint maxAcceleration)
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsSetMaxAcceleration(KeyHandle, NodeId, maxAcceleration, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool GetMaxFollowingError(ref uint maxFollowingError)
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsGetMaxFollowingError(KeyHandle, NodeId, ref maxFollowingError, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool SetMaxFollowingError(uint maxFollowingError)
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsSetMaxFollowingError(KeyHandle, NodeId, maxFollowingError, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool GetMaxProfileVelocity(ref uint maxProfileVelocity)
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsGetMaxProfileVelocity(KeyHandle, NodeId, ref maxProfileVelocity, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool SetMaxProfileVelocity(uint maxProfileVelocity)
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsSetMaxProfileVelocity(KeyHandle, NodeId, maxProfileVelocity, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool WaitForTargetReached(uint timeout)
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsWaitForTargetReached(KeyHandle, NodeId, timeout, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool GetAllDigitalInputs(out ushort digitalInputs)
        {
            uint lastErrorCode = 0;

            digitalInputs = 0;

            var result = VcsWrapper.Device.VcsGetAllDigitalInputs(KeyHandle, NodeId, ref digitalInputs, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool GetAllDigitalOutputs(out ushort digitalOutputs)
        {
            uint lastErrorCode = 0;

            digitalOutputs = 0;

            var result = VcsWrapper.Device.VcsGetAllDigitalInputs(KeyHandle, NodeId, ref digitalOutputs, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool SetAllDigitalOutputs(ushort digitalOutputs)
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsSetAllDigitalOutputs(KeyHandle, NodeId, digitalOutputs, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool HaltPositionMovement()
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsHaltPositionMovement(KeyHandle, NodeId, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool HaltVelocityMovement()
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsHaltVelocityMovement(KeyHandle, NodeId, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }
        
        public bool GetAnalogInput(ushort inputNumber, out ushort analogValue)
        {
            uint lastErrorCode = 0;

            analogValue = 0;

            var result = VcsWrapper.Device.VcsGetAnalogInput(KeyHandle, NodeId, inputNumber, ref analogValue, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool SetAnalogOutput(ushort outputNumber, ushort analogValue)
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsSetAnalogOutput(KeyHandle, NodeId, outputNumber, analogValue, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool GetAnalogInputState(EAnalogInputConfiguration config, out int stateValue)
        {
            uint lastErrorCode = 0;

            stateValue = 0;

            var result = VcsWrapper.Device.VcsGetAnalogInputState(KeyHandle, NodeId, config, ref stateValue, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool SetAnalogOutputState(EAnalogOutputConfiguration configuration, int stateValue)
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsSetAnalogOutputState(KeyHandle, NodeId, configuration, stateValue, ref lastErrorCode) > 0;
                
            LastErrorCode = lastErrorCode;

            return result;
        }
        
        public bool SetAnalogOutputVoltage(ushort outputNumber, int voltageValue)
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsSetAnalogOutputVoltage(KeyHandle, NodeId, outputNumber, voltageValue, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool GetAnalogInputVoltage(ushort inputNumber, out int voltageValue)
        {
            uint lastErrorCode = 0;

            voltageValue = 0;

            var result = VcsWrapper.Device.VcsGetAnalogInputVoltage(KeyHandle, NodeId, inputNumber, ref voltageValue, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool WaitForHomingAttained(uint timeout)
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsWaitForHomingAttained(KeyHandle, NodeId, timeout, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool ActivateHomingMode()
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsActivateHomingMode(KeyHandle, NodeId, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool DefinePosition(int homePosition)
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsDefinePosition(KeyHandle, NodeId, homePosition, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool FindHome(EHomingMethod method)
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsFindHome(KeyHandle, NodeId, method, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool ActivateVelocityMode()
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsActivateVelocityMode(KeyHandle, NodeId, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool GetHomingParameter(ref uint homingAcceleration, ref uint speedSwitch, ref uint speedIndex, ref int homeOffset, ref ushort currentThreshold, ref int homePosition)
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsGetHomingParameter(KeyHandle, NodeId, ref homingAcceleration, ref speedSwitch, ref speedIndex, ref homeOffset, ref currentThreshold, ref homePosition, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        public bool SetHomingParameter(uint homingAcceleration, uint speedSwitch, uint speedIndex, int homeOffset, ushort currentThreshold, int homePosition)
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsSetHomingParameter(KeyHandle, NodeId, homingAcceleration, speedSwitch, speedIndex, homeOffset, currentThreshold, homePosition, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }
        public bool StopHoming()
        {
            uint lastErrorCode = 0;

            var result = VcsWrapper.Device.VcsStopHoming(KeyHandle, NodeId, ref lastErrorCode) > 0;

            LastErrorCode = lastErrorCode;

            return result;
        }

        #endregion


    }
}
