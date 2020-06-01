using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using EltraMaster.Os.Interface;
using EltraMaster.Os.Linux;

namespace EposMaster.DeviceManager.VcsWrapper
{
    /// <summary>
    /// These are the EposCmd wrapper class methods for <c>EPOS Command library</c>.
    /// </summary>
    /// <example>
    /// <code>
    ///
    /// const ushort NodeId = 1;
    /// uint errorCode = 0;
    /// int keyHandle = 0;
    ///
    /// try
    /// {
    ///     // You have to call this method once to create bindings between EposCmd(64).dll and the wrapper.
    ///     // Use Cleanup method to release windows dll
    ///     Device.Init();
    ///
    ///     // Init/VcsOpenDevice(Dlg)/Cleanup should be called in the application main thread
    ///     keyHandle = Device.VcsOpenDeviceDlg(ref errorCode);
    ///
    ///     Device.VcsSetProtocolStackSettings(keyHandle, 1000000, 500, ref errorCode); }
    ///
    ///     // Clear errors
    ///     if (Device.VcsClearFault(keyHandle, NodeId, ref errorCode) == 0)
    ///     {
    ///         throw new Exception("VcsClearFault failed!");
    ///     }
    ///
    ///     // Disable
    ///     if (Device.VcsSetDisableState(keyHandle, NodeId, ref errorCode) == 0)
    ///     {
    ///         throw new Exception("VcsSetDisableState failed!");
    ///     }
    ///
    ///     // Enable
    ///     if (Device.VcsSetEnableState(keyHandle, NodeId, ref errorCode) == 0)
    ///     {
    ///         throw new Exception("VcsSetEnableState failed!");
    ///     }
    ///
    ///     // Activate profile velocity mode
    ///     if (Device.VcsActivateProfileVelocityMode(keyHandle, NodeId, ref errorCode) == 0)
    ///     {
    ///         throw new Exception("VcsActivateProfileVelocityMode failed!");
    ///     }
    ///
    ///     // Move
    ///     if (Device.VcsMoveWithVelocity(keyHandle, NodeId, 2000, ref errorCode) == 0)
    ///     {
    ///         throw new Exception("VcsMoveWithVelocity failed!");
    ///     }
    ///
    ///     int pos = 0;
    ///
    ///     // Get new position
    ///     if (Device.VcsGetPositionIs(keyHandle, NodeId, ref pos, ref errorCode) == 0)
    ///     {
    ///         throw new Exception("VcsMoveWithVelocity failed!");
    ///     }
    /// }
    /// catch (Exception e)
    /// {
    ///     Console.WriteLine(e.Message + errorCode);
    /// }
    /// finally
    /// {
    ///     if (keyHandle != 0)
    ///     {
    ///         Device.VcsCloseDevice(keyHandle, ref errorCode); }
    ///     }
    ///
    ///     Device.Cleanup();
    /// }
    ///
    ///
    /// </code>
    /// </example>
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute]
    // ReSharper disable UnusedMember.Global
    public class NamespaceDoc
    // ReSharper restore UnusedMember.Global
    {
    }

    /// <summary>
    /// EposCmd device wrapper class
    /// </summary>
    public static class Device
    {
        #region Const

        private const uint ErrorCrossThreadError = 0x10000024;

        #endregion

        #region Sync

        private static object _syncObject = new object(); 

        #endregion

        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate IntPtr InternalOpenDevice(
            [MarshalAs(UnmanagedType.LPStr)] string deviceName,
            [MarshalAs(UnmanagedType.LPStr)] string protocolStackName,
            [MarshalAs(UnmanagedType.LPStr)] string interfaceName,
            [MarshalAs(UnmanagedType.LPStr)] string portName,
            ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate /*unsafe*/ IntPtr InternalOpenDeviceDlg(ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate /*unsafe*/ int InternalCloseDevice(IntPtr keyHandle, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate /*unsafe*/ int InternalCloseAllDevices(ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate /*unsafe*/ int InternalSetProtocolStackSettings(
            IntPtr keyHandle, uint baudrate, uint timeout, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate /*unsafe*/ int InternalGetProtocolStackSettings(
            IntPtr keyHandle, ref uint baudrate, ref uint timeout, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate /*unsafe*/ int InternalFindDeviceCommunicationSettings(
            ref IntPtr keyHandle,
            StringBuilder deviceName,
            StringBuilder protocolStackName,
            StringBuilder interfaceName,
            StringBuilder portName,
            ushort sizeName,
            ref uint baudrate,
            ref uint timeout,
            ref ushort nodeId,
            int dialogMode,
            ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate IntPtr InternalOpenSubDevice(
            IntPtr deviceHandle,
            [MarshalAs(UnmanagedType.LPStr)] string deviceName,
            [MarshalAs(UnmanagedType.LPStr)] string protocolStackName,
            ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate /*unsafe*/ IntPtr InternalOpenSubDeviceDlg(IntPtr deviceHandle, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate /*unsafe*/ int InternalCloseSubDevice(IntPtr keyHandle, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate /*unsafe*/ int InternalCloseAllSubDevices(IntPtr deviceHandle, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate /*unsafe*/ int InternalSetGatewaySettings(
            IntPtr keyHandle, uint baudrate, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate /*unsafe*/ int InternalGetGatewaySettings(
            IntPtr keyHandle, ref uint baudrate, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate /*unsafe*/ int InternalFindSubDeviceCommunicationSettings(
            IntPtr deviceHandle,
            ref IntPtr keyHandle,
            StringBuilder deviceName,
            StringBuilder protocolStackName,
            ushort sizeName,
            ref uint baudrate,
            ref ushort nodeId,
            int dialogMode,
            ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetDriverInfo(
            StringBuilder libraryName,
            ushort maxNameSize,
            StringBuilder libraryVersion,
            ushort maxVersionSize,
            ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetVersion(
            IntPtr keyHandle,
            ushort nodeId,
            ref ushort hardwareVersion,
            ref ushort softwareVersion,
            ref ushort applicationNumber,
            ref ushort applicationVersion,
            ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int InternalGetDeviceNameSelection(
            int startOfSelection,
            void* deviceNameSel,
            ushort maxStrSize,
            ref int endOfSelection,
            ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetProtocolStackNameSelection(
            string deviceName,
            int startOfSelection,
            StringBuilder protocolStackNameSel,
            ushort maxStrSize,
            ref int endOfSelection,
            ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetInterfaceNameSelection(
            string deviceName,
            string protocolStackName,
            int startOfSelection,
            StringBuilder interfaceNameSel,
            ushort maxStrSize,
            ref int endOfSelection,
            ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetPortNameSelection(string deviceName, string protocolStackName, string interfaceName, int startOfSelection, StringBuilder portNameSel, ushort maxStrSize, ref int endOfSelection, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalResetPortNameSelection([MarshalAs(UnmanagedType.LPStr)] string deviceName, [MarshalAs(UnmanagedType.LPStr)] string protocolStackName, [MarshalAs(UnmanagedType.LPStr)] string interfaceName, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetBaudrateSelection(string deviceName, string protocolStackName, string interfaceName, string portName, int startOfSelection, ref uint baudrateSel, ref int endOfSelection, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetKeyHandle(string deviceName, string protocolStackName, string interfaceName, string portName, ref IntPtr keyHandle, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetDeviceName(IntPtr keyHandle, [MarshalAs(UnmanagedType.LPStr)] StringBuilder deviceName, ushort maxStrSize, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetProtocolStackName(IntPtr keyHandle, StringBuilder protocolStackName, ushort maxStrSize, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetInterfaceName(IntPtr keyHandle, [MarshalAs(UnmanagedType.LPStr)] StringBuilder interfaceName, ushort maxStrSize, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetPortName(IntPtr keyHandle, [MarshalAs(UnmanagedType.LPStr)] StringBuilder portName, ushort maxStrSize, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalImportParameter(IntPtr keyHandle, ushort nodeId, [MarshalAs(UnmanagedType.LPStr)] string parameterFileName, int showDlg, int showMsg, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalExportParameter(IntPtr keyHandle, ushort nodeId, [MarshalAs(UnmanagedType.LPStr)] string parameterFileName, [MarshalAs(UnmanagedType.LPStr)] string binaryFile, [MarshalAs(UnmanagedType.LPStr)] string userId, [MarshalAs(UnmanagedType.LPStr)] string comment, int showDlg, int showMsg, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetObject(IntPtr keyHandle, ushort nodeId, ushort objectIndex, byte objectSubindex, [MarshalAs(UnmanagedType.LPArray)] byte[] data, uint nbOfBytesToWrite, ref uint nbOfBytesWritten, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetObject(IntPtr keyHandle, ushort nodeId, ushort objectIndex, byte objectSubindex, [MarshalAs(UnmanagedType.LPArray)] byte[] data, uint nbOfBytesToRead, ref uint nbOfBytesRead, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalRestore(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalStore(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalUpdateFirmware(IntPtr keyHandle, ushort nodeId, [MarshalAs(UnmanagedType.LPStr)] string binaryFileName, int showDlg, int showHistory, int showMsg, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetMotorType(IntPtr keyHandle, ushort nodeId, ushort motorType, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetDcMotorParameter(
            IntPtr keyHandle,
            ushort nodeId,
            ushort nominalCurrent,
            ushort maxOutputCurrent,
            ushort thermalTimeConstant,
            ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetEcMotorParameter(
            IntPtr keyHandle,
            ushort nodeId,
            ushort nominalCurrent,
            ushort maxOutputCurrent,
            ushort thermalTimeConstant,
            byte nbOfPolePairs,
            ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetMotorType(IntPtr keyHandle, ushort nodeId, ref ushort motorType, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetDcMotorParameter(
            IntPtr keyHandle,
            ushort nodeId,
            ref ushort nominalCurrent,
            ref ushort maxOutputCurrent,
            ref ushort thermalTimeConstant,
            ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetEcMotorParameter(
            IntPtr keyHandle,
            ushort nodeId,
            ref ushort nominalCurrent,
            ref ushort maxOutputCurrent,
            ref ushort thermalTimeConstant,
            ref byte nbOfPolePairs,
            ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetSensorType(IntPtr keyHandle, ushort nodeId, ushort sensorType, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetIncEncoderParameter(
            IntPtr keyHandle, ushort nodeId, uint encoderResolution, int invertedPolarity, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetHallSensorParameter(
            IntPtr keyHandle, ushort nodeId, int invertedPolarity, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetSsiAbsEncoderParameter(
            IntPtr keyHandle,
            ushort nodeId,
            ushort dataRate,
            ushort nbOfMultiTurnDataBits,
            ushort nbOfSingleTurnDataBits,
            int invertedPolarity,
            ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetSsiAbsEncoderParameterEx(
            IntPtr keyHandle,
            ushort nodeId,
            ushort dataRate,
            ushort nbOfMultiTurnDataBits,
            ushort nbOfSingleTurnDataBits,
            ushort nbOfSpecialDataBits,
            int invertedPolarity,
            ushort timeout,
            ushort powerupTime,
            ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetSensorType(
            IntPtr keyHandle, ushort nodeId, ref ushort pSensorType, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetIncEncoderParameter(
            IntPtr keyHandle, ushort nodeId, ref uint encoderResolution, ref int invertedPolarity, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetHallSensorParameter(
            IntPtr keyHandle, ushort nodeId, ref int invertedPolarity, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetSsiAbsEncoderParameter(
            IntPtr keyHandle,
            ushort nodeId,
            ref ushort dataRate,
            ref ushort nbOfMultiTurnDataBits,
            ref ushort nbOfSingleTurnDataBits,
            ref int invertedPolarity,
            ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetSsiAbsEncoderParameterEx(
            IntPtr keyHandle,
            ushort nodeId,
            ref ushort dataRate,
            ref ushort nbOfMultiTurnDataBits,
            ref ushort nbOfSingleTurnDataBits,
            ref ushort mbOfSpecialDataBits,
            ref int invertedPolarity,
            ref ushort timeout,
            ref ushort powerupTime,
            ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetMaxFollowingError(
            IntPtr keyHandle, ushort nodeId, uint maxFollowingError, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetMaxFollowingError(
            IntPtr keyHandle, ushort nodeId, ref uint maxFollowingError, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetMaxAcceleration(
            IntPtr keyHandle, ushort nodeId, uint maxAcceleration, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetMaxAcceleration(
            IntPtr keyHandle, ushort nodeId, ref uint maxAcceleration, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetControllerGain(IntPtr keyHandle, ushort nodeId, ushort controller, ushort gain, ulong value, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetControllerGain(IntPtr keyHandle, ushort nodeId, ushort controller, ushort gain, ref ulong value, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetPositionRegulatorGain(IntPtr keyHandle, ushort nodeId, ushort p, ushort i, ushort d, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetPositionRegulatorFeedForward(
            IntPtr keyHandle, ushort nodeId, ushort velocityFeedForward, ushort accelerationFeedForward, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetPositionRegulatorGain(IntPtr keyHandle, ushort nodeId, ref ushort p, ref ushort i, ref ushort d, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetPositionRegulatorFeedForward(IntPtr keyHandle, ushort nodeId, ref ushort velocityFeedForward, ref ushort accelerationFeedForward, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetVelocityRegulatorGain(IntPtr keyHandle, ushort nodeId, ushort p, ushort i, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetVelocityRegulatorFeedForward(IntPtr keyHandle, ushort nodeId, ushort velocityFeedForward, ushort accelerationFeedForward, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetVelocityRegulatorGain(IntPtr keyHandle, ushort nodeId, ref ushort p, ref ushort i, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetVelocityRegulatorFeedForward(IntPtr keyHandle, ushort nodeId, ref ushort velocityFeedForward, ref ushort accelerationFeedForward, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetCurrentRegulatorGain(
            IntPtr keyHandle, ushort nodeId, ushort p, ushort i, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetCurrentRegulatorGain(
            IntPtr keyHandle, ushort nodeId, ref ushort p, ref ushort i, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalDigitalInputConfiguration(IntPtr keyHandle, ushort nodeId, ushort digitalInputNb, ushort configuration, int mask, int polarity, int executionMask, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalDigitalOutputConfiguration(IntPtr keyHandle, ushort nodeId, ushort digitalOutputNb, ushort configuration, int state, int mask, int polarity, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalAnalogInputConfiguration(IntPtr keyHandle, ushort nodeId, ushort analogInputNb, ushort configuration, int executionMask, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalAnalogOutputConfiguration(IntPtr keyHandle, ushort nodeId, ushort analogInputNb, ushort configuration, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetVelocityUnits(
            IntPtr keyHandle, ushort nodeId, byte velDimension, sbyte velNotation, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetVelocityUnits(
            IntPtr keyHandle, ushort nodeId, ref byte velDimension, ref sbyte velNotation, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetMotorParameter(IntPtr keyHandle, ushort nodeId, ushort motorType, ushort continuousCurrent, ushort peakCurrent, byte polePair, ushort thermalTimeConstant, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetEncoderParameter(IntPtr keyHandle, ushort nodeId, ushort counts, ushort positionSensorType, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetMotorParameter(IntPtr keyHandle, ushort nodeId, ref ushort motorType, ref ushort continuousCurrent, ref ushort peakCurrent, ref byte polePair, ref ushort thermalTimeConstant, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetEncoderParameter(IntPtr keyHandle, ushort nodeId, ref ushort counts, ref ushort positionSensorType, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetOperationMode(IntPtr keyHandle, ushort nodeId, sbyte operationMode, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetOperationMode(IntPtr keyHandle, ushort nodeId, ref sbyte operationMode, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalResetDevice(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetEnableState(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetDisableState(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetQuickStopState(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalClearFault(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetEnableState(IntPtr keyHandle, ushort nodeId, ref int isEnabled, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetDisableState(IntPtr keyHandle, ushort nodeId, ref int isDisabled, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetQuickStopState(IntPtr keyHandle, ushort nodeId, ref int isQuickStopped, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetFaultState(IntPtr keyHandle, ushort nodeId, ref int isInFault, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetNbOfDeviceError(IntPtr keyHandle, ushort nodeId, ref byte nbDeviceError, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetDeviceErrorCode(IntPtr keyHandle, ushort nodeId, byte subIndex, ref uint pDeviceErrorCode, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetErrorInfo(uint errorCodeValue, StringBuilder errofInfo, ushort maxStrSize);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetMovementState(IntPtr keyHandle, ushort nodeId, ref int targetReached, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetPositionIs(IntPtr keyHandle, ushort nodeId, ref int positionIs, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetVelocityIs(IntPtr keyHandle, ushort nodeId, ref int velocityIs, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetVelocityIsAveraged(IntPtr keyHandle, ushort nodeId, ref int velocityIsAveraged, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetCurrentIs(IntPtr keyHandle, ushort nodeId, ref short currentIs, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetCurrentIsAveraged(IntPtr keyHandle, ushort nodeId, ref short currentIsAveraged, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalActivateProfilePositionMode(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetPositionProfile(IntPtr keyHandle, ushort nodeId, uint profileVelocity, uint profileAcceleration, uint profileDeceleration, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetPositionProfile(IntPtr keyHandle, ushort nodeId, ref uint profileVelocity, ref uint profileAcceleration, ref uint profileDeceleration, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalMoveToPosition(IntPtr keyHandle, ushort nodeId, int targetPosition, int absolute, int immediately, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetTargetPosition(IntPtr keyHandle, ushort nodeId, ref int pTargetPosition, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalHaltPositionMovement(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalEnablePositionWindow(
            IntPtr keyHandle, ushort nodeId, uint positionWindow, ushort positionWindowTime, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalDisablePositionWindow(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalActivateProfileVelocityMode(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetVelocityProfile(
            IntPtr keyHandle, ushort nodeId, uint profileAcceleration, uint profileDeceleration, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetVelocityProfile(IntPtr keyHandle, ushort nodeId, ref uint pProfileAcceleration, ref uint pProfileDeceleration, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalMoveWithVelocity(IntPtr keyHandle, ushort nodeId, int targetVelocity, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetTargetVelocity(IntPtr keyHandle, ushort nodeId, ref int targetVelocity, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalHaltVelocityMovement(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalEnableVelocityWindow(IntPtr keyHandle, ushort nodeId, uint velocityWindow, ushort velocityWindowTime, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalDisableVelocityWindow(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalActivateHomingMode(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetHomingParameter(IntPtr keyHandle, ushort nodeId, uint homingAcceleration, uint speedSwitch, uint speedIndex, int homeOffset, ushort currentThreshold, int homePosition, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetHomingParameter(IntPtr keyHandle, ushort nodeId, ref uint homingAcceleration, ref uint speedSwitch, ref uint speedIndex, ref int homeOffset, ref ushort currentThreshold, ref int homePosition, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalFindHome(IntPtr keyHandle, ushort nodeId, sbyte homingMethod, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalStopHoming(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalDefinePosition(IntPtr keyHandle, ushort nodeId, int homePosition, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalWaitForHomingAttained(IntPtr keyHandle, ushort nodeId, uint timeout, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetHomingState(IntPtr keyHandle, ushort nodeId, ref int homingAttained, ref int homingError, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalActivateInterpolatedPositionMode(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetIpmBufferParameter(IntPtr keyHandle, ushort nodeId, ushort underflowWarningLimit, ushort overflowWarningLimit, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetIpmBufferParameter(IntPtr keyHandle, ushort nodeId, ref ushort pUnderflowWarningLimit, ref ushort pOverflowWarningLimit, ref uint pMaxBufferSize, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalClearIpmBuffer(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetFreeIpmBufferSize(
            IntPtr keyHandle, ushort nodeId, ref uint pBufferSize, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalAddPvtValueToIpmBuffer(IntPtr keyHandle, ushort nodeId, int position, int velocity, byte time, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalStartIpmTrajectory(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalStopIpmTrajectory(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetIpmStatus(IntPtr keyHandle, ushort nodeId, ref int trajectoryRunning, ref int isUnderflowWarning, ref int isOverflowWarning, ref int isVelocityWarning, ref int isAccelerationWarning, ref int isUnderflowError, ref int isOverflowError, ref int isVelocityError, ref int isAccelerationError, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalActivatePositionMode(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetPositionMust(IntPtr keyHandle, ushort nodeId, int positionMust, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetPositionMust(IntPtr keyHandle, ushort nodeId, ref int pPositionMust, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalActivateAnalogPositionSetpoint(IntPtr keyHandle, ushort nodeId, ushort analogInputNumber, float scaling, int offset, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalDeactivateAnalogPositionSetpoint(IntPtr keyHandle, ushort nodeId, ushort analogInputNumber, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalEnableAnalogPositionSetpoint(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalDisableAnalogPositionSetpoint(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalActivateVelocityMode(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetVelocityMust(IntPtr keyHandle, ushort nodeId, int velocityMust, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetVelocityMust(
            IntPtr keyHandle, ushort nodeId, ref int pVelocityMust, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalActivateAnalogVelocitySetpoint(IntPtr keyHandle, ushort nodeId, ushort analogInputNumber, float scaling, int offset, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalDeactivateAnalogVelocitySetpoint(IntPtr keyHandle, ushort nodeId, ushort analogInputNumber, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalEnableAnalogVelocitySetpoint(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalDisableAnalogVelocitySetpoint(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalActivateCurrentMode(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetCurrentMust(IntPtr keyHandle, ushort nodeId, short currentMust, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetCurrentMust(IntPtr keyHandle, ushort nodeId, ref short currentMust, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalActivateAnalogCurrentSetpoint(IntPtr keyHandle, ushort nodeId, ushort analogInputNumber, float scaling, short offset, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalDeactivateAnalogCurrentSetpoint(IntPtr keyHandle, ushort nodeId, ushort analogInputNumber, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalEnableAnalogCurrentSetpoint(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalDisableAnalogCurrentSetpoint(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalActivateMasterEncoderMode(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetMasterEncoderParameter(IntPtr keyHandle, ushort nodeId, ushort scalingNumerator, ushort scalingDenominator, byte polarity, uint maxVelocity, uint maxAcceleration, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetMasterEncoderParameter(IntPtr keyHandle, ushort nodeId, ref ushort scalingNumerator, ref ushort scalingDenominator, ref byte polarity, ref uint maxVelocity, ref uint maxAcceleration, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalActivateStepDirectionMode(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetStepDirectionParameter(IntPtr keyHandle, ushort nodeId, ushort scalingNumerator, ushort scalingDenominator, byte polarity, uint maxVelocity, uint maxAcceleration, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetStepDirectionParameter(IntPtr keyHandle, ushort nodeId, ref ushort pScalingNumerator, ref ushort pScalingDenominator, ref byte pPolarity, ref uint pMaxVelocity, ref uint pMaxAcceleration, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetAllDigitalInputs(IntPtr keyHandle, ushort nodeId, ref ushort pInputs, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetAllDigitalOutputs(IntPtr keyHandle, ushort nodeId, ref ushort pOutputs, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetAllDigitalOutputs(IntPtr keyHandle, ushort nodeId, ushort outputs, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetAnalogInput(IntPtr keyHandle, ushort nodeId, ushort inputNumber, ref ushort pAnalogValue, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetAnalogInputVoltage(IntPtr keyHandle, ushort nodeId, ushort inputNumber, ref int pVoltageValue, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetAnalogInputState(IntPtr keyHandle, ushort nodeId, ushort configuration, ref int pStateValue, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetAnalogOutput(IntPtr keyHandle, ushort nodeId, ushort outputNumber, ushort analogValue, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetAnalogOutputVoltage(IntPtr keyHandle, ushort nodeId, ushort outputNumber, int voltageValue, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetAnalogOutputState(IntPtr keyHandle, ushort nodeId, ushort configuration, int stateValue, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetPositionCompareParameter(IntPtr keyHandle, ushort nodeId, byte operationalMode, byte intervalMode, byte directionDependency, ushort intervalWidth, ushort intervalRepetitions, ushort pulseWidth, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetPositionCompareParameter(IntPtr keyHandle, ushort nodeId, ref byte pOperationalMode, ref byte pIntervalMode, ref byte pDirectionDependency, ref ushort pIntervalWidth, ref ushort pIntervalRepetitions, ref ushort pPulseWidth, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalActivatePositionCompare(IntPtr keyHandle, ushort nodeId, ushort digitalOutputNumber, int polarity, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalDeactivatePositionCompare(IntPtr keyHandle, ushort nodeId, ushort digitalOutputNumber, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalEnablePositionCompare(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalDisablePositionCompare(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetPositionCompareReferencePosition(IntPtr keyHandle, ushort nodeId, int referencePosition, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetPositionMarkerParameter(IntPtr keyHandle, ushort nodeId, byte positionMarkerEdgeType, byte positionMarkerMode, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetPositionMarkerParameter(IntPtr keyHandle, ushort nodeId, ref byte pPositionMarkerEdgeType, ref byte pPositionMarkerMode, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalActivatePositionMarker(IntPtr keyHandle, ushort nodeId, ushort digitalInputNumber, int polarity, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalDeactivatePositionMarker(IntPtr keyHandle, ushort nodeId, ushort digitalInputNumber, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalReadPositionMarkerCounter(IntPtr keyHandle, ushort nodeId, ref ushort pCount, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalReadPositionMarkerCapturedPosition(IntPtr keyHandle, ushort nodeId, ushort counterIndex, ref int pCapturedPosition, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalResetPositionMarkerCounter(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetRecorderParameter(IntPtr keyHandle, ushort nodeId, ushort samplingPeriod, ushort nbOfPrecedingSamples, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetRecorderParameter(IntPtr keyHandle, ushort nodeId, ref ushort pSamplingPeriod, ref ushort pNbOfPrecedingSamples, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalEnableTrigger(IntPtr keyHandle, ushort nodeId, byte triggerType, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalDisableAllTriggers(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalActivateChannel(IntPtr keyHandle, ushort nodeId, byte channelNumber, ushort objectIndex, byte objectSubIndex, byte objectSize, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalDeactivateAllChannels(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalStartRecorder(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalStopRecorder(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalForceTrigger(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalIsRecorderRunning(IntPtr keyHandle, ushort nodeId, ref int pRunning, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalIsRecorderTriggered(IntPtr keyHandle, ushort nodeId, ref int pTriggered, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalReadChannelVectorSize(IntPtr keyHandle, ushort nodeId, ref uint pVectorSize, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int InternalReadChannelDataVector(IntPtr keyHandle, ushort nodeId, byte channelNumber, void* pDataVector, uint vectorSize, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalShowChannelDataDlg(IntPtr keyHandle, ushort nodeId, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalExportChannelDataToFile(IntPtr keyHandle, ushort nodeId, string fileName, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int InternalReadDataBuffer(IntPtr keyHandle, ushort nodeId, void* pDataBuffer, uint bufferSizeToRead, ref uint pBufferSizeRead, ref ushort pVectorStartOffset, ref ushort pMaxNbOfSamples, ref ushort pNbOfRecordedSamples, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int InternalExtractChannelDataVector(IntPtr keyHandle, ushort nodeId, byte channelNumber, void* pDataBuffer, uint bufferSize, void* pDataVector, uint vectorSize, ushort vectorStartOffset, ushort maxNbOfSamples, ushort nbOfRecordedSamples, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSendCanFrame(IntPtr keyHandle, ushort cobId, ushort length, [MarshalAs(UnmanagedType.LPArray)] byte[] data, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalReadCanFrame(IntPtr keyHandle, ushort cobId, ushort length, [MarshalAs(UnmanagedType.LPArray)] byte[] data, uint timeout, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalRequestCanFrame(IntPtr keyHandle, ushort cobId, ushort length, [MarshalAs(UnmanagedType.LPArray)] byte[] data, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSendNmtService(IntPtr keyHandle, ushort nodeId, ushort commandSpecifier, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalWaitForTargetReached(IntPtr keyHandle, ushort nodeId, uint timeout, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetState(IntPtr keyHandle, ushort nodeId, uint state, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetState(IntPtr keyHandle, ushort nodeId, ref ushort state, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalSetMaxProfileVelocity(IntPtr keyHandle, ushort nodeId, uint maxProfileVelocity, ref uint errorCode);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int InternalGetMaxProfileVelocity(IntPtr keyHandle, ushort nodeId, ref uint maxProfileVelocity, ref uint errorCode);

        #endregion

        #region Private members

        private static int _referenceCount;
        private static int _creatorThreadId;
        private static IntPtr _eposCmdDll;
        private static IntPtr _addressOfOpenDevice;
        private static IntPtr _addressOfOpenDeviceDlg;
        private static IntPtr _addressOfCloseDevice;
        private static IntPtr _addressOfCloseAllDevices;
        private static IntPtr _addressOfSetProtocolStackSettings;
        private static IntPtr _addressOfGetProtocolStackSettings;
        private static IntPtr _addressOfFindDeviceCommunicationSettings;
        private static IntPtr _addressOfSetGatewaySettings;
        private static IntPtr _addressOfGetGatewaySettings;
        private static IntPtr _addressOfOpenSubDevice;
        private static IntPtr _addressOfOpenSubDeviceDlg;
        private static IntPtr _addressOfCloseSubDevice;
        private static IntPtr _addressOfCloseAllSubDevices;
        private static IntPtr _addressOfFindSubDeviceCommunicationSettings;
        private static IntPtr _addressOfGetDriverInfo;
        private static IntPtr _addressOfGetVersion;
        private static IntPtr _addressOfGetDeviceNameSelection;
        private static IntPtr _addressOfGetProtocolStackNameSelection;
        private static IntPtr _addressOfGetInterfaceNameSelection;
        private static IntPtr _addressOfGetPortNameSelection;
        private static IntPtr _addressOfResetPortNameSelection;
        private static IntPtr _addressOfGetBaudrateSelection;
        private static IntPtr _addressOfGetKeyHandle;
        private static IntPtr _addressOfGetDeviceName;
        private static IntPtr _addressOfGetProtocolStackName;
        private static IntPtr _addressOfGetInterfaceName;
        private static IntPtr _addressOfGetPortName;
        private static IntPtr _addressOfImportParameter;
        private static IntPtr _addressOfExportParameter;
        private static IntPtr _addressOfSetObject;
        private static IntPtr _addressOfGetObject;
        private static IntPtr _addressOfRestore;
        private static IntPtr _addressOfStore;
        private static IntPtr _addressOfUpdateFirmware;
        private static IntPtr _addressOfSetMotorType;
        private static IntPtr _addressOfSetDcMotorParameter;
        private static IntPtr _addressOfSetEcMotorParameter;
        private static IntPtr _addressOfGetMotorType;
        private static IntPtr _addressOfGetDcMotorParameter;
        private static IntPtr _addressOfGetEcMotorParameter;
        private static IntPtr _addressOfSetSensorType;
        private static IntPtr _addressOfSetIncEncoderParameter;
        private static IntPtr _addressOfSetHallSensorParameter;
        private static IntPtr _addressOfSetSsiAbsEncoderParameter;
        private static IntPtr _addressOfSetSsiAbsEncoderParameterEx;
        private static IntPtr _addressOfGetSensorType;
        private static IntPtr _addressOfGetIncEncoderParameter;
        private static IntPtr _addressOfGetHallSensorParameter;
        private static IntPtr _addressOfGetSsiAbsEncoderParameter;
        private static IntPtr _addressOfGetSsiAbsEncoderParameterEx;
        private static IntPtr _addressOfSetMaxFollowingError;
        private static IntPtr _addressOfGetMaxFollowingError;
        private static IntPtr _addressOfSetMaxAcceleration;
        private static IntPtr _addressOfGetMaxAcceleration;
        private static IntPtr _addressOfSetControllerGain;
        private static IntPtr _addressOfGetControllerGain;
        private static IntPtr _addressOfSetPositionRegulatorGain;
        private static IntPtr _addressOfSetPositionRegulatorFeedForward;
        private static IntPtr _addressOfGetPositionRegulatorGain;
        private static IntPtr _addressOfGetPositionRegulatorFeedForward;
        private static IntPtr _addressOfSetVelocityRegulatorGain;
        private static IntPtr _addressOfSetVelocityRegulatorFeedForward;
        private static IntPtr _addressOfGetVelocityRegulatorGain;
        private static IntPtr _addressOfGetVelocityRegulatorFeedForward;
        private static IntPtr _addressOfSetCurrentRegulatorGain;
        private static IntPtr _addressOfGetCurrentRegulatorGain;
        private static IntPtr _addressOfDigitalInputConfiguration;
        private static IntPtr _addressOfDigitalOutputConfiguration;
        private static IntPtr _addressOfAnalogInputConfiguration;
        private static IntPtr _addressOfAnalogOutputConfiguration;
        private static IntPtr _addressOfSetVelocityUnits;
        private static IntPtr _addressOfGetVelocityUnits;
        private static IntPtr _addressOfSetMotorParameter;
        private static IntPtr _addressOfSetEncoderParameter;
        private static IntPtr _addressOfGetMotorParameter;
        private static IntPtr _addressOfGetEncoderParameter;
        private static IntPtr _addressOfSetOperationMode;
        private static IntPtr _addressOfGetOperationMode;
        private static IntPtr _addressOfResetDevice;
        private static IntPtr _addressOfSetEnableState;
        private static IntPtr _addressOfSetDisableState;
        private static IntPtr _addressOfSetQuickStopState;
        private static IntPtr _addressOfClearFault;
        private static IntPtr _addressOfGetEnableState;
        private static IntPtr _addressOfGetDisableState;
        private static IntPtr _addressOfGetQuickStopState;
        private static IntPtr _addressOfGetFaultState;
        private static IntPtr _addressOfGetNbOfDeviceError;
        private static IntPtr _addressOfGetDeviceErrorCode;
        private static IntPtr _addressOfGetErrorInfo;
        private static IntPtr _addressOfGetMovementState;
        private static IntPtr _addressOfGetPositionIs;
        private static IntPtr _addressOfGetVelocityIs;
        private static IntPtr _addressOfGetVelocityIsAveraged;
        private static IntPtr _addressOfGetCurrentIs;
        private static IntPtr _addressOfGetCurrentIsAveraged;
        private static IntPtr _addressOfActivateProfilePositionMode;
        private static IntPtr _addressOfSetPositionProfile;
        private static IntPtr _addressOfGetPositionProfile;
        private static IntPtr _addressOfMoveToPosition;
        private static IntPtr _addressOfGetTargetPosition;
        private static IntPtr _addressOfHaltPositionMovement;
        private static IntPtr _addressOfEnablePositionWindow;
        private static IntPtr _addressOfDisablePositionWindow;
        private static IntPtr _addressOfActivateProfileVelocityMode;
        private static IntPtr _addressOfSetVelocityProfile;
        private static IntPtr _addressOfGetVelocityProfile;
        private static IntPtr _addressOfMoveWithVelocity;
        private static IntPtr _addressOfGetTargetVelocity;
        private static IntPtr _addressOfHaltVelocityMovement;
        private static IntPtr _addressOfEnableVelocityWindow;
        private static IntPtr _addressOfDisableVelocityWindow;
        private static IntPtr _addressOfActivateHomingMode;
        private static IntPtr _addressOfSetHomingParameter;
        private static IntPtr _addressOfGetHomingParameter;
        private static IntPtr _addressOfFindHome;
        private static IntPtr _addressOfStopHoming;
        private static IntPtr _addressOfDefinePosition;
        private static IntPtr _addressOfWaitForHomingAttained;
        private static IntPtr _addressOfGetHomingState;
        private static IntPtr _addressOfActivateInterpolatedPositionMode;
        private static IntPtr _addressOfSetIpmBufferParameter;
        private static IntPtr _addressOfGetIpmBufferParameter;
        private static IntPtr _addressOfClearIpmBuffer;
        private static IntPtr _addressOfGetFreeIpmBufferSize;
        private static IntPtr _addressOfAddPvtValueToIpmBuffer;
        private static IntPtr _addressOfStartIpmTrajectory;
        private static IntPtr _addressOfStopIpmTrajectory;
        private static IntPtr _addressOfGetIpmStatus;
        private static IntPtr _addressOfActivatePositionMode;
        private static IntPtr _addressOfSetPositionMust;
        private static IntPtr _addressOfGetPositionMust;
        private static IntPtr _addressOfActivateAnalogPositionSetpoint;
        private static IntPtr _addressOfDeactivateAnalogPositionSetpoint;
        private static IntPtr _addressOfDisableAnalogPositionSetpoint;
        private static IntPtr _addressOfActivateVelocityMode;
        private static IntPtr _addressOfSetVelocityMust;
        private static IntPtr _addressOfGetVelocityMust;
        private static IntPtr _addressOfActivateAnalogVelocitySetpoint;
        private static IntPtr _addressOfDeactivateAnalogVelocitySetpoint;
        private static IntPtr _addressOfEnableAnalogVelocitySetpoint;
        private static IntPtr _addressOfDisableAnalogVelocitySetpoint;
        private static IntPtr _addressOfActivateCurrentMode;
        private static IntPtr _addressOfSetCurrentMust;
        private static IntPtr _addressOfGetCurrentMust;
        private static IntPtr _addressOfActivateAnalogCurrentSetpoint;
        private static IntPtr _addressOfDeactivateAnalogCurrentSetpoint;
        private static IntPtr _addressOfEnableAnalogCurrentSetpoint;
        private static IntPtr _addressOfDisableAnalogCurrentSetpoint;
        private static IntPtr _addressOfActivateMasterEncoderMode;
        private static IntPtr _addressOfSetMasterEncoderParameter;
        private static IntPtr _addressOfGetMasterEncoderParameter;
        private static IntPtr _addressOfActivateStepDirectionMode;
        private static IntPtr _addressOfSetStepDirectionParameter;
        private static IntPtr _addressOfGetStepDirectionParameter;
        private static IntPtr _addressOfGetAllDigitalInputs;
        private static IntPtr _addressOfGetAllDigitalOutputs;
        private static IntPtr _addressOfSetAllDigitalOutputs;
        private static IntPtr _addressOfGetAnalogInput;
        private static IntPtr _addressOfGetAnalogInputVoltage;
        private static IntPtr _addressOfGetAnalogInputState;
        private static IntPtr _addressOfSetAnalogOutput;
        private static IntPtr _addressOfSetAnalogOutputVoltage;
        private static IntPtr _addressOfSetAnalogOutputState;
        private static IntPtr _addressOfSetPositionCompareParameter;
        private static IntPtr _addressOfGetPositionCompareParameter;
        private static IntPtr _addressOfActivatePositionCompare;
        private static IntPtr _addressOfDeactivatePositionCompare;
        private static IntPtr _addressOfEnablePositionCompare;
        private static IntPtr _addressOfDisablePositionCompare;
        private static IntPtr _addressOfSetPositionCompareReferencePosition;
        private static IntPtr _addressOfSetPositionMarkerParameter;
        private static IntPtr _addressOfGetPositionMarkerParameter;
        private static IntPtr _addressOfActivatePositionMarker;
        private static IntPtr _addressOfDeactivatePositionMarker;
        private static IntPtr _addressOfReadPositionMarkerCounter;
        private static IntPtr _addressOfReadPositionMarkerCapturedPosition;
        private static IntPtr _addressOfResetPositionMarkerCounter;
        private static IntPtr _addressOfSetRecorderParameter;
        private static IntPtr _addressOfGetRecorderParameter;
        private static IntPtr _addressOfEnableTrigger;
        private static IntPtr _addressOfDisableAllTriggers;
        private static IntPtr _addressOfActivateChannel;
        private static IntPtr _addressOfDeactivateAllChannels;
        private static IntPtr _addressOfStartRecorder;
        private static IntPtr _addressOfStopRecorder;
        private static IntPtr _addressOfForceTrigger;
        private static IntPtr _addressOfIsRecorderRunning;
        private static IntPtr _addressOfIsRecorderTriggered;
        private static IntPtr _addressOfReadChannelVectorSize;
        private static IntPtr _addressOfReadChannelDataVector;
        private static IntPtr _addressOfShowChannelDataDlg;
        private static IntPtr _addressOfExportChannelDataToFile;
        private static IntPtr _addressOfReadDataBuffer;
        private static IntPtr _addressOfExtractChannelDataVector;
        private static IntPtr _addressOfSendCanFrame;
        private static IntPtr _addressOfReadCanFrame;
        private static IntPtr _addressOfRequestCanFrame;
        private static IntPtr _addressOfSendNmtService;
        private static IntPtr _addressOfEnableAnalogPositionSetpoint;
        private static IntPtr _addressOfWaitForTargetReached;
        private static IntPtr _addressOfSetMaxProfileVelocity;
        private static IntPtr _addressOfGetMaxProfileVelocity;
        private static IntPtr _addressOfGetState;
        private static IntPtr _addressOfSetState;

        private static ISystemHelper _systemHelper;

        #endregion

        #region Initialization

        /// <summary>
        /// This method have to be called when we are going to use the EposCmd interface.
        /// Function "Init" should be called in the application main thread.
        /// </summary>
        /// <example>
        /// <span id="Example">
        /// Open device example:
        /// <code>
        /// using System;
        /// using EposCmd.Net.Vcs.Wrapper;
        ///
        /// namespace Test
        /// {
        ///     public static class Wrapper
        ///     {
        ///         public static void Sample()
        ///         {
        ///             uint errorCode = 0;
        ///             int keyHandle = 0;
        ///
        ///             // You have to call this method once to create bindings between EposCmd(64).dll and the wrapper
        ///             Device.Init();
        ///
        ///             // Init/VcsOpenDevice(Dlg)/Cleanup should be called in the application main thread
        ///             keyHandle = Device.VcsOpenDeviceDlg(ref errorCode);
        ///
        ///             Device.VcsCloseDevice(keyHandle);
        ///
        ///             // Use Cleanup method to release windows dll
        ///             Device.Cleanup();
        ///         }
        ///     }
        /// }
        /// </code>
        /// </span>
        /// </example>
        public static void Init()
        {
            if (IsLibraryLoaded())
            {
                IncrementReferenceCounter();
            }
            else
            {
                CreateSystemHelper();

                InitThreadingInfos();

                InitReferenceCounter();

                LoadLibraryFunctions();
            }
        }

        private static void InitThreadingInfos()
        {
            _creatorThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        private static void InitReferenceCounter()
        {
            _referenceCount = 1;
        }

        private static void IncrementReferenceCounter()
        {
            _referenceCount++;
        }

        private static bool IsLibraryLoaded()
        {
            return _eposCmdDll != IntPtr.Zero;
        }

        private static void LoadLibraryFunctions()
        {
            string dllFileName = "EposCmd.dll";

            if (_systemHelper.IsWindows())
            {   
                if (_systemHelper.Is64BitProcess())
                {
                    dllFileName = "EposCmd64.dll";
                }
            }
            else
            {
                dllFileName = "libEposCmd.so";
            }
            
            _eposCmdDll = _systemHelper.GetDllInstance(dllFileName);

            _addressOfOpenDevice = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_OpenDevice@20");
            _addressOfOpenDeviceDlg = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_OpenDeviceDlg@4");
            _addressOfCloseDevice = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_CloseDevice@8");
            _addressOfCloseAllDevices = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_CloseAllDevices@4");
            _addressOfSetProtocolStackSettings = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetProtocolStackSettings@16");
            _addressOfGetProtocolStackSettings = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetProtocolStackSettings@16");
            _addressOfFindDeviceCommunicationSettings = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_FindDeviceCommunicationSettings@44");
            _addressOfOpenSubDevice = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_OpenSubDevice@16");
            _addressOfOpenSubDeviceDlg = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_OpenSubDeviceDlg@8");
            _addressOfCloseSubDevice = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_CloseSubDevice@8");
            _addressOfCloseAllSubDevices = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_CloseAllSubDevices@8");
            _addressOfSetGatewaySettings = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetGatewaySettings@12");
            _addressOfGetGatewaySettings = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetGatewaySettings@12");
            _addressOfFindSubDeviceCommunicationSettings = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_FindSubDeviceCommunicationSettings@36");
            _addressOfGetDriverInfo = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetDriverInfo@20");
            _addressOfGetVersion = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetVersion@28");
            _addressOfGetDeviceNameSelection = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetDeviceNameSelection@20");
            _addressOfGetProtocolStackNameSelection = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetProtocolStackNameSelection@24");
            _addressOfGetInterfaceNameSelection = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetInterfaceNameSelection@28");
            _addressOfGetPortNameSelection = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetPortNameSelection@32");
            _addressOfResetPortNameSelection = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_ResetPortNameSelection@16");
            _addressOfGetBaudrateSelection = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetBaudrateSelection@32");
            _addressOfGetKeyHandle = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetKeyHandle@24");
            _addressOfGetDeviceName = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetDeviceName@16");
            _addressOfGetProtocolStackName = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetProtocolStackName@16");
            _addressOfGetInterfaceName = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetInterfaceName@16");
            _addressOfGetPortName = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetPortName@16");
            _addressOfImportParameter = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_ImportParameter@24");
            _addressOfExportParameter = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_ExportParameter@36");
            _addressOfSetObject = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetObject@32");
            _addressOfGetObject = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetObject@32");
            _addressOfRestore = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_Restore@12");
            _addressOfStore = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_Store@12");
            _addressOfUpdateFirmware = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_UpdateFirmware@28");
            _addressOfSetMotorType = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetMotorType@16");
            _addressOfSetDcMotorParameter = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetDcMotorParameter@24");
            _addressOfSetEcMotorParameter = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetEcMotorParameter@28");
            _addressOfGetMotorType = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetMotorType@16");
            _addressOfGetDcMotorParameter = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetDcMotorParameter@24");
            _addressOfGetEcMotorParameter = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetEcMotorParameter@28");
            _addressOfSetSensorType = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetSensorType@16");
            _addressOfSetIncEncoderParameter = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetIncEncoderParameter@20");
            _addressOfSetHallSensorParameter = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetHallSensorParameter@16");
            _addressOfSetSsiAbsEncoderParameter = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetSsiAbsEncoderParameter@28");
            _addressOfSetSsiAbsEncoderParameterEx = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetSsiAbsEncoderParameterEx@40");
            _addressOfGetSensorType = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetSensorType@16");
            _addressOfGetIncEncoderParameter = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetIncEncoderParameter@20");
            _addressOfGetHallSensorParameter = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetHallSensorParameter@16");
            _addressOfGetSsiAbsEncoderParameter = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetSsiAbsEncoderParameter@28");
            _addressOfGetSsiAbsEncoderParameterEx = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetSsiAbsEncoderParameterEx@40");
            _addressOfSetMaxFollowingError = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetMaxFollowingError@16");
            _addressOfGetMaxFollowingError = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetMaxFollowingError@16");
            _addressOfSetMaxAcceleration = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetMaxAcceleration@16");
            _addressOfGetMaxAcceleration = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetMaxAcceleration@16");
            _addressOfSetControllerGain = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetControllerGain@28");
            _addressOfGetControllerGain = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetControllerGain@24");
            _addressOfSetPositionRegulatorGain = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetPositionRegulatorGain@24");
            _addressOfSetPositionRegulatorFeedForward = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetPositionRegulatorFeedForward@20");
            _addressOfGetPositionRegulatorGain = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetPositionRegulatorGain@24");
            _addressOfGetPositionRegulatorFeedForward = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetPositionRegulatorFeedForward@20");
            _addressOfSetVelocityRegulatorGain = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetVelocityRegulatorGain@20");
            _addressOfSetVelocityRegulatorFeedForward = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetVelocityRegulatorFeedForward@20");
            _addressOfGetVelocityRegulatorGain = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetVelocityRegulatorGain@20");
            _addressOfGetVelocityRegulatorFeedForward = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetVelocityRegulatorFeedForward@20");
            _addressOfSetCurrentRegulatorGain = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetCurrentRegulatorGain@20");
            _addressOfGetCurrentRegulatorGain = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetCurrentRegulatorGain@20");
            _addressOfDigitalInputConfiguration = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_DigitalInputConfiguration@32");
            _addressOfDigitalOutputConfiguration = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_DigitalOutputConfiguration@32");
            _addressOfAnalogInputConfiguration = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_AnalogInputConfiguration@24");
            _addressOfAnalogOutputConfiguration = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_AnalogOutputConfiguration@20");
            _addressOfSetVelocityUnits = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetVelocityUnits@20");
            _addressOfGetVelocityUnits = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetVelocityUnits@20");
            _addressOfSetMotorParameter = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetMotorParameter@32");
            _addressOfSetEncoderParameter = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetEncoderParameter@20");
            _addressOfGetMotorParameter = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetMotorParameter@32");
            _addressOfGetEncoderParameter = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetEncoderParameter@20");
            _addressOfSetOperationMode = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetOperationMode@16");
            _addressOfGetOperationMode = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetOperationMode@16");
            _addressOfResetDevice = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_ResetDevice@12");
            _addressOfSetEnableState = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetEnableState@12");
            _addressOfSetDisableState = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetDisableState@12");
            _addressOfSetQuickStopState = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetQuickStopState@12");
            _addressOfClearFault = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_ClearFault@12");
            _addressOfGetEnableState = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetEnableState@16");
            _addressOfGetDisableState = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetDisableState@16");
            _addressOfGetQuickStopState = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetQuickStopState@16");
            _addressOfGetFaultState = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetFaultState@16");
            _addressOfGetNbOfDeviceError = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetNbOfDeviceError@16");
            _addressOfGetDeviceErrorCode = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetDeviceErrorCode@20");
            _addressOfGetErrorInfo = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetErrorInfo@12");
            _addressOfGetMovementState = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetMovementState@16");
            _addressOfGetPositionIs = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetPositionIs@16");
            _addressOfGetVelocityIs = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetVelocityIs@16");
            _addressOfGetVelocityIsAveraged = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetVelocityIsAveraged@16");
            _addressOfGetCurrentIs = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetCurrentIs@16");
            _addressOfGetCurrentIsAveraged = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetCurrentIsAveraged@16");
            _addressOfActivateProfilePositionMode = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_ActivateProfilePositionMode@12");
            _addressOfSetPositionProfile = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetPositionProfile@24");
            _addressOfGetPositionProfile = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetPositionProfile@24");
            _addressOfMoveToPosition = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_MoveToPosition@24");
            _addressOfGetTargetPosition = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetTargetPosition@16");
            _addressOfHaltPositionMovement = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_HaltPositionMovement@12");
            _addressOfEnablePositionWindow = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_EnablePositionWindow@20");
            _addressOfDisablePositionWindow = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_DisablePositionWindow@12");
            _addressOfActivateProfileVelocityMode = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_ActivateProfileVelocityMode@12");
            _addressOfSetVelocityProfile = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetVelocityProfile@20");
            _addressOfGetVelocityProfile = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetVelocityProfile@20");
            _addressOfMoveWithVelocity = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_MoveWithVelocity@16");
            _addressOfGetTargetVelocity = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetTargetVelocity@16");
            _addressOfHaltVelocityMovement = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_HaltVelocityMovement@12");
            _addressOfEnableVelocityWindow = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_EnableVelocityWindow@20");
            _addressOfDisableVelocityWindow = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_DisableVelocityWindow@12");
            _addressOfActivateHomingMode = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_ActivateHomingMode@12");
            _addressOfSetHomingParameter = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetHomingParameter@36");
            _addressOfGetHomingParameter = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetHomingParameter@36");
            _addressOfFindHome = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_FindHome@16");
            _addressOfStopHoming = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_StopHoming@12");
            _addressOfDefinePosition = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_DefinePosition@16");
            _addressOfWaitForHomingAttained = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_WaitForHomingAttained@16");
            _addressOfGetHomingState = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetHomingState@20");
            _addressOfActivateInterpolatedPositionMode = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_ActivateInterpolatedPositionMode@12");
            _addressOfSetIpmBufferParameter = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetIpmBufferParameter@20");
            _addressOfGetIpmBufferParameter = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetIpmBufferParameter@24");
            _addressOfClearIpmBuffer = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_ClearIpmBuffer@12");
            _addressOfGetFreeIpmBufferSize = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetFreeIpmBufferSize@16");
            _addressOfAddPvtValueToIpmBuffer = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_AddPvtValueToIpmBuffer@24");
            _addressOfStartIpmTrajectory = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_StartIpmTrajectory@12");
            _addressOfStopIpmTrajectory = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_StopIpmTrajectory@12");
            _addressOfGetIpmStatus = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetIpmStatus@48");
            _addressOfActivatePositionMode = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_ActivatePositionMode@12");
            _addressOfSetPositionMust = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetPositionMust@16");
            _addressOfGetPositionMust = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetPositionMust@16");
            _addressOfActivateAnalogPositionSetpoint = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_ActivateAnalogPositionSetpoint@24");
            _addressOfDeactivateAnalogPositionSetpoint = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_DeactivateAnalogPositionSetpoint@16");
            _addressOfDisableAnalogPositionSetpoint = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_DisableAnalogPositionSetpoint@12");
            _addressOfActivateVelocityMode = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_ActivateVelocityMode@12");
            _addressOfSetVelocityMust = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetVelocityMust@16");
            _addressOfGetVelocityMust = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetVelocityMust@16");
            _addressOfActivateAnalogVelocitySetpoint = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_ActivateAnalogVelocitySetpoint@24");
            _addressOfDeactivateAnalogVelocitySetpoint = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_DeactivateAnalogVelocitySetpoint@16");
            _addressOfEnableAnalogVelocitySetpoint = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_EnableAnalogVelocitySetpoint@12");
            _addressOfDisableAnalogVelocitySetpoint = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_DisableAnalogVelocitySetpoint@12");
            _addressOfActivateCurrentMode = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_ActivateCurrentMode@12");
            _addressOfSetCurrentMust = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetCurrentMust@16");
            _addressOfGetCurrentMust = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetCurrentMust@16");
            _addressOfActivateAnalogCurrentSetpoint = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_ActivateAnalogCurrentSetpoint@24");
            _addressOfDeactivateAnalogCurrentSetpoint = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_DeactivateAnalogCurrentSetpoint@16");
            _addressOfEnableAnalogCurrentSetpoint = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_EnableAnalogCurrentSetpoint@12");
            _addressOfDisableAnalogCurrentSetpoint = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_DisableAnalogCurrentSetpoint@12");
            _addressOfActivateMasterEncoderMode = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_ActivateMasterEncoderMode@12");
            _addressOfSetMasterEncoderParameter = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetMasterEncoderParameter@32");
            _addressOfGetMasterEncoderParameter = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetMasterEncoderParameter@32");
            _addressOfActivateStepDirectionMode = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_ActivateStepDirectionMode@12");
            _addressOfSetStepDirectionParameter = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetStepDirectionParameter@32");
            _addressOfGetStepDirectionParameter = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetStepDirectionParameter@32");
            _addressOfGetAllDigitalInputs = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetAllDigitalInputs@16");
            _addressOfGetAllDigitalOutputs = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetAllDigitalOutputs@16");
            _addressOfSetAllDigitalOutputs = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetAllDigitalOutputs@16");
            _addressOfGetAnalogInput = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetAnalogInput@20");
            _addressOfGetAnalogInputVoltage = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetAnalogInputVoltage@20");
            _addressOfGetAnalogInputState = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetAnalogInputState@20");
            _addressOfSetAnalogOutput = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetAnalogOutput@20");
            _addressOfSetAnalogOutputVoltage = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetAnalogOutputVoltage@20");
            _addressOfSetAnalogOutputState = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetAnalogOutputState@20");
            _addressOfSetPositionCompareParameter = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetPositionCompareParameter@36");
            _addressOfGetPositionCompareParameter = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetPositionCompareParameter@36");
            _addressOfActivatePositionCompare = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_ActivatePositionCompare@20");
            _addressOfDeactivatePositionCompare = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_DeactivatePositionCompare@16");
            _addressOfEnablePositionCompare = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_EnablePositionCompare@12");
            _addressOfDisablePositionCompare = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_DisablePositionCompare@12");
            _addressOfSetPositionCompareReferencePosition = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetPositionCompareReferencePosition@16");
            _addressOfSetPositionMarkerParameter = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetPositionMarkerParameter@20");
            _addressOfGetPositionMarkerParameter = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetPositionMarkerParameter@20");
            _addressOfActivatePositionMarker = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_ActivatePositionMarker@20");
            _addressOfDeactivatePositionMarker = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_DeactivatePositionMarker@16");
            _addressOfReadPositionMarkerCounter = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_ReadPositionMarkerCounter@16");
            _addressOfReadPositionMarkerCapturedPosition = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_ReadPositionMarkerCapturedPosition@20");
            _addressOfResetPositionMarkerCounter = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_ResetPositionMarkerCounter@12");
            _addressOfSetRecorderParameter = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetRecorderParameter@20");
            _addressOfGetRecorderParameter = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetRecorderParameter@20");
            _addressOfEnableTrigger = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_EnableTrigger@16");
            _addressOfDisableAllTriggers = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_DisableAllTriggers@12");
            _addressOfActivateChannel = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_ActivateChannel@28");
            _addressOfDeactivateAllChannels = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_DeactivateAllChannels@12");
            _addressOfStartRecorder = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_StartRecorder@12");
            _addressOfStopRecorder = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_StopRecorder@12");
            _addressOfForceTrigger = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_ForceTrigger@12");
            _addressOfIsRecorderRunning = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_IsRecorderRunning@16");
            _addressOfIsRecorderTriggered = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_IsRecorderTriggered@16");
            _addressOfReadChannelVectorSize = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_ReadChannelVectorSize@16");
            _addressOfReadChannelDataVector = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_ReadChannelDataVector@24");
            _addressOfShowChannelDataDlg = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_ShowChannelDataDlg@12");
            _addressOfExportChannelDataToFile = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_ExportChannelDataToFile@16");
            _addressOfReadDataBuffer = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_ReadDataBuffer@36");
            _addressOfExtractChannelDataVector = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_ExtractChannelDataVector@44");
            _addressOfSendCanFrame = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SendCANFrame@20");
            _addressOfReadCanFrame = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_ReadCANFrame@24");
            _addressOfRequestCanFrame = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_RequestCANFrame@20");
            _addressOfSendNmtService = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SendNMTService@16");
            _addressOfEnableAnalogPositionSetpoint = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_EnableAnalogPositionSetpoint@12");
            _addressOfWaitForTargetReached = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_WaitForTargetReached@16");
            _addressOfSetMaxProfileVelocity = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetMaxProfileVelocity@16");
            _addressOfGetMaxProfileVelocity = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetMaxProfileVelocity@16");
            _addressOfGetState = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_GetState@16");
            _addressOfSetState = _systemHelper.GetProcAddress(_eposCmdDll, "_VCS_SetState@16");
        }

        private static bool IsLinuxOs()
        {
            bool result;
            OperatingSystem os = Environment.OSVersion;
            PlatformID pid = os.Platform;

            switch (pid)
            {
                case PlatformID.Unix:
                    result = true;
                    break;

                default:
                    result = false;
                    break;
            }

            return result;
        }

        private static void CreateSystemHelper()
        {
            if (IsLinuxOs())
            {
                _systemHelper = new SystemHelper();
            }
            else
            {
                _systemHelper = new global::EltraMaster.Os.Windows.SystemHelper();
            }
        }

        /// <summary>
        /// Method used to free by the EposCmd allocated resources.
        /// Function "Cleanup" should be called in the application main thread
        /// </summary>
        /// <example>
        /// <span id="Example">
        /// Open device example:
        /// <code>
        /// using System;
        /// using EposCmd.Net.Vcs.Wrapper;
        ///
        /// namespace Test
        /// {
        ///     public static class Wrapper
        ///     {
        ///         public static void Sample()
        ///         {
        ///             uint errorCode = 0;
        ///             int keyHandle = 0;
        ///
        ///             Device.Init();
        ///
        ///             // Init/VcsOpenDevice(Dlg)/Cleanup should be called in the application main thread
        ///             keyHandle = Device.VcsOpenDeviceDlg(ref errorCode);
        ///
        ///             Device.VcsCloseDevice(keyHandle);
        ///
        ///             Device.Cleanup();
        ///         }
        ///     }
        /// }
        /// </code>
        /// </span>
        /// </example>
        public static void Cleanup()
        {
            _referenceCount--;

            if (_eposCmdDll != IntPtr.Zero)
            {
                if (_creatorThreadId == Thread.CurrentThread.ManagedThreadId && _referenceCount == 0)
                {
                    _systemHelper.FreeLibrary(_eposCmdDll);
                    _eposCmdDll = IntPtr.Zero;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// VcsOpenDevice opens the port for sending and receiving commands. This function
        /// opens interfaces with the RS232, the USB and with CANopen boards.</summary>
        /// For correct designations on DeviceName, ProtocolStackName, InterfaceName and PortName
        /// use the functions GetDeviceNameSelection , GetProtocolStackNameSelection, GetInterfaceNameSelection and GetPortNameSelection.
        /// Function "VcsOpenDevice" should be called in the application main thread.
        /// <param name="deviceName">Device name</param>
        /// <param name="protocolStackName">Protocol stack name</param>
        /// <param name="interfaceName">Interface name</param>
        /// <param name="portName">Port name</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Handle for communication port access. Nonzero if successful; otherwise “0”.</returns>
        /// <example>
        /// <span id="Example">
        /// Open device example:
        /// <code>
        /// using System;
        /// using EposCmd.Net.Vcs.Wrapper;
        ///
        /// namespace Test
        /// {
        ///     public static class Wrapper
        ///     {
        ///         public static void Sample()
        ///         {
        ///             uint errorCode = 0;
        ///             int keyHandle = 0;
        ///
        ///             // Init/VcsOpenDevice(Dlg)/Cleanup should be called in the application main thread
        ///             Device.Init();
        ///
        ///             keyHandle = Device.VcsOpenDevice("EPOS","MAXON_RS232","RS232","COM1",ref errorCode);
        ///
        ///             Device.VcsCloseDevice(keyHandle);
        ///
        ///             Device.Cleanup();
        ///         }
        ///     }
        /// }
        /// </code>
        /// </span>
        /// </example>
        public static int VcsOpenDevice([MarshalAs(UnmanagedType.LPStr)] string deviceName, [MarshalAs(UnmanagedType.LPStr)] string protocolStackName, [MarshalAs(UnmanagedType.LPStr)] string interfaceName, [MarshalAs(UnmanagedType.LPStr)] string portName, ref uint errorCode)
        {
            int result = 0; if (_addressOfOpenDevice == IntPtr.Zero)
            {
                throw new Exception("Function VCS_OpenDevice not supported");
            }

            InternalOpenDevice func = (InternalOpenDevice)Marshal.GetDelegateForFunctionPointer(_addressOfOpenDevice, typeof(InternalOpenDevice));

            lock(_syncObject)
            {
                result = func(deviceName, protocolStackName, interfaceName, portName, ref errorCode).ToInt32();
            }

            return result;
        }

        /// <summary>
        /// VcsOpenDeviceDlg recognizes available interfaces capable to operate with EPOS and opens the selected interface for communication.
        /// VcsOpenDeviceDlg should be called in the application main thread.
        /// </summary>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Handle for port access. Nonzero if successful; otherwise “0”.</returns>
        /// <example>
        /// <span id="Example">
        /// Open device dialog example:
        /// <code>
        /// using System;
        /// using EposCmd.Net.Vcs.Wrapper;
        ///
        /// namespace Test
        /// {
        ///     public static class Wrapper
        ///     {
        ///         public static void Sample()
        ///         {
        ///             uint errorCode = 0;
        ///             int keyHandle = 0;
        ///
        ///             // Init/VcsOpenDevice(Dlg)/Cleanup should be called in the application main thread
        ///             Device.Init();
        ///
        ///             keyHandle = Device.VcsOpenDeviceDlg(ref errorCode);
        ///
        ///             Device.VcsCloseDevice(keyHandle);
        ///
        ///             Device.Cleanup();
        ///         }
        ///     }
        /// }
        /// </code>
        /// </span>
        /// </example>
        public static /*unsafe*/ int VcsOpenDeviceDlg(ref uint errorCode)
        {
            int result = 0; if (_addressOfOpenDeviceDlg == IntPtr.Zero)
            {
                throw new Exception("Function VCS_OpenDeviceDlg not supported");
            }

            InternalOpenDeviceDlg func = (InternalOpenDeviceDlg)Marshal.GetDelegateForFunctionPointer(_addressOfOpenDeviceDlg, typeof(InternalOpenDeviceDlg));

            lock(_syncObject)
            {
                result = func(ref errorCode).ToInt32();
            }

            return result;
        }

        /// <summary>
        /// VcsCloseDevice closes the port and releases it for other applications.
        /// Function "VcsCloseDevice" should be called in the application main thread.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        /// <example>
        /// <span id="Example">
        /// Open device dialog example:
        /// <code>
        /// using System;
        /// using EposCmd.Net.Vcs.Wrapper;
        ///
        /// namespace Test
        /// {
        ///     public static class Wrapper
        ///     {
        ///         public static void Sample()
        ///         {
        ///             uint errorCode = 0;
        ///             int keyHandle = 0;
        ///
        ///             // Init/VcsOpenDevice(Dlg)/Cleanup should be called in the application main thread
        ///             Device.Init();
        ///
        ///             keyHandle = Device.VcsOpenDeviceDlg(ref errorCode);
        ///
        ///             Device.VcsCloseDevice(keyHandle);
        ///
        ///             Device.Cleanup();
        ///         }
        ///     }
        /// }
        /// </code>
        /// </span>
        /// </example>
        public static /*unsafe*/ int VcsCloseDevice(int keyHandle, ref uint errorCode)
        {
            int result = 0;
            int threadId = Thread.CurrentThread.ManagedThreadId;

            if (_creatorThreadId == threadId)
            {
                if (_addressOfCloseDevice == IntPtr.Zero)
                {
                    throw new Exception("Function VCS_CloseDevice not supported");
                }

                InternalCloseDevice func = (InternalCloseDevice)Marshal.GetDelegateForFunctionPointer(_addressOfCloseDevice, typeof(InternalCloseDevice));

                lock (_syncObject) 
                { 
                    result = func(new IntPtr(keyHandle), ref errorCode); 
                }
            }

            errorCode = ErrorCrossThreadError;
            return result;
        }

        /// <summary>
        /// VcsCloseAllDevices closes all opened ports and releases it for other applications.
        /// Function "VcsCloseAllDevices" should be called in the application main thread.
        /// </summary>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        /// <example>
        /// <span id="Example">
        /// Open device dialog example:
        /// <code>
        /// using System;
        /// using EposCmd.Net.Vcs.Wrapper;
        ///
        /// namespace Test
        /// {
        ///     public static class Wrapper
        ///     {
        ///         public static void Sample()
        ///         {
        ///             uint errorCode = 0;
        ///             int keyHandle = 0;
        ///
        ///             // Init/VcsOpenDevice(Dlg)/Cleanup should be called in the application main thread
        ///             Device.Init();
        ///
        ///             keyHandle = Device.VcsOpenDeviceDlg(ref errorCode);
        ///
        ///             Device.VcsCloseAllDevices(keyHandle);
        ///
        ///             Device.Cleanup();
        ///         }
        ///     }
        /// }
        /// </code>
        /// </span>
        /// </example>
        public static /*unsafe*/ int VcsCloseAllDevices(ref uint errorCode)
        {
            int result = 0; if (_addressOfCloseAllDevices == IntPtr.Zero)
            {
                throw new Exception("Function VCS_CloseAllDevices not supported");
            }

            InternalCloseAllDevices func = (InternalCloseAllDevices)Marshal.GetDelegateForFunctionPointer(_addressOfCloseAllDevices, typeof(InternalCloseAllDevices));
            
            lock(_syncObject)
            { 
                result = func(ref errorCode);
            } 
            
            return result;
        }

        /// <summary>
        /// VcsSetProtocolStackSettings writes the communication parameters. For exact values on available baud rates, use function VcsGetBaudRateSelection. For correct communication, use the same baud rate as the connected device.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="baudrate">Actual baud rate from opened port [Bit/s]</param>
        /// <param name="timeout">Actual timeout from opened port [ms]</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static /*unsafe*/ int VcsSetProtocolStackSettings(int keyHandle, uint baudrate, uint timeout, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetProtocolStackSettings == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetProtocolStackSettings not supported");
            }

            InternalSetProtocolStackSettings func = (InternalSetProtocolStackSettings)Marshal.GetDelegateForFunctionPointer(_addressOfSetProtocolStackSettings, typeof(InternalSetProtocolStackSettings));
            
            lock (_syncObject) 
            { 
                result = func(new IntPtr(keyHandle), baudrate, timeout, ref errorCode);
            }

            return result;
        }

        /// <summary>
        /// VcsGetProtocolStackSettings returns the baud rate and timeout communication parameters.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="baudrate">Actual baud rate from opened port [Bit/s]</param>
        /// <param name="timeout">Actual timeout from opened port [ms]</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static /*unsafe*/ int VcsGetProtocolStackSettings(int keyHandle, ref uint baudrate, ref uint timeout, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetProtocolStackSettings == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetProtocolStackSettings not supported");
            }

            InternalGetProtocolStackSettings func = (InternalGetProtocolStackSettings)Marshal.GetDelegateForFunctionPointer(_addressOfGetProtocolStackSettings, typeof(InternalGetProtocolStackSettings));
            
            lock (_syncObject) 
            { 
                result = func(new IntPtr(keyHandle), ref baudrate, ref timeout, ref errorCode);
            }

            return result;
        }

        /// <summary>
        /// VcsFindDeviceCommunicationSettings searches the communication setting parameters. Parameters can be defined to accelerate the process. The search will be terminated as the first device is found.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="deviceName">Device name</param>
        /// <param name="protocolStackName">Protocol stack name</param>
        /// <param name="interfaceName">Interface name</param>
        /// <param name="portName">Port name</param>
        /// <param name="sizeName">Reserved memory size for return parameters</param>
        /// <param name="baudrate">The baudrate.</param>
        /// <param name="timeout">Actual timeout from opened port [ms]</param>
        /// <param name="nodeId">Node ID</param>
        /// <param name="dialogMode">Dialog Mode</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static /*unsafe*/ int VcsFindDeviceCommunicationSettings(
            ref int keyHandle,
            ref string deviceName,
            ref string protocolStackName,
            ref string interfaceName,
            ref string portName,
            ref ushort sizeName,
            ref uint baudrate,
            ref uint timeout,
            ref ushort nodeId,
            EDialogMode dialogMode,
            ref uint errorCode)
        {
            if (_addressOfFindDeviceCommunicationSettings == IntPtr.Zero)
            {
                throw new Exception("Function VCS_FindDeviceCommunicationSettings not supported");
            }

            InternalFindDeviceCommunicationSettings func =
                (InternalFindDeviceCommunicationSettings)
                    Marshal.GetDelegateForFunctionPointer(_addressOfFindDeviceCommunicationSettings, typeof(InternalFindDeviceCommunicationSettings));

            StringBuilder deviceNameCopy = new StringBuilder(sizeName);
            deviceNameCopy.Append(deviceName);
            StringBuilder protocolStackNameCopy = new StringBuilder(sizeName);
            protocolStackNameCopy.Append(protocolStackName);
            StringBuilder interfaceNameCopy = new StringBuilder(sizeName);
            interfaceNameCopy.Append(interfaceName);
            StringBuilder portNameCopy = new StringBuilder(sizeName);
            portNameCopy.Append(portName);
            IntPtr ptrHandle = IntPtr.Zero;
            int result = 0;

            try
            {
                result = func(
                    ref ptrHandle,
                    deviceNameCopy,
                    protocolStackNameCopy,
                    interfaceNameCopy,
                    portNameCopy,
                    sizeName,
                    ref baudrate,
                    ref timeout,
                    ref nodeId,
                    (int)dialogMode,
                    ref errorCode);

                if (result != 0)
                {
                    keyHandle = ptrHandle.ToInt32();
                    deviceName = deviceNameCopy.ToString();
                    protocolStackName = protocolStackNameCopy.ToString();
                    interfaceName = interfaceNameCopy.ToString();
                    portName = portNameCopy.ToString();
                }
            }
            catch (ThreadAbortException)
            {
            }

            return result;
        }

        /// <summary>
        /// VcsOpenSubDevice opens the subdevice connected to gateway device to send and receive commands.
        /// </summary>
        /// <param name="deviceHandle">Handle from opened device.</param>
        /// <param name="deviceName">Name of the subdevice.</param>
        /// <param name="protocolStackName">Name of the communication protocol.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Handle for gateway port access.
        /// Nonzero if successful; otherwise “0”.</returns>
        /// <exception cref="Exception">Function VcsOpenSubDevice not supported</exception>
        public static int VcsOpenSubDevice(int deviceHandle, [MarshalAs(UnmanagedType.LPStr)] string deviceName, [MarshalAs(UnmanagedType.LPStr)] string protocolStackName, ref uint errorCode)
        {
            int result = 0;

            if (_addressOfOpenSubDevice == IntPtr.Zero)
            {
                throw new Exception("Function VCS_OpenSubDevice not supported");
            }

            InternalOpenSubDevice func = (InternalOpenSubDevice)Marshal.GetDelegateForFunctionPointer(_addressOfOpenSubDevice, typeof(InternalOpenSubDevice));

            lock (_syncObject) 
            { 
                result = func(new IntPtr(deviceHandle), deviceName, protocolStackName, ref errorCode).ToInt32();
            }

            return result;
        }

        /// <summary>
        /// VcsOpenSubDeviceDlg recognizes available subdevices capable to operate with gateway device and opens the selected device for communication.
        /// </summary>
        /// <param name="deviceHandle">Handle from opened device.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Handle for gateway port access.
        /// Nonzero if successful; otherwise “0”.</returns>
        /// <exception cref="Exception">Function VcsOpenSubDeviceDlg not supported</exception>
        public static /*unsafe*/ int VcsOpenSubDeviceDlg(int deviceHandle, ref uint errorCode)
        {
            int result = 0;

            if (_addressOfOpenSubDeviceDlg == IntPtr.Zero)
            {
                throw new Exception("Function VCS_OpenSubDeviceDlg not supported");
            }

            InternalOpenSubDeviceDlg func = (InternalOpenSubDeviceDlg)Marshal.GetDelegateForFunctionPointer(_addressOfOpenSubDeviceDlg, typeof(InternalOpenSubDeviceDlg));

            lock (_syncObject) 
            { 
                result = func(new IntPtr(deviceHandle), ref errorCode).ToInt32();
            }

            return result;
        }

        /// <summary>
        /// VcsCloseSubDevice closes the subdevice and releases it for other applications.
        /// </summary>
        /// <param name="keyHandle">The key handle.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        /// <exception cref="Exception">Function VcsCloseSubDevice not supported</exception>
        public static /*unsafe*/ int VcsCloseSubDevice(int keyHandle, ref uint errorCode)
        {
            int result = 0;
            int threadId = Thread.CurrentThread.ManagedThreadId;

            if (_creatorThreadId == threadId)
            {
                if (_addressOfCloseSubDevice == IntPtr.Zero)
                {
                    throw new Exception("Function VCS_CloseSubDevice not supported");
                }

                InternalCloseSubDevice func = (InternalCloseSubDevice)Marshal.GetDelegateForFunctionPointer(_addressOfCloseSubDevice, typeof(InternalCloseSubDevice));

                lock (_syncObject) 
                { 
                    result = func(new IntPtr(keyHandle), ref errorCode);
                }
            }
            else
            {
                errorCode = ErrorCrossThreadError;
            }
            
            return result;
        }

        /// <summary>
        /// VcsCloseAllSubDevices closes all opened subdevices and releases them for other applications.
        /// </summary>
        /// <param name="deviceHandle">The device handle.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        /// <exception cref="Exception">Function VcsCloseAllSubDevices not supported</exception>
        public static /*unsafe*/ int VcsCloseAllSubDevices(int deviceHandle, ref uint errorCode)
        {
            int result = 0;

            if (_addressOfCloseAllSubDevices == IntPtr.Zero)
            {
                throw new Exception("Function VCS_CloseAllSubDevices not supported");
            }

            InternalCloseAllSubDevices func = (InternalCloseAllSubDevices)Marshal.GetDelegateForFunctionPointer(_addressOfCloseAllSubDevices, typeof(InternalCloseAllSubDevices));

            lock (_syncObject) 
            { 
                result = func(new IntPtr(deviceHandle), ref errorCode);
            }

            return result;
        }

        /// <summary>
        /// VcsSetGatewaySettings writes the gateway communication parameters. For correct communication, use the same baud rate as the connected device.
        /// </summary>
        /// <param name="keyHandle">The key handle.</param>
        /// <param name="baudrate">The baudrate.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        /// <exception cref="Exception">Function VcsSetGatewaySettings not supported</exception>
        public static /*unsafe*/ int VcsSetGatewaySettings(int keyHandle, uint baudrate, ref uint errorCode)
        {
            int result = 0;

            if (_addressOfSetGatewaySettings == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetGatewaySettings not supported");
            }

            InternalSetGatewaySettings func = (InternalSetGatewaySettings)Marshal.GetDelegateForFunctionPointer(_addressOfSetGatewaySettings, typeof(InternalSetGatewaySettings));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), baudrate, ref errorCode); }

            return result;
        }

        /// <summary>
        /// VcsGetGatewaySettings returns the baud rate and timeout gateway communication parameters.
        /// </summary>
        /// <param name="keyHandle">The key handle.</param>
        /// <param name="baudrate">The baudrate.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        /// <exception cref="Exception">Function VcsGetGatewaySettings not supported</exception>
        public static /*unsafe*/ int VcsGetGatewaySettings(int keyHandle, ref uint baudrate, ref uint errorCode)
        {
            int result = 0;

            if (_addressOfGetGatewaySettings == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetGatewaySettings not supported");
            }

            InternalGetGatewaySettings func = (InternalGetGatewaySettings)Marshal.GetDelegateForFunctionPointer(_addressOfGetGatewaySettings, typeof(InternalGetGatewaySettings));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), ref baudrate, ref errorCode); }
        
            return result;
        }

        /// <summary>
        /// VcsFindSubDeviceCommunicationSettings searches the subdevice communication setting parameters. Parameters can be defined to accelerate the process. The search will be terminated as the first device is found.
        /// </summary>
        /// <param name="deviceHandle">Handle from opened device.</param>
        /// <param name="keyHandle">The key handle.</param>
        /// <param name="deviceName">Name of the device.</param>
        /// <param name="protocolStackName">Name of the protocol stack.</param>
        /// <param name="sizeName">Name of the size.</param>
        /// <param name="baudrate">The baudrate.</param>
        /// <param name="nodeId">The node identifier.</param>
        /// <param name="dialogMode">The dialog mode.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        /// <exception cref="Exception">Function VcsFindSubDeviceCommunicationSettings not supported</exception>
        public static /*unsafe*/ int VcsFindSubDeviceCommunicationSettings(int deviceHandle, ref int keyHandle, ref string deviceName, ref string protocolStackName, ushort sizeName, ref uint baudrate, ref ushort nodeId, EDialogMode dialogMode, ref uint errorCode)
        {
            if (_addressOfFindSubDeviceCommunicationSettings == IntPtr.Zero)
            {
                throw new Exception("Function VCS_FindSubDeviceCommunicationSettings not supported");
            }

            InternalFindSubDeviceCommunicationSettings func = (InternalFindSubDeviceCommunicationSettings)Marshal.GetDelegateForFunctionPointer(_addressOfFindSubDeviceCommunicationSettings, typeof(InternalFindSubDeviceCommunicationSettings));

            StringBuilder deviceNameCopy = new StringBuilder(sizeName);
            deviceNameCopy.Append(deviceName);
            StringBuilder protocolStackNameCopy = new StringBuilder(sizeName);
            protocolStackNameCopy.Append(protocolStackName);
            IntPtr ptrHandle = IntPtr.Zero;
            int result = 0;

            try
            {
                result = func(
                    new IntPtr(deviceHandle),
                    ref ptrHandle,
                    deviceNameCopy,
                    protocolStackNameCopy,
                    sizeName,
                    ref baudrate,
                    ref nodeId,
                    (int)dialogMode,
                    ref errorCode);

                if (result != 0)
                {
                    keyHandle = ptrHandle.ToInt32();
                    deviceName = deviceNameCopy.ToString();
                    protocolStackName = protocolStackNameCopy.ToString();
                }
            }
            catch (ThreadAbortException)
            {
            }

            return result;
        }

        /// <summary>
        /// VcsGetDriverInfo returns the name and version from the Windows DLL.
        /// </summary>
        /// <param name="libraryName">Name from DLL</param>
        /// <param name="maxNameSize">Reserved memory size for the name</param>
        /// <param name="libraryVersion">Version from DLL</param>
        /// <param name="maxVersionSize">Reserved memory size for the version</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        private static int VcsGetDriverInfo(StringBuilder libraryName, ushort maxNameSize, StringBuilder libraryVersion, ushort maxVersionSize, ref uint errorCode)
        {
            if (_addressOfGetDriverInfo == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetDriverInfo not supported");
            }

            InternalGetDriverInfo func = (InternalGetDriverInfo)Marshal.GetDelegateForFunctionPointer(_addressOfGetDriverInfo, typeof(InternalGetDriverInfo));

            return func(libraryName, maxNameSize, libraryVersion, maxVersionSize, ref errorCode);
        }

        /// <summary>
        /// VcsGetVersion returns the firmware version.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="hardwareVersion">Hardware version</param>
        /// <param name="softwareVersion">Software version</param>
        /// <param name="applicationNumber">Application number</param>
        /// <param name="applicationVersion">Application version</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetVersion(int keyHandle, ushort nodeId, ref ushort hardwareVersion, ref ushort softwareVersion, ref ushort applicationNumber, ref ushort applicationVersion, ref uint errorCode)
        {
            int result = 0;

            if (_addressOfGetVersion == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetVersion not supported");
            }

            InternalGetVersion func = (InternalGetVersion)Marshal.GetDelegateForFunctionPointer(_addressOfGetVersion, typeof(InternalGetVersion));

            lock (_syncObject) 
            { 
                result = func(new IntPtr(keyHandle), nodeId, ref hardwareVersion, ref softwareVersion, ref applicationNumber, ref applicationVersion, ref errorCode);
            }

            return result;
        }

        /// <summary>
        /// VcsGetDeviceNameSelection returns all available device names.
        /// </summary>
        /// <param name="startOfSelection">True: Get first selection string False: Get next selection string</param>
        /// <param name="deviceNameSel">Device name</param>
        /// <param name="maxStrSize">Reserved memory size for the device name</param>
        /// <param name="endOfSelection">1: No more selection string available<br></br>0: More string available</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        private static unsafe int VcsGetDeviceNameSelection(int startOfSelection, ref byte[] deviceNameSel, ushort maxStrSize, ref int endOfSelection, ref uint errorCode)
        {
            int result = 0;

            if (_addressOfGetDeviceNameSelection == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetDeviceNameSelection not supported");
            }

            InternalGetDeviceNameSelection func = (InternalGetDeviceNameSelection)Marshal.GetDelegateForFunctionPointer(_addressOfGetDeviceNameSelection, typeof(InternalGetDeviceNameSelection));

            lock(_syncObject)
            {
                fixed (byte* dataBuffer = deviceNameSel)
                {
                    result = func(startOfSelection, dataBuffer, maxStrSize, ref endOfSelection, ref errorCode);
                }
            }

            return result;
        }

        /// <summary>
        /// VcsGetProtocolStackNameSelection returns all available protocol stack names.
        /// </summary>
        /// <param name="deviceName">Device name</param>
        /// <param name="startOfSelection">1: Get first selection string<br></br>0: Get next selection string</param>
        /// <param name="protocolStackNameSel">Pointer to available protocol stack name</param>
        /// <param name="maxStrSize">Reserved memory size for the name</param>
        /// <param name="endOfSelection">1: No more string available<br></br>0: More string available</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        private static int VcsGetProtocolStackNameSelection(string deviceName, int startOfSelection, StringBuilder protocolStackNameSel, ushort maxStrSize, ref int endOfSelection, ref uint errorCode)
        {
            int result = 0;

            if (_addressOfGetProtocolStackNameSelection == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetProtocolStackNameSelection not supported");
            }

            InternalGetProtocolStackNameSelection func = (InternalGetProtocolStackNameSelection)Marshal.GetDelegateForFunctionPointer(_addressOfGetProtocolStackNameSelection, typeof(InternalGetProtocolStackNameSelection));

            lock(_syncObject)
            {
                result = func(deviceName, startOfSelection, protocolStackNameSel, maxStrSize, ref endOfSelection, ref errorCode);
            }

            return result;        
        }

        /// <summary>
        /// VcsGetInterfaceNameSelection returns all available interface names.
        /// </summary>
        /// <param name="deviceName">Device name</param>
        /// <param name="protocolStackName">Protocol stack name</param>
        /// <param name="startOfSelection">1: Get first selection string<br></br>0: Get next selection string</param>
        /// <param name="interfaceNameSel">Interface name</param>
        /// <param name="maxStrSize">Reserved memory size for the interface name</param>
        /// <param name="endOfSelection">1: No more string available<br></br>0: More string available</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        private static int VcsGetInterfaceNameSelection(string deviceName, string protocolStackName, int startOfSelection, StringBuilder interfaceNameSel, ushort maxStrSize, ref int endOfSelection, ref uint errorCode)
        {
            int result = 0;

            if (_addressOfGetInterfaceNameSelection == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetInterfaceNameSelection not supported");
            }

            InternalGetInterfaceNameSelection func = (InternalGetInterfaceNameSelection)Marshal.GetDelegateForFunctionPointer(_addressOfGetInterfaceNameSelection, typeof(InternalGetInterfaceNameSelection));

            lock(_syncObject)
            {
                result = func(deviceName, protocolStackName, startOfSelection, interfaceNameSel, maxStrSize, ref endOfSelection, ref errorCode);
            }

            return result;
        }

        /// <summary>
        /// VcsGetPortNameSelection returns all available port names.
        /// </summary>
        /// <param name="deviceName">Device name</param>
        /// <param name="protocolStackName">Protocol stack name</param>
        /// <param name="interfaceName">Interface name</param>
        /// <param name="startOfSelection">1: Get first selection string<br></br>0: Get next selection string</param>
        /// <param name="portNameSel">Pointer to port name</param>
        /// <param name="maxStrSize">Reserved memory size for the port name</param>
        /// <param name="endOfSelection">1: No more string available<br></br>0: More string available</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        private static int VcsGetPortNameSelection(string deviceName, string protocolStackName, string interfaceName, int startOfSelection, StringBuilder portNameSel, ushort maxStrSize, ref int endOfSelection, ref uint errorCode)
        {
            int result = 0;

            if (_addressOfGetPortNameSelection == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetPortNameSelection not supported");
            }

            InternalGetPortNameSelection func = (InternalGetPortNameSelection)Marshal.GetDelegateForFunctionPointer(_addressOfGetPortNameSelection, typeof(InternalGetPortNameSelection));

            lock(_syncObject)
            {
                result = func(deviceName, protocolStackName, interfaceName, startOfSelection, portNameSel, maxStrSize, ref endOfSelection, ref errorCode);
            }

            return result;        
        }

        /// <summary>
        /// VcsGetBaudrateSelection returns all available baud rates for the connected port.
        /// </summary>
        /// <param name="deviceName">Device name</param>
        /// <param name="protocolStackName">Protocol stack name</param>
        /// <param name="interfaceName">Interface name</param>
        /// <param name="portName">Port name</param>
        /// <param name="startOfSelection">1: Get first selection value<br></br>0: Get next selection value</param>
        /// <param name="baudrateSel">Baud rate [Bit/s]</param>
        /// <param name="endOfSelection">1: No more value available<br></br>0: More value available</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        private static int VcsGetBaudrateSelectionInt(string deviceName, string protocolStackName, string interfaceName, string portName, int startOfSelection, ref uint baudrateSel, ref int endOfSelection, ref uint errorCode)
        {
            int result = 0;

            if (_addressOfGetBaudrateSelection == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetBaudrateSelection not supported");
            }

            InternalGetBaudrateSelection func = (InternalGetBaudrateSelection)Marshal.GetDelegateForFunctionPointer(_addressOfGetBaudrateSelection, typeof(InternalGetBaudrateSelection));

            lock(_syncObject)
            {
                result = func(deviceName, protocolStackName, interfaceName, portName, startOfSelection, ref baudrateSel, ref endOfSelection, ref errorCode);
            }

            return result;
        }

        /// <summary>
        /// VcsGetKeyHandle returns the key handle from the opened interface.
        /// </summary>
        /// <param name="deviceName">Device name</param>
        /// <param name="protocolStackName">Protocol stack name</param>
        /// <param name="interfaceName">Interface name</param>
        /// <param name="portName">Port name</param>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetKeyHandle(string deviceName, string protocolStackName, string interfaceName, string portName, ref int keyHandle, ref uint errorCode)
        {
            IntPtr ptrKeyHandle = IntPtr.Zero;

            if (_addressOfGetKeyHandle == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetKeyHandle not supported");
            }

            InternalGetKeyHandle func = (InternalGetKeyHandle)Marshal.GetDelegateForFunctionPointer(_addressOfGetKeyHandle, typeof(InternalGetKeyHandle));
            int result = func(deviceName, protocolStackName, interfaceName, portName, ref ptrKeyHandle, ref errorCode);

            if (result != 0)
            {
                keyHandle = ptrKeyHandle.ToInt32();
            }

            return result;
        }

        /// <summary>
        /// VcsGetDeviceName returns the device name to corresponding handle.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="deviceName">Device name</param>
        /// <param name="maxStrSize">Reserved memory size for the device name</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetDeviceName(int keyHandle, [MarshalAs(UnmanagedType.LPStr)] StringBuilder deviceName, ushort maxStrSize, ref uint errorCode)
        {
            int result = 0;

            if (_addressOfGetDeviceName == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetDeviceName not supported");
            }

            InternalGetDeviceName func = (InternalGetDeviceName)Marshal.GetDelegateForFunctionPointer(_addressOfGetDeviceName, typeof(InternalGetDeviceName));

            lock (_syncObject) 
            { 
                result = func(new IntPtr(keyHandle), deviceName, maxStrSize, ref errorCode);
            }

            return result;
        }

        /// <summary>
        /// VcsGetProtocolStackName returns the protocol stack name to corresponding handle.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="protocolStackName">Protocol stack name</param>
        /// <param name="maxStrSize">Reserved memory size for the name</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetProtocolStackName(int keyHandle, [MarshalAs(UnmanagedType.LPStr)] StringBuilder protocolStackName, ushort maxStrSize, ref uint errorCode)
        {
            int result = 0;

            if (_addressOfGetProtocolStackName == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetProtocolStackName not supported");
            }

            InternalGetProtocolStackName func = (InternalGetProtocolStackName)Marshal.GetDelegateForFunctionPointer(_addressOfGetProtocolStackName, typeof(InternalGetProtocolStackName));

            lock (_syncObject) 
            { 
                result = func(new IntPtr(keyHandle), protocolStackName, maxStrSize, ref errorCode);
            }

            return result;
        }

        /// <summary>
        /// VcsGetInterfaceName returns the interface name to corresponding handle.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="interfaceName">Interface name</param>
        /// <param name="maxStrSize">Reserved memory size for the name</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetInterfaceName(int keyHandle, [MarshalAs(UnmanagedType.LPStr)] StringBuilder interfaceName, ushort maxStrSize, ref uint errorCode)
        {
            int result = 0;

            if (_addressOfGetInterfaceName == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetInterfaceName not supported");
            }

            InternalGetInterfaceName func = (InternalGetInterfaceName)Marshal.GetDelegateForFunctionPointer(_addressOfGetInterfaceName, typeof(InternalGetInterfaceName));

            lock (_syncObject) 
            { 
                result = func(new IntPtr(keyHandle), interfaceName, maxStrSize, ref errorCode);
            }

            return result;
        }

        /// <summary>
        /// VcsGetPortName returns the port name to corresponding handle.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="portName">Port name</param>
        /// <param name="maxStrSize">Reserved memory size for the name</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetPortName(int keyHandle, [MarshalAs(UnmanagedType.LPStr)] StringBuilder portName, ushort maxStrSize, ref uint errorCode)
        {
            int result = 0;

            if (_addressOfGetPortName == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetPortName not supported");
            }

            InternalGetPortName func = (InternalGetPortName)Marshal.GetDelegateForFunctionPointer(_addressOfGetPortName, typeof(InternalGetPortName));

            lock (_syncObject) 
            { 
                result = func(new IntPtr(keyHandle), portName, maxStrSize, ref errorCode);
            }

            return result;
        }

        /// <summary>
        /// VcsImportParameter writes parameters from a file to the device.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="parameterFileName">Path of the needed file</param>
        /// <param name="showDlg">Show modal dialog</param>
        /// <param name="showMsg">Show message boxes in case of error</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsImportParameter(int keyHandle, ushort nodeId, [MarshalAs(UnmanagedType.LPStr)] string parameterFileName, int showDlg, int showMsg, ref uint errorCode)
        {
            int result = 0;

            if (_addressOfImportParameter == IntPtr.Zero)
            {
                throw new Exception("Function VCS_ImportParameter not supported");
            }

            InternalImportParameter func = (InternalImportParameter)Marshal.GetDelegateForFunctionPointer(_addressOfImportParameter, typeof(InternalImportParameter));

            lock (_syncObject) 
            { 
                result = func(new IntPtr(keyHandle), nodeId, parameterFileName, showDlg, showMsg, ref errorCode);
            }

            return result;
        }

        /// <summary>
        /// VcsExportParameter reads all parameters of the device and writes this into the file.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="parameterFileName">Name of the parameter file.</param>
        /// <param name="firmwareFileName">The binary file.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="comment">The comment.</param>
        /// <param name="showDlg">Show modal dialog.</param>
        /// <param name="showMsg">Show message boxes in case of error.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsExportParameter(int keyHandle, ushort nodeId, [MarshalAs(UnmanagedType.LPStr)] string parameterFileName, [MarshalAs(UnmanagedType.LPStr)] string firmwareFileName, [MarshalAs(UnmanagedType.LPStr)] string userId, [MarshalAs(UnmanagedType.LPStr)] string comment, int showDlg, int showMsg, ref uint errorCode)
        {
            int result = 0;

            if (_addressOfExportParameter == IntPtr.Zero)
            {
                throw new Exception("Function VCS_ExportParameter not supported");
            }

            InternalExportParameter func = (InternalExportParameter)Marshal.GetDelegateForFunctionPointer(_addressOfExportParameter, typeof(InternalExportParameter));

            lock (_syncObject) 
            { 
                result = func(new IntPtr(keyHandle), nodeId, parameterFileName, firmwareFileName, userId, comment, showDlg, showMsg, ref errorCode);
            }

            return result;
        }

        /// <summary>
        /// VcsSetObject writes an object value at the given index and subindex. For information on object index, object subindex, and object length separate document «Firmware Specification».
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="objectIndex">Object index</param>
        /// <param name="objectSubindex">Object sub-index</param>
        /// <param name="data">Object data</param>
        /// <param name="numberOfBytesToWrite">Object length to write (number of bytes)</param>
        /// <param name="numberOfBytesWritten">Object length written (number of bytes)</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetObject(int keyHandle, ushort nodeId, ushort objectIndex, byte objectSubindex, [MarshalAs(UnmanagedType.LPArray)] byte[] data, uint numberOfBytesToWrite, ref uint numberOfBytesWritten, ref uint errorCode)
        {
            int result = 0;
            
            if (_addressOfRestore == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetObject not supported");
            }

            InternalSetObject func = (InternalSetObject)Marshal.GetDelegateForFunctionPointer(_addressOfSetObject, typeof(InternalSetObject));
            
            lock(_syncObject)
            {
                result = func(new IntPtr(keyHandle), nodeId, objectIndex, objectSubindex, data, numberOfBytesToWrite, ref numberOfBytesWritten, ref errorCode);
            }

            return result;
        }

        /// <summary>
        /// VcsGetObject reads an object value at the given index and subindex. For information on object index, object subindex, and object length separate document «Firmware Specification».
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="objectIndex">Object index</param>
        /// <param name="objectSubindex">Object sub-index</param>
        /// <param name="data">Object data</param>
        /// <param name="numberOfBytesToRead">Object length to read (number of bytes)</param>
        /// <param name="numberOfBytesRead">Object length read (number of bytes)</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetObject(int keyHandle, ushort nodeId, ushort objectIndex, byte objectSubindex, [MarshalAs(UnmanagedType.LPArray)] byte[] data, uint numberOfBytesToRead, ref uint numberOfBytesRead, ref uint errorCode)
        {
            int result = 0;

            if (_addressOfRestore == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetObject not supported");
            }

            InternalGetObject func = (InternalGetObject)Marshal.GetDelegateForFunctionPointer(_addressOfGetObject, typeof(InternalGetObject));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, objectIndex, objectSubindex, data, numberOfBytesToRead, ref numberOfBytesRead, ref errorCode); }
            
            return result;    
        }

        /// <summary>
        /// VcsRestore restores all default parameters.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsRestore(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0;
            
            if (_addressOfRestore == IntPtr.Zero)
            {
                throw new Exception("Function VCS_Restore not supported");
            }

            InternalRestore func = (InternalRestore)Marshal.GetDelegateForFunctionPointer(_addressOfRestore, typeof(InternalRestore));

            lock (_syncObject) 
            { 
                result = func(new IntPtr(keyHandle), nodeId, ref errorCode);
            }

            return result;
        }

        /// <summary>
        /// VcsStore stores all parameters.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsStore(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0;

            if (_addressOfStore == IntPtr.Zero)
            {
                throw new Exception("Function VCS_Store not supported");
            }

            InternalStore func = (InternalStore)Marshal.GetDelegateForFunctionPointer(_addressOfStore, typeof(InternalStore));

            lock (_syncObject) 
            { 
                result = func(new IntPtr(keyHandle), nodeId, ref errorCode);
            }

            return result;
        }

        /// <summary>
        /// VcsUpdateFirmware downloads a firmware binary file to the controller.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="binaryFileName">Name of the firmware binary file.</param>
        /// <param name="showDlg">Show modal dialog.</param>
        /// <param name="showHistory">Show history message list.</param>
        /// <param name="showMsg">Show message boxes in case of error.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsUpdateFirmware(int keyHandle, ushort nodeId, [MarshalAs(UnmanagedType.LPStr)] string binaryFileName, int showDlg, int showHistory, int showMsg, ref uint errorCode)
        {
            int result = 0;

            if (_addressOfUpdateFirmware == IntPtr.Zero)
            {
                throw new Exception("Function VCS_UpdateFirmware not supported");
            }

            InternalUpdateFirmware func = (InternalUpdateFirmware)Marshal.GetDelegateForFunctionPointer(_addressOfUpdateFirmware, typeof(InternalUpdateFirmware));
            
            lock (_syncObject) 
            { 
                result = func(new IntPtr(keyHandle), nodeId, binaryFileName, showDlg, showHistory, showMsg, ref errorCode);
            }

            return result;
        }

        /// <summary>
        /// VcsSetMotorType writes the motor type.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="motorType">Kind of motor</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetMotorType(int keyHandle, ushort nodeId, EMotorType motorType, ref uint errorCode)
        {
            int result = 0;

            if (_addressOfSetMotorType == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetMotorType not supported");
            }

            InternalSetMotorType func = (InternalSetMotorType)Marshal.GetDelegateForFunctionPointer(_addressOfSetMotorType, typeof(InternalSetMotorType));
            
            lock (_syncObject) 
            { 
                result = func(new IntPtr(keyHandle), nodeId, (ushort)motorType, ref errorCode);
            }

            return result;
        }

        /// <summary>
        /// VcsSetDcMotorParameter writes all DC motor parameters.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="nominalCurrent">Maximal continuous current.</param>
        /// <param name="maxOutputCurrent">Maximal peak current.</param>
        /// <param name="thermalTimeConstant">Thermal time constant winding.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetDcMotorParameter(int keyHandle, ushort nodeId, ushort nominalCurrent, ushort maxOutputCurrent, ushort thermalTimeConstant, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetDcMotorParameter == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetDcMotorParameter not supported");
            }

            InternalSetDcMotorParameter func = (InternalSetDcMotorParameter)Marshal.GetDelegateForFunctionPointer(_addressOfSetDcMotorParameter, typeof(InternalSetDcMotorParameter));
            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, nominalCurrent, maxOutputCurrent, thermalTimeConstant, ref errorCode); }

            return result;
        }

        /// <summary>
        /// VcsSetEcMotorParameter writes all EC motor parameters.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="nominalCurrent">Maximal continuous current.</param>
        /// <param name="maxOutputCurrent">Maximal peak current.</param>
        /// <param name="thermalTimeConstant">Thermal time constant winding.</param>
        /// <param name="numberOfPolePairs">Number of pole pairs.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetEcMotorParameter(int keyHandle, ushort nodeId, ushort nominalCurrent, ushort maxOutputCurrent, ushort thermalTimeConstant, byte numberOfPolePairs, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetEcMotorParameter == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetEcMotorParameter not supported");
            }

            InternalSetEcMotorParameter func = (InternalSetEcMotorParameter)Marshal.GetDelegateForFunctionPointer(_addressOfSetEcMotorParameter, typeof(InternalSetEcMotorParameter));
            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, nominalCurrent, maxOutputCurrent, thermalTimeConstant, numberOfPolePairs, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetMotorType reads the motor type.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="motorType">Kind of motor.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetMotorType(int keyHandle, ushort nodeId, ref EMotorType motorType, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetMotorType == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetMotorType not supported");
            }

            InternalGetMotorType func = (InternalGetMotorType)Marshal.GetDelegateForFunctionPointer(_addressOfGetMotorType, typeof(InternalGetMotorType));

            ushort mt = 0;

            lock (_syncObject)
            {
                result = func(new IntPtr(keyHandle), nodeId, ref mt, ref errorCode);
            }

            motorType = (EMotorType)mt;

            return result;
        }

        /// <summary>
        /// VcsGetState reads the new state for the state machine.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="state">Control word value.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetState(int keyHandle, ushort nodeId, ref EStates state, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetState == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetState not supported");
            }

            InternalGetState func = (InternalGetState)Marshal.GetDelegateForFunctionPointer(_addressOfGetState, typeof(InternalGetState));

            ushort st = 0;

            lock (_syncObject)
            {
                result = func(new IntPtr(keyHandle), nodeId, ref st, ref errorCode);
            }

            state = (EStates)st;

            return result;
        }

        /// <summary>
        /// VcsSetState writes the current state machine state.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="state">Value of state machine.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetState(int keyHandle, ushort nodeId, EStates state, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetState == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetState not supported");
            }

            InternalSetState func = (InternalSetState)Marshal.GetDelegateForFunctionPointer(_addressOfSetState, typeof(InternalSetState));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, (ushort)state, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsSetMaxProfileVelocity reads the maximal allowed velocity.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="maxProfileVelocity">This value is used as velocity limit in a position (or velocity) move.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetMaxProfileVelocity(int keyHandle, ushort nodeId, uint maxProfileVelocity, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetMaxProfileVelocity == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetMaxProfileVelocity not supported");
            }

            InternalSetMaxProfileVelocity func = (InternalSetMaxProfileVelocity)Marshal.GetDelegateForFunctionPointer(_addressOfSetMaxProfileVelocity, typeof(InternalSetMaxProfileVelocity));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, maxProfileVelocity, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetMaxProfileVelocity writes the maximal allowed velocity.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="maxProfileVelocity">This value is used as velocity limit in a position (or velocity) move.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetMaxProfileVelocity(int keyHandle, ushort nodeId, ref uint maxProfileVelocity, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetMaxProfileVelocity == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetMaxProfileVelocity not supported");
            }

            InternalGetMaxProfileVelocity func = (InternalGetMaxProfileVelocity)Marshal.GetDelegateForFunctionPointer(_addressOfGetMaxProfileVelocity, typeof(InternalGetMaxProfileVelocity));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref maxProfileVelocity, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetDcMotorParameter reads all DC motor parameters.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="nominalCurrent">Maximal continuous current</param>
        /// <param name="maxOutputCurrent">Maximal peak current</param>
        /// <param name="thermalTimeConstant">Thermal time constant winding</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetDcMotorParameter(int keyHandle, ushort nodeId, ref ushort nominalCurrent, ref ushort maxOutputCurrent, ref ushort thermalTimeConstant, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetDcMotorParameter == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetDcMotorParameter not supported");
            }

            InternalGetDcMotorParameter func = (InternalGetDcMotorParameter)Marshal.GetDelegateForFunctionPointer(_addressOfGetDcMotorParameter, typeof(InternalGetDcMotorParameter));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref nominalCurrent, ref maxOutputCurrent, ref thermalTimeConstant, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetEcMotorParameter reads all EC motor parameters.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="nominalCurrent">Maximal continuous current</param>
        /// <param name="maxOutputCurrent">Maximal peak current</param>
        /// <param name="thermalTimeConstant">Thermal time constant winding</param>
        /// <param name="numberOfPolePairs">Number of pole pairs</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetEcMotorParameter(int keyHandle, ushort nodeId, ref ushort nominalCurrent, ref ushort maxOutputCurrent, ref ushort thermalTimeConstant, ref byte numberOfPolePairs, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetEcMotorParameter == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetEcMotorParameter not supported");
            }

            InternalGetEcMotorParameter func = (InternalGetEcMotorParameter)Marshal.GetDelegateForFunctionPointer(_addressOfGetEcMotorParameter, typeof(InternalGetEcMotorParameter));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref nominalCurrent, ref maxOutputCurrent, ref thermalTimeConstant, ref numberOfPolePairs, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsSetSensorType writes the sensor type.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="sensorType">Position Sensor Type</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetSensorType(int keyHandle, ushort nodeId, ESensorType sensorType, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetSensorType == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetSensorType not supported");
            }

            InternalSetSensorType func = (InternalSetSensorType)Marshal.GetDelegateForFunctionPointer(_addressOfSetSensorType, typeof(InternalSetSensorType));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, (ushort)sensorType, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsSetIncEncoderParameter writes the incremental encoder parameters.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="encoderResolution">Encoder pulse number [pulse per turn]</param>
        /// <param name="invertedPolarity">Position sensor polarity</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetIncEncoderParameter(int keyHandle, ushort nodeId, uint encoderResolution, int invertedPolarity, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetIncEncoderParameter == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetIncEncoderParameter not supported");
            }

            InternalSetIncEncoderParameter func = (InternalSetIncEncoderParameter)Marshal.GetDelegateForFunctionPointer(_addressOfSetIncEncoderParameter, typeof(InternalSetIncEncoderParameter));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, encoderResolution, invertedPolarity, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsSetHallSensorParameter writes the Hall sensor parameter.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="invertedPolarity">Position sensor polarity</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetHallSensorParameter(int keyHandle, ushort nodeId, int invertedPolarity, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetHallSensorParameter == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetHallSensorParameter not supported");
            }

            InternalSetHallSensorParameter func = (InternalSetHallSensorParameter)Marshal.GetDelegateForFunctionPointer(_addressOfSetHallSensorParameter, typeof(InternalSetHallSensorParameter));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, invertedPolarity, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsSetSsiAbsEncoderParameter writes all parameters for SSI absolute encoder.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="dataRate">SSI Encoder data rate</param>
        /// <param name="numberOfMultiTurnDataBits">Number of bits multi turn</param>
        /// <param name="numberOfSingleTurnDataBits">Number of bits single turn</param>
        /// <param name="invertedPolarity">Position sensor polarity</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetSsiAbsEncoderParameter(int keyHandle, ushort nodeId, ushort dataRate, ushort numberOfMultiTurnDataBits, ushort numberOfSingleTurnDataBits, int invertedPolarity, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetSsiAbsEncoderParameter == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetSsiAbsEncoderParameter not supported");
            }

            InternalSetSsiAbsEncoderParameter func = (InternalSetSsiAbsEncoderParameter)Marshal.GetDelegateForFunctionPointer(_addressOfSetSsiAbsEncoderParameter, typeof(InternalSetSsiAbsEncoderParameter));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, dataRate, numberOfMultiTurnDataBits, numberOfSingleTurnDataBits, invertedPolarity, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsSetSsiAbsEncoderParameterEx writes all parameters for SSI absolute encoder.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="dataRate">SSI Encoder data rate</param>
        /// <param name="numberOfMultiTurnDataBits">Number of bits multi turn</param>
        /// <param name="numberOfSingleTurnDataBits">Number of bits single turn</param>
        /// <param name="numberOfSpecialDataBits">Number of special databits</param>
        /// <param name="invertedPolarity">Direction maxon/inverted</param>
        /// <param name="timeout">SSI timeout time</param>
        /// <param name="powerupTime">SSI power-up time</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetSsiAbsEncoderParameterEx(int keyHandle, ushort nodeId, ushort dataRate, ushort numberOfMultiTurnDataBits, ushort numberOfSingleTurnDataBits, ushort numberOfSpecialDataBits, int invertedPolarity, ushort timeout, ushort powerupTime, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetSsiAbsEncoderParameterEx == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetSsiAbsEncoderParameterEx not supported");
            }

            InternalSetSsiAbsEncoderParameterEx func = (InternalSetSsiAbsEncoderParameterEx)Marshal.GetDelegateForFunctionPointer(_addressOfSetSsiAbsEncoderParameterEx, typeof(InternalSetSsiAbsEncoderParameterEx));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, dataRate, numberOfMultiTurnDataBits, numberOfSingleTurnDataBits, numberOfSpecialDataBits, invertedPolarity, timeout, powerupTime, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetSensorType reads the sensor type.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="sensorType">Sensor type</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetSensorType(int keyHandle, ushort nodeId, ref ESensorType sensorType, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetSensorType == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetSensorType not supported");
            }

            InternalGetSensorType func = (InternalGetSensorType)Marshal.GetDelegateForFunctionPointer(_addressOfGetSensorType, typeof(InternalGetSensorType));

            ushort st = 0;

            lock (_syncObject)
            {
                result = func(new IntPtr(keyHandle), nodeId, ref st, ref errorCode);
            }
            
            sensorType = (ESensorType)st;

            return result;
        }

        /// <summary>
        /// VcsGetIncEncoderParameter reads the incremental encoder parameters.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="encoderResolution">Encoder pulse number [pulse per turn]</param>
        /// <param name="invertedPolarity">Position sensor polarity</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetIncEncoderParameter(int keyHandle, ushort nodeId, ref uint encoderResolution, ref int invertedPolarity, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetIncEncoderParameter == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetIncEncoderParameter not supported");
            }

            InternalGetIncEncoderParameter func = (InternalGetIncEncoderParameter)Marshal.GetDelegateForFunctionPointer(_addressOfGetIncEncoderParameter, typeof(InternalGetIncEncoderParameter));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref encoderResolution, ref invertedPolarity, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetHallSensorParameter reads the Hall sensor parameters.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="invertedPolarity">Position sensor polarity</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetHallSensorParameter(int keyHandle, ushort nodeId, ref int invertedPolarity, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetHallSensorParameter == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetHallSensorParameter not supported");
            }

            InternalGetHallSensorParameter func = (InternalGetHallSensorParameter)Marshal.GetDelegateForFunctionPointer(_addressOfGetHallSensorParameter, typeof(InternalGetHallSensorParameter));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref invertedPolarity, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetSsiAbsEncoderParameter reads all parameters from SSI absolute encoder.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="dataRate">SSI Encoder data rate</param>
        /// <param name="numberOfMultiTurnDataBits">Number of bits multi turn</param>
        /// <param name="numberOfSingleTurnDataBits">Number of bits single turn</param>
        /// <param name="invertedPolarity">Position sensor polarity</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetSsiAbsEncoderParameter(int keyHandle, ushort nodeId, ref ushort dataRate, ref ushort numberOfMultiTurnDataBits, ref ushort numberOfSingleTurnDataBits, ref int invertedPolarity, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetSsiAbsEncoderParameter == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetSsiAbsEncoderParameter not supported");
            }

            InternalGetSsiAbsEncoderParameter func = (InternalGetSsiAbsEncoderParameter)Marshal.GetDelegateForFunctionPointer(_addressOfGetSsiAbsEncoderParameter, typeof(InternalGetSsiAbsEncoderParameter));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref dataRate, ref numberOfMultiTurnDataBits, ref numberOfSingleTurnDataBits, ref invertedPolarity, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetSsiAbsEncoderParameterEx reads all parameters from SSI absolute encoder.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="dataRate">SSI Encoder data rate</param>
        /// <param name="numberOfMultiTurnDataBits">Number of bits multi turn</param>
        /// <param name="numberOfSingleTurnDataBits">Number of bits single turn</param>
        /// <param name="numberOfSpecialDataBits">Number of special databits</param>
        /// <param name="invertedPolarity">Direction maxon/inverted</param>
        /// <param name="timeout">SSI timeout time</param>
        /// <param name="powerupTime">SSI power-up time</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetSsiAbsEncoderParameterEx(int keyHandle, ushort nodeId, ref ushort dataRate, ref ushort numberOfMultiTurnDataBits, ref ushort numberOfSingleTurnDataBits, ref ushort numberOfSpecialDataBits, ref int invertedPolarity, ref ushort timeout, ref ushort powerupTime, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetSsiAbsEncoderParameterEx == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetSsiAbsEncoderParameterEx not supported");
            }

            InternalGetSsiAbsEncoderParameterEx func = (InternalGetSsiAbsEncoderParameterEx)Marshal.GetDelegateForFunctionPointer(_addressOfGetSsiAbsEncoderParameterEx, typeof(InternalGetSsiAbsEncoderParameterEx));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref dataRate, ref numberOfMultiTurnDataBits, ref numberOfSingleTurnDataBits, ref numberOfSpecialDataBits, ref invertedPolarity, ref timeout, ref powerupTime, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsSetMaxFollowingError reads the maximal allowed following parameter.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="maxFollowingError">Maximal allowed difference of position actual value to position demand value.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetMaxFollowingError(int keyHandle, ushort nodeId, uint maxFollowingError, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetMaxFollowingError == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetMaxFollowingError not supported");
            }

            InternalSetMaxFollowingError func = (InternalSetMaxFollowingError)Marshal.GetDelegateForFunctionPointer(_addressOfSetMaxFollowingError, typeof(InternalSetMaxFollowingError));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, maxFollowingError, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetMaxFollowingError writes the maximal allowed following parameter.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="maxFollowingError">Maximal allowed difference of position actual value to position demand value.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetMaxFollowingError(int keyHandle, ushort nodeId, ref uint maxFollowingError, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetMaxFollowingError == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetMaxFollowingError not supported");
            }

            InternalGetMaxFollowingError func = (InternalGetMaxFollowingError)Marshal.GetDelegateForFunctionPointer(_addressOfGetMaxFollowingError, typeof(InternalGetMaxFollowingError));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref maxFollowingError, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsSetMaxAcceleration writes the maximal allowed acceleration/deceleration.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="maxAcceleration">This value is the limit of the other acceleration/deceleration objects.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetMaxAcceleration(int keyHandle, ushort nodeId, uint maxAcceleration, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetMaxAcceleration == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetMaxAcceleration not supported");
            }

            InternalSetMaxAcceleration func = (InternalSetMaxAcceleration)Marshal.GetDelegateForFunctionPointer(_addressOfSetMaxAcceleration, typeof(InternalSetMaxAcceleration));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, maxAcceleration, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetMaxAcceleration reads the maximal allowed acceleration/deceleration.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="maxAcceleration">This value is the limit of the other acceleration/deceleration objects.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetMaxAcceleration(int keyHandle, ushort nodeId, ref uint maxAcceleration, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetMaxAcceleration == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetMaxAcceleration not supported");
            }

            InternalGetMaxAcceleration func = (InternalGetMaxAcceleration)Marshal.GetDelegateForFunctionPointer(_addressOfGetMaxAcceleration, typeof(InternalGetMaxAcceleration));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref maxAcceleration, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsSetControllerGain writes the regulation tuning controller gain.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="controller">The regulation controller.</param>
        /// <param name="gain">The gain.</param>
        /// <param name="value">The value.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetControllerGain(int keyHandle, ushort nodeId, EController controller, EGain gain, ulong value, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetControllerGain == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetControllerGain not supported");
            }

            InternalSetControllerGain func = (InternalSetControllerGain)Marshal.GetDelegateForFunctionPointer(_addressOfSetControllerGain, typeof(InternalSetControllerGain));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, (ushort)controller, (ushort)gain, (ulong)value, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetControllerGain reads the regulation tuning controller gain.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="controller">The regulation controller.</param>
        /// <param name="gain">The gain.</param>
        /// <param name="value">The value.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetControllerGain(int keyHandle, ushort nodeId, EController controller, EGain gain, ref ulong value, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetControllerGain == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetControllerGain not supported");
            }

            InternalGetControllerGain func = (InternalGetControllerGain)Marshal.GetDelegateForFunctionPointer(_addressOfGetControllerGain, typeof(InternalGetControllerGain));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, (ushort)controller, (ushort)gain, ref value, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsSetPositionRegulatorGain writes all position regulator gains. Determine the optimal parametersusing “Regulation Tuning” in «EPOS Studio».
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="p">Position regulator P-Gain</param>
        /// <param name="i">Position regulator I-Gain</param>
        /// <param name="d">Position regulator D-Gain</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetPositionRegulatorGain(int keyHandle, ushort nodeId, ushort p, ushort i, ushort d, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetPositionRegulatorGain == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetPositionRegulatorGain not supported");
            }

            InternalSetPositionRegulatorGain func = (InternalSetPositionRegulatorGain)Marshal.GetDelegateForFunctionPointer(_addressOfSetPositionRegulatorGain, typeof(InternalSetPositionRegulatorGain));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, p, i, d, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsSetPositionRegulatorFeedForward writes parameters for position regulation with feed forward.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="velocityFeedForward">Velocity feed forward factor</param>
        /// <param name="accelerationFeedForward">Acceleration feed forward factor</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetPositionRegulatorFeedForward(int keyHandle, ushort nodeId, ushort velocityFeedForward, ushort accelerationFeedForward, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetPositionRegulatorFeedForward == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetPositionRegulatorFeedForward not supported");
            }

            InternalSetPositionRegulatorFeedForward func = (InternalSetPositionRegulatorFeedForward)Marshal.GetDelegateForFunctionPointer(_addressOfSetPositionRegulatorFeedForward, typeof(InternalSetPositionRegulatorFeedForward));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, velocityFeedForward, accelerationFeedForward, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetPositionRegulatorGain reads all position regulator gains.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="p">Position regulator P-Gain.</param>
        /// <param name="i">Position regulator I-Gain.</param>
        /// <param name="d">Position regulator D-Gain.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetPositionRegulatorGain(
            int keyHandle,
            ushort nodeId,
            ref ushort p,
            ref ushort i,
            ref ushort d,
            ref uint errorCode)
        {
            int result = 0; if (_addressOfGetPositionRegulatorGain == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetPositionRegulatorGain not supported");
            }

            InternalGetPositionRegulatorGain func = (InternalGetPositionRegulatorGain)Marshal.GetDelegateForFunctionPointer(_addressOfGetPositionRegulatorGain, typeof(InternalGetPositionRegulatorGain));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref p, ref i, ref d, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetPositionRegulatorFeedForward reads parameter for position regulation feed forward.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="velocityFeedForward">Velocity feed forward factor</param>
        /// <param name="accelerationFeedForward">Acceleration feed forward factor</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetPositionRegulatorFeedForward(
            int keyHandle,
            ushort nodeId,
            ref ushort velocityFeedForward,
            ref ushort accelerationFeedForward,
            ref uint errorCode)
        {
            int result = 0; if (_addressOfGetPositionRegulatorFeedForward == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetPositionRegulatorFeedForward not supported");
            }

            InternalGetPositionRegulatorFeedForward func = (InternalGetPositionRegulatorFeedForward)Marshal.GetDelegateForFunctionPointer(_addressOfGetPositionRegulatorFeedForward, typeof(InternalGetPositionRegulatorFeedForward));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref velocityFeedForward, ref accelerationFeedForward, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsSetVelocityRegulatorGain writes all velocity regulator gains. Determine the optimal parameters using “Regulation Tuning” in «EPOS Studio».
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="p">Velocity regulator P-Gain</param>
        /// <param name="i">Velocity regulator I-Gain</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetVelocityRegulatorGain(int keyHandle, ushort nodeId, ushort p, ushort i, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetVelocityRegulatorGain == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetVelocityRegulatorGain not supported");
            }

            InternalSetVelocityRegulatorGain func = (InternalSetVelocityRegulatorGain)Marshal.GetDelegateForFunctionPointer(_addressOfSetVelocityRegulatorGain, typeof(InternalSetVelocityRegulatorGain));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, p, i, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsSetVelocityRegulatorFeedForward writes parameters for velocity regulation with feed forward.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="velocityFeedForward">Velocity feed forward factor</param>
        /// <param name="accelerationFeedForward">Acceleration feed forward factor</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetVelocityRegulatorFeedForward(int keyHandle, ushort nodeId, ushort velocityFeedForward, ushort accelerationFeedForward, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetVelocityRegulatorFeedForward == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetVelocityRegulatorFeedForward not supported");
            }

            InternalSetVelocityRegulatorFeedForward func = (InternalSetVelocityRegulatorFeedForward)Marshal.GetDelegateForFunctionPointer(_addressOfSetVelocityRegulatorFeedForward, typeof(InternalSetVelocityRegulatorFeedForward));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, velocityFeedForward, accelerationFeedForward, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetVelocityRegulatorGain reads all velocity regulator gains.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="p">Velocity regulator P-Gain.</param>
        /// <param name="i">Velocity regulator I-Gain.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetVelocityRegulatorGain(int keyHandle, ushort nodeId, ref ushort p, ref ushort i, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetVelocityRegulatorGain == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetVelocityRegulatorGain not supported");
            }

            InternalGetVelocityRegulatorGain func = (InternalGetVelocityRegulatorGain)Marshal.GetDelegateForFunctionPointer(_addressOfGetVelocityRegulatorGain, typeof(InternalGetVelocityRegulatorGain));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref p, ref i, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetVelocityRegulatorFeedForward reads parameter for velocity regulation feed forward.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="velocityFeedForward">Velocity feed forward factor.</param>
        /// <param name="accelerationFeedForward">Acceleration feed forward factor.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetVelocityRegulatorFeedForward(int keyHandle, ushort nodeId, ref ushort velocityFeedForward, ref ushort accelerationFeedForward, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetVelocityRegulatorFeedForward == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetVelocityRegulatorFeedForward not supported");
            }

            InternalGetVelocityRegulatorFeedForward func = (InternalGetVelocityRegulatorFeedForward)Marshal.GetDelegateForFunctionPointer(_addressOfGetVelocityRegulatorFeedForward, typeof(InternalGetVelocityRegulatorFeedForward));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref velocityFeedForward, ref accelerationFeedForward, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsSetCurrentRegulatorGain writes all current regulator gains. Determine the optimal parameters using “Regulation Tuning” in «EPOS Studio».
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="p">Current regulator P-Gain</param>
        /// <param name="i">Current regulator I-Gain</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetCurrentRegulatorGain(int keyHandle, ushort nodeId, ushort p, ushort i, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetCurrentRegulatorGain == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetCurrentRegulatorGain not supported");
            }

            InternalSetCurrentRegulatorGain func = (InternalSetCurrentRegulatorGain)Marshal.GetDelegateForFunctionPointer(_addressOfSetCurrentRegulatorGain, typeof(InternalSetCurrentRegulatorGain));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, p, i, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetCurrentRegulatorGain enables reading all current regulator gains.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="p">Current regulator P-Gain</param>
        /// <param name="i">Current regulator I-Gain</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetCurrentRegulatorGain(int keyHandle, ushort nodeId, ref ushort p, ref ushort i, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetCurrentRegulatorGain == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetCurrentRegulatorGain not supported");
            }

            InternalGetCurrentRegulatorGain func = (InternalGetCurrentRegulatorGain)Marshal.GetDelegateForFunctionPointer(_addressOfGetCurrentRegulatorGain, typeof(InternalGetCurrentRegulatorGain));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref p, ref i, ref errorCode); }
            return result;
        }

        /// <summary>
        /// The function "VcsDigitalInputConfiguration" sets the parameter for one digital input.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="digitalInputNb">Number of digital input (object subindex)</param>
        /// <param name="configuration">Configures the functionality assigned to the digital input (bit number) => See Firmware Specification</param>
        /// <param name="mask">1: Functionality state will be displayed<br></br>0: not displayed</param>
        /// <param name="polarity">1: Low active<br></br>0: High active</param>
        /// <param name="executionMask">1: Set the error routine. Only for positive and negative switch.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsDigitalInputConfiguration(int keyHandle, ushort nodeId, ushort digitalInputNb, EDigitalInputConfiguration configuration, int mask, int polarity, int executionMask, ref uint errorCode)
        {
            int result = 0; if (_addressOfDigitalInputConfiguration == IntPtr.Zero)
            {
                throw new Exception("Function VCS_DigitalInputConfiguration not supported");
            }

            InternalDigitalInputConfiguration func = (InternalDigitalInputConfiguration)Marshal.GetDelegateForFunctionPointer(_addressOfDigitalInputConfiguration, typeof(InternalDigitalInputConfiguration));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, digitalInputNb, (ushort)configuration, mask, polarity, executionMask, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsDigitalOutputConfiguration sets parameter for one digital output.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="digitalOutputNb">Number of digital output (object subindex)</param>
        /// <param name="configuration">Configures the functionality assigned to the digital output (bit number) => See Firmware Specification</param>
        /// <param name="state">State of digital output</param>
        /// <param name="mask">1: Register will be modified</param>
        /// <param name="polarity">1: Output will be inverted</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsDigitalOutputConfiguration(int keyHandle, ushort nodeId, ushort digitalOutputNb, EDigitalOutputConfiguration configuration, int state, int mask, int polarity, ref uint errorCode)
        {
            int result = 0; if (_addressOfDigitalOutputConfiguration == IntPtr.Zero)
            {
                throw new Exception("Function VCS_DigitalOutputConfiguration not supported");
            }

            InternalDigitalOutputConfiguration func = (InternalDigitalOutputConfiguration)Marshal.GetDelegateForFunctionPointer(_addressOfDigitalOutputConfiguration, typeof(InternalDigitalOutputConfiguration));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, digitalOutputNb, (ushort)configuration, state, mask, polarity, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsAnalogInputConfiguration sets parameter for an analog input.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="analogInputNb">Number of analog input</param>
        /// <param name="configuration">Configures the functionality assigned to the analog input => See Firmware Specification.</param>
        /// <param name="executionMask">1: Register will be modified.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsAnalogInputConfiguration(int keyHandle, ushort nodeId, ushort analogInputNb, EAnalogInputConfiguration configuration, int executionMask, ref uint errorCode)
        {
            int result = 0; if (_addressOfAnalogInputConfiguration == IntPtr.Zero)
            {
                throw new Exception("Function VCS_AnalogInputConfiguration not supported");
            }

            InternalAnalogInputConfiguration func = (InternalAnalogInputConfiguration)Marshal.GetDelegateForFunctionPointer(_addressOfAnalogInputConfiguration, typeof(InternalAnalogInputConfiguration));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, analogInputNb, (ushort)configuration, executionMask, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsAnalogOutputConfiguration sets parameter for an analog output.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="analogOutputNb">Number of analog output</param>
        /// <param name="configuration">Configures the functionality assigned to the analog output => See Firmware Specification.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsAnalogOutputConfiguration(int keyHandle, ushort nodeId, ushort analogOutputNb, EAnalogOutputConfiguration configuration, ref uint errorCode)
        {
            int result = 0; if (_addressOfAnalogOutputConfiguration == IntPtr.Zero)
            {
                throw new Exception("Function VCS_AnalogOutputConfiguration not supported");
            }

            InternalAnalogOutputConfiguration func = (InternalAnalogOutputConfiguration)Marshal.GetDelegateForFunctionPointer(_addressOfAnalogOutputConfiguration, typeof(InternalAnalogOutputConfiguration));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, analogOutputNb, (ushort)configuration, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsSetVelocityUnits writes velocity unit parameters.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="velDimension">Velocity dimension index => See Firmware Specification</param>
        /// <param name="velNotation">Velocity notation index => See Firmware Specification</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetVelocityUnits(int keyHandle, ushort nodeId, EVelocityDimension velDimension, EVelocityNotation velNotation, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetVelocityUnits == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetVelocityUnits not supported");
            }

            InternalSetVelocityUnits func = (InternalSetVelocityUnits)Marshal.GetDelegateForFunctionPointer(_addressOfSetVelocityUnits, typeof(InternalSetVelocityUnits));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, (byte)velDimension, (sbyte)velNotation, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetVelocityUnits reads velocity unit parameters.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="velDimension">Velocity dimension index => See Firmware Specification</param>
        /// <param name="velNotation">Velocity notation index => See Firmware Specification</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetVelocityUnits(int keyHandle, ushort nodeId, ref EVelocityDimension velDimension, ref EVelocityNotation velNotation, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetVelocityUnits == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetVelocityUnits not supported");
            }

            InternalGetVelocityUnits func = (InternalGetVelocityUnits)Marshal.GetDelegateForFunctionPointer(_addressOfGetVelocityUnits, typeof(InternalGetVelocityUnits));

            byte vd = 0;
            sbyte vn = 0;

            lock (_syncObject)
            {
                result = func(new IntPtr(keyHandle), nodeId, ref vd, ref vn, ref errorCode);
            }

            velDimension = (EVelocityDimension)vd;
            velNotation = (EVelocityNotation)vn;

            return result;
        }

        /// <summary>
        /// VcsSetMotorParameter" writes all motor parameters.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="motorType">Kind of motor.</param>
        /// <param name="continuousCurrent">The continuous current.</param>
        /// <param name="peakCurrent">The peak current.</param>
        /// <param name="polePair">Number of pole pairs.</param>
        /// <param name="thermalTimeConstant">Thermal time constant winding.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetMotorParameter(int keyHandle, ushort nodeId, EMotorType motorType, ushort continuousCurrent, ushort peakCurrent, byte polePair, ushort thermalTimeConstant, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetMotorParameter == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetMotorParameter not supported");
            }

            InternalSetMotorParameter func = (InternalSetMotorParameter)Marshal.GetDelegateForFunctionPointer(_addressOfSetMotorParameter, typeof(InternalSetMotorParameter));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, (ushort)motorType, continuousCurrent, peakCurrent, polePair, thermalTimeConstant, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsSetEncoderParameter writes all encoder parameters.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="counts">Incremental Encoder counts [pulse per turn]</param>
        /// <param name="positionSensorType">Position Sensor Type</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetEncoderParameter(int keyHandle, ushort nodeId, ushort counts, ESensorType positionSensorType, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetEncoderParameter == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetEncoderParameter not supported");
            }

            InternalSetEncoderParameter func = (InternalSetEncoderParameter)Marshal.GetDelegateForFunctionPointer(_addressOfSetEncoderParameter, typeof(InternalSetEncoderParameter));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, counts, (ushort)positionSensorType, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetMotorParameter reads all motor parameters.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="motorType">Kind of motor</param>
        /// <param name="continuousCurrent">The continuous current.</param>
        /// <param name="peakCurrent">The peak current.</param>
        /// <param name="polePair">Number of pole pairs</param>
        /// <param name="thermalTimeConstant">Thermal time constant winding</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetMotorParameter(int keyHandle, ushort nodeId, ref EMotorType motorType, ref ushort continuousCurrent, ref ushort peakCurrent, ref byte polePair, ref ushort thermalTimeConstant, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetMotorParameter == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetMotorParameter not supported");
            }

            InternalGetMotorParameter func = (InternalGetMotorParameter)Marshal.GetDelegateForFunctionPointer(_addressOfGetMotorParameter, typeof(InternalGetMotorParameter));

            ushort mt = 0;

            lock (_syncObject)
            {
                result = func(new IntPtr(keyHandle), nodeId, ref mt, ref continuousCurrent, ref peakCurrent, ref polePair, ref thermalTimeConstant, ref errorCode);
            }

            motorType = (EMotorType)mt;

            return result;
        }

        /// <summary>
        /// VcsGetEncoderParameter reads all encoder parameters.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="counts">The counts.</param>
        /// <param name="positionSensorType">Type of the position sensor.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetEncoderParameter(int keyHandle, ushort nodeId, ref ushort counts, ref ESensorType positionSensorType, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetEncoderParameter == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetEncoderParameter not supported");
            }

            InternalGetEncoderParameter func = (InternalGetEncoderParameter)Marshal.GetDelegateForFunctionPointer(_addressOfGetEncoderParameter, typeof(InternalGetEncoderParameter));

            ushort pst = 0;

            lock (_syncObject)
            {
                result = func(new IntPtr(keyHandle), nodeId, ref counts, ref pst, ref errorCode);
            }

            positionSensorType = (ESensorType)pst;

            return result;
        }

        /// <summary>
        /// VcsSetOperationMode sets the operation mode.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="operationMode">Operation mode, Interpolated Position Mode(7), Homing mode(6), Profile Velocity Mode(3), Profile Position Mode(1), Position Mode(-1), Velocity Mode(-2), Current Mode(-3), Diagnostic Mode(-4), Master Encoder Mode(-5), Step/Direction Mode(-6)</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetOperationMode(int keyHandle, ushort nodeId, EOperationMode operationMode, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetOperationMode == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetOperationMode not supported");
            }

            InternalSetOperationMode func = (InternalSetOperationMode)Marshal.GetDelegateForFunctionPointer(_addressOfSetOperationMode, typeof(InternalSetOperationMode));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, (sbyte)operationMode, ref errorCode); }

            return result;
        }

        /// <summary>
        /// VcsGetOperationMode returns the activated operation mode.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="operationMode">Operation mode</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetOperationMode(int keyHandle, ushort nodeId, ref EOperationMode operationMode, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetOperationMode == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetOperationMode not supported");
            }

            InternalGetOperationMode func = (InternalGetOperationMode)Marshal.GetDelegateForFunctionPointer(_addressOfGetOperationMode, typeof(InternalGetOperationMode));

            sbyte om = 0;
        
            lock (_syncObject)
            {
                result = func(new IntPtr(keyHandle), nodeId, ref om, ref errorCode);
            }
            
            operationMode = (EOperationMode)om;

            return result;
        }

        /// <summary>
        /// VcsResetDevice is used to send the NMT service ‘Reset Node’. It is a command without acknowledge.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsResetDevice(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfResetDevice == IntPtr.Zero)
            {
                throw new Exception("Function VCS_ResetDevice not supported");
            }

            InternalResetDevice func = (InternalResetDevice)Marshal.GetDelegateForFunctionPointer(_addressOfResetDevice, typeof(InternalResetDevice));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }

            return result;
        }

        /// <summary>
        /// VcsSetEnableState changes the device state to “enable”.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetEnableState(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetEnableState == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetEnableState not supported");
            }

            InternalSetEnableState func = (InternalSetEnableState)Marshal.GetDelegateForFunctionPointer(_addressOfSetEnableState, typeof(InternalSetEnableState));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }

            return result;
        }

        /// <summary>
        /// VcsSetDisableState changes the device state to “disable”.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetDisableState(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetDisableState == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetDisableState not supported");
            }

            InternalSetDisableState func = (InternalSetDisableState)Marshal.GetDelegateForFunctionPointer(_addressOfSetDisableState, typeof(InternalSetDisableState));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }

            return result;
        }

        /// <summary>
        /// VcsSetQuickStopState changes the device state to “quick stop”.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetQuickStopState(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetQuickStopState == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetQuickStopState not supported");
            }

            InternalSetQuickStopState func = (InternalSetQuickStopState)Marshal.GetDelegateForFunctionPointer(_addressOfSetQuickStopState, typeof(InternalSetQuickStopState));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }

            return result;
        }

        /// <summary>
        /// VcsClearFault changes the device state from “fault” to “disable”.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsClearFault(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfClearFault == IntPtr.Zero)
            {
                throw new Exception("Function VCS_ClearFault not supported");
            }

            InternalClearFault func = (InternalClearFault)Marshal.GetDelegateForFunctionPointer(_addressOfClearFault, typeof(InternalClearFault));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }

            return result;
        }

        /// <summary>
        /// VcsGetEnableState checks if the device is enabled.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="isEnabled">1: Device enabled<br></br>0: Device not enabled</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetEnableState(int keyHandle, ushort nodeId, ref int isEnabled, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetEnableState == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetEnableState not supported");
            }

            InternalGetEnableState func = (InternalGetEnableState)Marshal.GetDelegateForFunctionPointer(_addressOfGetEnableState, typeof(InternalGetEnableState));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref isEnabled, ref errorCode); }

            return result;
        }

        /// <summary>
        /// VcsGetDisableState checks if the device is disabled.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="isDisabled">1: Device disabled<br></br>0: Device not disabled</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetDisableState(int keyHandle, ushort nodeId, ref int isDisabled, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetDisableState == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetDisableState not supported");
            }

            InternalGetDisableState func = (InternalGetDisableState)Marshal.GetDelegateForFunctionPointer(_addressOfGetDisableState, typeof(InternalGetDisableState));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref isDisabled, ref errorCode); }

            return result;
        }

        /// <summary>
        /// VcsGetQuickStopState returns the device state quick stop.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="isQuickStopped">1: Device is in quick stop state<br></br>0: Device is not in quick stop state</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetQuickStopState(int keyHandle, ushort nodeId, ref int isQuickStopped, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetQuickStopState == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetQuickStopState not supported");
            }

            InternalGetQuickStopState func = (InternalGetQuickStopState)Marshal.GetDelegateForFunctionPointer(_addressOfGetQuickStopState, typeof(InternalGetQuickStopState));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref isQuickStopped, ref errorCode); }

            return result;
        }

        /// <summary>
        /// VcsGetFaultState returns the device state fault.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="isInFault">1: Device is in fault state<br></br>0: Device is not in fault state</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetFaultState(int keyHandle, ushort nodeId, ref int isInFault, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetFaultState == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetFaultState not supported");
            }

            InternalGetFaultState func = (InternalGetFaultState)Marshal.GetDelegateForFunctionPointer(_addressOfGetFaultState, typeof(InternalGetFaultState));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref isInFault, ref errorCode); }

            return result;
        }

        /// <summary>
        /// VcsGetNbOfDeviceError returns the number of actual errors that are recorded.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="numberOfDeviceError">Number of occurred device errors</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetNbOfDeviceError(int keyHandle, ushort nodeId, ref byte numberOfDeviceError, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetNbOfDeviceError == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetNbOfDeviceError not supported");
            }

            InternalGetNbOfDeviceError func = (InternalGetNbOfDeviceError)Marshal.GetDelegateForFunctionPointer(_addressOfGetNbOfDeviceError, typeof(InternalGetNbOfDeviceError));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref numberOfDeviceError, ref errorCode); }

            return result;
        }

        /// <summary>
        /// VcsGetDeviceErrorCode returns the error code of selected sub-index.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="deviceErrorNumber">Object sub-index of device error.</param>
        /// <param name="deviceErrorCode">Actual error code from error history</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetDeviceErrorCode(int keyHandle, ushort nodeId, byte deviceErrorNumber, ref uint deviceErrorCode, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetDeviceErrorCode == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetDeviceErrorCode not supported");
            }

            InternalGetDeviceErrorCode func = (InternalGetDeviceErrorCode)Marshal.GetDelegateForFunctionPointer(_addressOfGetDeviceErrorCode, typeof(InternalGetDeviceErrorCode));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, deviceErrorNumber, ref deviceErrorCode, ref errorCode); }

            return result;
        }

        /// <summary>
        /// VcsGetErrorInfo returns the error information about the executed function from a received error code. The function returns communication and library errors but no device errors.
        /// </summary>
        /// <param name="errorCodeValue">Received error code</param>
        /// <param name="errorInfo">Error string</param>
        /// <param name="maxStrSize">Reserved memory size for the name</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetErrorInfo(uint errorCodeValue, ref string errorInfo, ushort maxStrSize)
        {
            int result = 0; if (_addressOfGetErrorInfo == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetErrorInfo not supported");
            }

            InternalGetErrorInfo func = (InternalGetErrorInfo)Marshal.GetDelegateForFunctionPointer(_addressOfGetErrorInfo, typeof(InternalGetErrorInfo));

            StringBuilder errofInfo2 = new StringBuilder(maxStrSize);
            
            result = func(errorCodeValue, errofInfo2, (ushort)errofInfo2.Capacity);
            
            errorInfo = errofInfo2.ToString();

            return result;
        }

        /// <summary>
        /// VcsGetMovementState checks if the drive has reached target.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="targetReached">The drive has reached the target. The function reads actual state of bit 10 from the status word.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetMovementState(int keyHandle, ushort nodeId, ref int targetReached, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetMovementState == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetMovementState not supported");
            }

            InternalGetMovementState func = (InternalGetMovementState)Marshal.GetDelegateForFunctionPointer(_addressOfGetMovementState, typeof(InternalGetMovementState));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref targetReached, ref errorCode); }

            return result;
        }

        /// <summary>
        /// VcsGetPositionIs returns the position actual value.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="positionIs">Position actual value</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetPositionIs(int keyHandle, ushort nodeId, ref int positionIs, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetPositionIs == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetPositionIs not supported");
            }

            InternalGetPositionIs func = (InternalGetPositionIs)Marshal.GetDelegateForFunctionPointer(_addressOfGetPositionIs, typeof(InternalGetPositionIs));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref positionIs, ref errorCode); }

            return result;
        }

        /// <summary>
        /// VcsGetVelocityIs reads the velocity actual value.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="velocityIs">Velocity actual value</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetVelocityIs(int keyHandle, ushort nodeId, ref int velocityIs, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetVelocityIs == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetVelocityIs not supported");
            }

            InternalGetVelocityIs func = (InternalGetVelocityIs)Marshal.GetDelegateForFunctionPointer(_addressOfGetVelocityIs, typeof(InternalGetVelocityIs));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref velocityIs, ref errorCode); }

            return result;
        }

        /// <summary>
        /// VcsGetVelocityIsAveraged reads the current average velocity value.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="velocityIsAveraged">Current average velocity value</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetVelocityIsAveraged(int keyHandle, ushort nodeId, ref int velocityIsAveraged, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetVelocityIsAveraged == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetVelocityIsAveraged not supported");
            }

            InternalGetVelocityIsAveraged func = (InternalGetVelocityIsAveraged)Marshal.GetDelegateForFunctionPointer(_addressOfGetVelocityIsAveraged, typeof(InternalGetVelocityIsAveraged));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref velocityIsAveraged, ref errorCode); }

            return result;
        }

        /// <summary>
        /// VcsGetCurrentIs returns the current actual value.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="currentIs">Current actual value</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetCurrentIs(int keyHandle, ushort nodeId, ref short currentIs, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetCurrentIs == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetCurrentIs not supported");
            }

            InternalGetCurrentIs func = (InternalGetCurrentIs)Marshal.GetDelegateForFunctionPointer(_addressOfGetCurrentIs, typeof(InternalGetCurrentIs));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref currentIs, ref errorCode); }

            return result;
        }

        /// <summary>
        /// VcsGetCurrentIsAveraged returns the current actual value.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="currentIsAveraged">Current average current value</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetCurrentIsAveraged(int keyHandle, ushort nodeId, ref short currentIsAveraged, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetCurrentIsAveraged == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetCurrentIsAveraged not supported");
            }

            InternalGetCurrentIsAveraged func = (InternalGetCurrentIsAveraged)Marshal.GetDelegateForFunctionPointer(_addressOfGetCurrentIsAveraged, typeof(InternalGetCurrentIsAveraged));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref currentIsAveraged, ref errorCode); }

            return result;
        }

        /// <summary>
        /// VcsActivateProfilePositionMode changes the operational mode to “profile position mode”.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsActivateProfilePositionMode(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfActivateProfilePositionMode == IntPtr.Zero)
            {
                throw new Exception("Function VCS_ActivateProfilePositionMode not supported");
            }

            InternalActivateProfilePositionMode func = (InternalActivateProfilePositionMode)Marshal.GetDelegateForFunctionPointer(_addressOfActivateProfilePositionMode, typeof(InternalActivateProfilePositionMode));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }

            return result;
        }

        /// <summary>
        /// VcsSetPositionProfile sets the position profile parameters.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="profileVelocity">Position profile velocity</param>
        /// <param name="profileAcceleration">The profile acceleration.</param>
        /// <param name="profileDeceleration">The profile deceleration.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetPositionProfile(int keyHandle, ushort nodeId, uint profileVelocity, uint profileAcceleration, uint profileDeceleration, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetPositionProfile == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetPositionProfile not supported");
            }

            InternalSetPositionProfile func =
                (InternalSetPositionProfile)
                Marshal.GetDelegateForFunctionPointer(_addressOfSetPositionProfile, typeof(InternalSetPositionProfile));
            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, profileVelocity, profileAcceleration, profileDeceleration, ref errorCode); }

            return result;
        }

        /// <summary>
        /// VcsGetPositionProfile returns the position profile parameters.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="profileVelocity">Position profile velocity</param>
        /// <param name="profileAcceleration">The profile acceleration.</param>
        /// <param name="profileDeceleration">The profile deceleration.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetPositionProfile(int keyHandle, ushort nodeId, ref uint profileVelocity, ref uint profileAcceleration, ref uint profileDeceleration, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetPositionProfile == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetPositionProfile not supported");
            }

            InternalGetPositionProfile func = (InternalGetPositionProfile)Marshal.GetDelegateForFunctionPointer(_addressOfGetPositionProfile, typeof(InternalGetPositionProfile));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref profileVelocity, ref profileAcceleration, ref profileDeceleration, ref errorCode); }

            return result;
        }

        /// <summary>
        /// VcsMoveToPosition starts movement with position profile to target position.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="targetPosition">Target position</param>
        /// <param name="absolute">TRUE starts an absolute,FALSE a relative movement</param>
        /// <param name="immediately">TRUE starts immediately, FALSE waits to end of last positioning</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsMoveToPosition(int keyHandle, ushort nodeId, int targetPosition, int absolute, int immediately, ref uint errorCode)
        {
            int result = 0; if (_addressOfMoveToPosition == IntPtr.Zero)
            {
                throw new Exception("Function VCS_MoveToPosition not supported");
            }

            InternalMoveToPosition func = (InternalMoveToPosition)Marshal.GetDelegateForFunctionPointer(_addressOfMoveToPosition, typeof(InternalMoveToPosition));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, targetPosition, absolute, immediately, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetTargetPosition returns the profile position mode target value.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="targetPosition">Target position.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetTargetPosition(int keyHandle, ushort nodeId, ref int targetPosition, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetTargetPosition == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetTargetPosition not supported");
            }

            InternalGetTargetPosition func = (InternalGetTargetPosition)Marshal.GetDelegateForFunctionPointer(_addressOfGetTargetPosition, typeof(InternalGetTargetPosition));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref targetPosition, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsHaltPositionMovement stops the movement with profile deceleration.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsHaltPositionMovement(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfHaltPositionMovement == IntPtr.Zero)
            {
                throw new Exception("Function VCS_HaltPositionMovement not supported");
            }

            InternalHaltPositionMovement func = (InternalHaltPositionMovement)Marshal.GetDelegateForFunctionPointer(_addressOfHaltPositionMovement, typeof(InternalHaltPositionMovement));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsEnablePositionWindow activates the position window.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="positionWindow">Position window value</param>
        /// <param name="positionWindowTime">Position window time value</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsEnablePositionWindow(int keyHandle, ushort nodeId, uint positionWindow, ushort positionWindowTime, ref uint errorCode)
        {
            int result = 0; if (_addressOfEnablePositionWindow == IntPtr.Zero)
            {
                throw new Exception("Function VCS_EnablePositionWindow not supported");
            }

            InternalEnablePositionWindow func = (InternalEnablePositionWindow)Marshal.GetDelegateForFunctionPointer(_addressOfEnablePositionWindow, typeof(InternalEnablePositionWindow));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, positionWindow, positionWindowTime, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsDisablePositionWindow deactivates the position window.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsDisablePositionWindow(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfDisablePositionWindow == IntPtr.Zero)
            {
                throw new Exception("Function VCS_DisablePositionWindow not supported");
            }

            InternalDisablePositionWindow func = (InternalDisablePositionWindow)Marshal.GetDelegateForFunctionPointer(_addressOfDisablePositionWindow, typeof(InternalDisablePositionWindow));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsActivateProfileVelocityMode changes the operational mode to “profile velocity mode”.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsActivateProfileVelocityMode(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfActivateProfileVelocityMode == IntPtr.Zero)
            {
                throw new Exception("Function VCS_ActivateProfileVelocityMode not supported");
            }

            InternalActivateProfileVelocityMode func = (InternalActivateProfileVelocityMode)Marshal.GetDelegateForFunctionPointer(_addressOfActivateProfileVelocityMode, typeof(InternalActivateProfileVelocityMode));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsSetVelocityProfile sets the velocity profile parameters.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="profileAcceleration">The profile acceleration.</param>
        /// <param name="profileDeceleration">The profile deceleration.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetVelocityProfile(int keyHandle, ushort nodeId, uint profileAcceleration, uint profileDeceleration, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetVelocityProfile == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetVelocityProfile not supported");
            }

            InternalSetVelocityProfile func = (InternalSetVelocityProfile)Marshal.GetDelegateForFunctionPointer(_addressOfSetVelocityProfile, typeof(InternalSetVelocityProfile));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, profileAcceleration, profileDeceleration, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetVelocityProfile returns the velocity profile parameters.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="profileAcceleration">The p profile acceleration.</param>
        /// <param name="profileDeceleration">The p profile deceleration.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetVelocityProfile(int keyHandle, ushort nodeId, ref uint profileAcceleration, ref uint profileDeceleration, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetVelocityProfile == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetVelocityProfile not supported");
            }

            InternalGetVelocityProfile func = (InternalGetVelocityProfile)Marshal.GetDelegateForFunctionPointer(_addressOfGetVelocityProfile, typeof(InternalGetVelocityProfile));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref profileAcceleration, ref profileDeceleration, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsMoveWithVelocity starts the movement with velocity profile to target velocity.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="targetVelocity">Target velocity</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsMoveWithVelocity(int keyHandle, ushort nodeId, int targetVelocity, ref uint errorCode)
        {
            int result = 0; if (_addressOfMoveWithVelocity == IntPtr.Zero)
            {
                throw new Exception("Function VCS_MoveWithVelocity not supported");
            }

            InternalMoveWithVelocity func = (InternalMoveWithVelocity)Marshal.GetDelegateForFunctionPointer(_addressOfMoveWithVelocity, typeof(InternalMoveWithVelocity));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, targetVelocity, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetTargetVelocity returns the profile velocity mode target value.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="targetVelocity">Target velocity</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetTargetVelocity(int keyHandle, ushort nodeId, ref int targetVelocity, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetTargetVelocity == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetTargetVelocity not supported");
            }

            InternalGetTargetVelocity func = (InternalGetTargetVelocity)Marshal.GetDelegateForFunctionPointer(_addressOfGetTargetVelocity, typeof(InternalGetTargetVelocity));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref targetVelocity, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsHaltVelocityMovement stops the movement with profile deceleration.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsHaltVelocityMovement(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfHaltVelocityMovement == IntPtr.Zero)
            {
                throw new Exception("Function VCS_HaltVelocityMovement not supported");
            }

            InternalHaltVelocityMovement func = (InternalHaltVelocityMovement)Marshal.GetDelegateForFunctionPointer(_addressOfHaltVelocityMovement, typeof(InternalHaltVelocityMovement));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsEnableVelocityWindow activates the velocity window.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="velocityWindow">Velocity window value</param>
        /// <param name="velocityWindowTime">Velocity window time value</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsEnableVelocityWindow(int keyHandle, ushort nodeId, uint velocityWindow, ushort velocityWindowTime, ref uint errorCode)
        {
            int result = 0; if (_addressOfEnableVelocityWindow == IntPtr.Zero)
            {
                throw new Exception("Function VCS_EnableVelocityWindow not supported");
            }

            InternalEnableVelocityWindow func = (InternalEnableVelocityWindow)Marshal.GetDelegateForFunctionPointer(_addressOfEnableVelocityWindow, typeof(InternalEnableVelocityWindow));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, velocityWindow, velocityWindowTime, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsDisableVelocityWindow deactivates the velocity window.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsDisableVelocityWindow(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfDisableVelocityWindow == IntPtr.Zero)
            {
                throw new Exception("Function VCS_DisableVelocityWindow not supported");
            }

            InternalDisableVelocityWindow func = (InternalDisableVelocityWindow)Marshal.GetDelegateForFunctionPointer(_addressOfDisableVelocityWindow, typeof(InternalDisableVelocityWindow));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsActivateHomingMode changes the operational mode to “homing mode”.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsActivateHomingMode(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfActivateHomingMode == IntPtr.Zero)
            {
                throw new Exception("Function VCS_ActivateHomingMode not supported");
            }

            InternalActivateHomingMode func = (InternalActivateHomingMode)Marshal.GetDelegateForFunctionPointer(_addressOfActivateHomingMode, typeof(InternalActivateHomingMode));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsSetHomingParameter writes all homing parameters. The parameter units depend on (position, velocity, acceleration) notation index.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="homingAcceleration">Acceleration for homing profile</param>
        /// <param name="speedSwitch">Speed during search for switch</param>
        /// <param name="speedIndex">Speed during search for index signal</param>
        /// <param name="homeOffset">Home offset after homing</param>
        /// <param name="currentThreshold">Current threshold for homing method -1, -2, -3 and -4</param>
        /// <param name="homePosition">Assign the current Homing position with this value</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetHomingParameter(int keyHandle, ushort nodeId, uint homingAcceleration, uint speedSwitch, uint speedIndex, int homeOffset, ushort currentThreshold, int homePosition, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetHomingParameter == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetHomingParameter not supported");
            }

            InternalSetHomingParameter func = (InternalSetHomingParameter)Marshal.GetDelegateForFunctionPointer(_addressOfSetHomingParameter, typeof(InternalSetHomingParameter));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, homingAcceleration, speedSwitch, speedIndex, homeOffset, currentThreshold, homePosition, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetHomingParameter reads all homing parameters. The parameter units depend on (position, velocity, acceleration) notation index.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="homingAcceleration">Acceleration for homing profile</param>
        /// <param name="speedSwitch">Speed during search for switch</param>
        /// <param name="speedIndex">Speed during search for index signal</param>
        /// <param name="homeOffset">Home offset after homing</param>
        /// <param name="currentThreshold">Current threshold for homing method -1, -2, -3 and -4</param>
        /// <param name="homePosition">Home position value</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetHomingParameter(int keyHandle, ushort nodeId, ref uint homingAcceleration, ref uint speedSwitch, ref uint speedIndex, ref int homeOffset, ref ushort currentThreshold, ref int homePosition, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetHomingParameter == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetHomingParameter not supported");
            }

            InternalGetHomingParameter func = (InternalGetHomingParameter)Marshal.GetDelegateForFunctionPointer(_addressOfGetHomingParameter, typeof(InternalGetHomingParameter));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref homingAcceleration, ref speedSwitch, ref speedIndex, ref homeOffset, ref currentThreshold, ref homePosition, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsFindHome and HomingMethod permit to find the system home (for example, a home switch).
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="homingMethod">Homing method</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsFindHome(int keyHandle, ushort nodeId, EHomingMethod homingMethod, ref uint errorCode)
        {
            int result = 0; if (_addressOfFindHome == IntPtr.Zero)
            {
                throw new Exception("Function VCS_FindHome not supported");
            }

            InternalFindHome func = (InternalFindHome)Marshal.GetDelegateForFunctionPointer(_addressOfFindHome, typeof(InternalFindHome));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, (sbyte)homingMethod, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsStopHoming interrupts homing.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsStopHoming(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfStopHoming == IntPtr.Zero)
            {
                throw new Exception("Function VCS_StopHoming not supported");
            }

            InternalStopHoming func = (InternalStopHoming)Marshal.GetDelegateForFunctionPointer(_addressOfStopHoming, typeof(InternalStopHoming));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsDefinePosition uses homing method 35 (Actual Position) to set a new home position.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="homePosition">Assign the homing position with this value</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsDefinePosition(int keyHandle, ushort nodeId, int homePosition, ref uint errorCode)
        {
            int result = 0; if (_addressOfDefinePosition == IntPtr.Zero)
            {
                throw new Exception("Function VCS_DefinePosition not supported");
            }

            InternalDefinePosition func = (InternalDefinePosition)Marshal.GetDelegateForFunctionPointer(_addressOfDefinePosition, typeof(InternalDefinePosition));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, homePosition, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsWaitForHomingAttained is waiting until the state is changed to homing attained or the time is up.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="timeout">Max. wait time until homing attained</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsWaitForHomingAttained(int keyHandle, ushort nodeId, uint timeout, ref uint errorCode)
        {
            int result = 0; if (_addressOfWaitForHomingAttained == IntPtr.Zero)
            {
                throw new Exception("Function VCS_WaitForHomingAttained not supported");
            }

            InternalWaitForHomingAttained func = (InternalWaitForHomingAttained)Marshal.GetDelegateForFunctionPointer(_addressOfWaitForHomingAttained, typeof(InternalWaitForHomingAttained));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, timeout, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetHomingState returns the states if the homing position is attained and if an homing error has occurred.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="homingAttained">The drive has attained homing position. The function reads current state of bit 12 from the status word.</param>
        /// <param name="homingError">An error occured during homing. The function reads current state of bit 13 from the status word.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetHomingState(int keyHandle, ushort nodeId, ref int homingAttained, ref int homingError, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetHomingState == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetHomingState not supported");
            }

            InternalGetHomingState func = (InternalGetHomingState)Marshal.GetDelegateForFunctionPointer(_addressOfGetHomingState, typeof(InternalGetHomingState));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref homingAttained, ref homingError, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsActivateInterpolatedPositionMode changes the operational mode to “interpolated position mode”.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsActivateInterpolatedPositionMode(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfActivateInterpolatedPositionMode == IntPtr.Zero)
            {
                throw new Exception("Function VCS_ActivateInterpolatedPositionMode not supported");
            }

            InternalActivateInterpolatedPositionMode func = (InternalActivateInterpolatedPositionMode)Marshal.GetDelegateForFunctionPointer(_addressOfActivateInterpolatedPositionMode, typeof(InternalActivateInterpolatedPositionMode));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsSetIpmBufferParameter sets warning borders of the data input.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="underflowWarningLimit">This object gives lower signalization level of the data input FIFO.</param>
        /// <param name="overflowWarningLimit">This object gives the higher signalization level of the data input FIFO.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetIpmBufferParameter(int keyHandle, ushort nodeId, ushort underflowWarningLimit, ushort overflowWarningLimit, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetIpmBufferParameter == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetIpmBufferParameter not supported");
            }

            InternalSetIpmBufferParameter func = (InternalSetIpmBufferParameter)Marshal.GetDelegateForFunctionPointer(_addressOfSetIpmBufferParameter, typeof(InternalSetIpmBufferParameter));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, underflowWarningLimit, overflowWarningLimit, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetIpmBufferParameter reads warning borders and the max. buffer size of the data input.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="underflowWarningLimit">This object gives lower signalization level of the data input FIFO.</param>
        /// <param name="overflowWarningLimit">This object gives the higher signalization level of the data input FIFO.</param>
        /// <param name="maxBufferSize">This object provides the maximal buffer size</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetIpmBufferParameter(int keyHandle, ushort nodeId, ref ushort underflowWarningLimit, ref ushort overflowWarningLimit, ref uint maxBufferSize, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetIpmBufferParameter == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetIpmBufferParameter not supported");
            }

            InternalGetIpmBufferParameter func = (InternalGetIpmBufferParameter)Marshal.GetDelegateForFunctionPointer(_addressOfGetIpmBufferParameter, typeof(InternalGetIpmBufferParameter));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref underflowWarningLimit, ref overflowWarningLimit, ref maxBufferSize, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsClearIpmBuffer clears the input buffer and enables access to the input buffer for drive functions.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsClearIpmBuffer(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfClearIpmBuffer == IntPtr.Zero)
            {
                throw new Exception("Function VCS_ClearIpmBuffer not supported");
            }

            InternalClearIpmBuffer func = (InternalClearIpmBuffer)Marshal.GetDelegateForFunctionPointer(_addressOfClearIpmBuffer, typeof(InternalClearIpmBuffer));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetFreeIpmBufferSize reads the available buffer size.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="bufferSize">Actual free buffer size</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetFreeIpmBufferSize(int keyHandle, ushort nodeId, ref uint bufferSize, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetFreeIpmBufferSize == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetFreeIpmBufferSize not supported");
            }

            InternalGetFreeIpmBufferSize func = (InternalGetFreeIpmBufferSize)Marshal.GetDelegateForFunctionPointer(_addressOfGetFreeIpmBufferSize, typeof(InternalGetFreeIpmBufferSize));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref bufferSize, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsAddPvtValueToIpmBuffer adds a new PVT reference point to the device.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="position">Position of the reference point</param>
        /// <param name="velocity">Velocity of the reference point</param>
        /// <param name="time">Time of the reference point</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsAddPvtValueToIpmBuffer(int keyHandle, ushort nodeId, int position, int velocity, byte time, ref uint errorCode)
        {
            int result = 0; if (_addressOfAddPvtValueToIpmBuffer == IntPtr.Zero)
            {
                throw new Exception("Function VCS_AddPvtValueToIpmBuffer not supported");
            }

            InternalAddPvtValueToIpmBuffer func = (InternalAddPvtValueToIpmBuffer)Marshal.GetDelegateForFunctionPointer(_addressOfAddPvtValueToIpmBuffer, typeof(InternalAddPvtValueToIpmBuffer));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, position, velocity, time, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsStartIpmTrajectory starts the IPM trajectory.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsStartIpmTrajectory(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfStartIpmTrajectory == IntPtr.Zero)
            {
                throw new Exception("Function VCS_StartIpmTrajectory not supported");
            }

            InternalStartIpmTrajectory func = (InternalStartIpmTrajectory)Marshal.GetDelegateForFunctionPointer(_addressOfStartIpmTrajectory, typeof(InternalStartIpmTrajectory));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsStopIpmTrajectory stops the IPM trajectory.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsStopIpmTrajectory(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfStopIpmTrajectory == IntPtr.Zero)
            {
                throw new Exception("Function VCS_StopIpmTrajectory not supported");
            }

            InternalStopIpmTrajectory func = (InternalStopIpmTrajectory)Marshal.GetDelegateForFunctionPointer(_addressOfStopIpmTrajectory, typeof(InternalStopIpmTrajectory));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetIpmStatus returns different warning and error states.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="trajectoryRunning">State if IPM active</param>
        /// <param name="isUnderflowWarning">State if buffer underflow level is reached</param>
        /// <param name="isOverflowWarning">State if buffer overflow level is reached</param>
        /// <param name="isVelocityWarning">State if IPM velocity greater than profile velocity</param>
        /// <param name="isAccelerationWarning">State if IPM acceleration greater than profile acceleration</param>
        /// <param name="isUnderflowError">State of underflow error</param>
        /// <param name="isOverflowError">State of overflow error</param>
        /// <param name="isVelocityError">State if IPM velocity greater than max. profile velocity</param>
        /// <param name="isAccelerationError">State if IPM acceleration greater than max. profile acceleration</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetIpmStatus(int keyHandle, ushort nodeId, ref int trajectoryRunning, ref int isUnderflowWarning, ref int isOverflowWarning, ref int isVelocityWarning, ref int isAccelerationWarning, ref int isUnderflowError, ref int isOverflowError, ref int isVelocityError, ref int isAccelerationError, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetIpmStatus == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetIpmStatus not supported");
            }

            InternalGetIpmStatus func = (InternalGetIpmStatus)Marshal.GetDelegateForFunctionPointer(_addressOfGetIpmStatus, typeof(InternalGetIpmStatus));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref trajectoryRunning, ref isUnderflowWarning, ref isOverflowWarning, ref isVelocityWarning, ref isAccelerationWarning, ref isUnderflowError, ref isOverflowError, ref isVelocityError, ref isAccelerationError, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsActivatePositionMode changes the operational mode to “position mode”.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsActivatePositionMode(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfActivatePositionMode == IntPtr.Zero)
            {
                throw new Exception("Function VCS_ActivatePositionMode not supported");
            }

            InternalActivatePositionMode func = (InternalActivatePositionMode)Marshal.GetDelegateForFunctionPointer(_addressOfActivatePositionMode, typeof(InternalActivatePositionMode));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsSetPositionMust sets the position mode setting value.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="positionMust">Position mode setting value</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetPositionMust(int keyHandle, ushort nodeId, int positionMust, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetPositionMust == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetPositionMust not supported");
            }

            InternalSetPositionMust func = (InternalSetPositionMust)Marshal.GetDelegateForFunctionPointer(_addressOfSetPositionMust, typeof(InternalSetPositionMust));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, positionMust, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetPositionMust returns the position mode setting value.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="positionMust">Position mode setting value</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetPositionMust(int keyHandle, ushort nodeId, ref int positionMust, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetPositionMust == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetPositionMust not supported");
            }

            InternalGetPositionMust func = (InternalGetPositionMust)Marshal.GetDelegateForFunctionPointer(_addressOfGetPositionMust, typeof(InternalGetPositionMust));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref positionMust, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsActivateAnalogPositionSetpoint configures the selected analog input for analog position setpoint.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="analogInputNumber">Number of the used analog input</param>
        /// <param name="scaling">The scaling factor for analog position setpoint functionality</param>
        /// <param name="offset">Offset for analog position setpoint functionality</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsActivateAnalogPositionSetpoint(int keyHandle, ushort nodeId, ushort analogInputNumber, float scaling, int offset, ref uint errorCode)
        {
            int result = 0; if (_addressOfActivateAnalogPositionSetpoint == IntPtr.Zero)
            {
                throw new Exception("Function VCS_ActivateAnalogPositionSetpoint not supported");
            }

            InternalActivateAnalogPositionSetpoint func = (InternalActivateAnalogPositionSetpoint)Marshal.GetDelegateForFunctionPointer(_addressOfActivateAnalogPositionSetpoint, typeof(InternalActivateAnalogPositionSetpoint));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, analogInputNumber, scaling, offset, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsDeactivateAnalogPositionSetpoint disable the selected analog input for analog position setpoint.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="analogInputNumber">Number of the used analog input</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsDeactivateAnalogPositionSetpoint(int keyHandle, ushort nodeId, ushort analogInputNumber, ref uint errorCode)
        {
            int result = 0; if (_addressOfDeactivateAnalogPositionSetpoint == IntPtr.Zero)
            {
                throw new Exception("Function VCS_DeactivateAnalogPositionSetpoint not supported");
            }

            InternalDeactivateAnalogPositionSetpoint func = (InternalDeactivateAnalogPositionSetpoint)Marshal.GetDelegateForFunctionPointer(_addressOfDeactivateAnalogPositionSetpoint, typeof(InternalDeactivateAnalogPositionSetpoint));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, analogInputNumber, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsEnableAnalogPositionSetpoint enable the execution mask for analog position setpoint.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsEnableAnalogPositionSetpoint(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfEnableAnalogPositionSetpoint == IntPtr.Zero)
            {
                throw new Exception("Function VCS_EnableAnalogPositionSetpoint not supported");
            }

            InternalEnableAnalogPositionSetpoint func = (InternalEnableAnalogPositionSetpoint)Marshal.GetDelegateForFunctionPointer(_addressOfEnableAnalogPositionSetpoint, typeof(InternalEnableAnalogPositionSetpoint));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsDisableAnalogPositionSetpoint disable the execution mask for analog position setpoint.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsDisableAnalogPositionSetpoint(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfDisableAnalogPositionSetpoint == IntPtr.Zero)
            {
                throw new Exception("Function VCS_DisableAnalogPositionSetpoint not supported");
            }

            InternalDisableAnalogPositionSetpoint func = (InternalDisableAnalogPositionSetpoint)Marshal.GetDelegateForFunctionPointer(_addressOfDisableAnalogPositionSetpoint, typeof(InternalDisableAnalogPositionSetpoint));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsActivateVelocityMode changes the operational mode to “velocity mode”.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsActivateVelocityMode(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfActivateVelocityMode == IntPtr.Zero)
            {
                throw new Exception("Function VCS_ActivateVelocityMode not supported");
            }

            InternalActivateVelocityMode func = (InternalActivateVelocityMode)Marshal.GetDelegateForFunctionPointer(_addressOfActivateVelocityMode, typeof(InternalActivateVelocityMode));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsSetVelocityMust sets the velocity mode setting value.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="velocityMust">Velocity mode setting value</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetVelocityMust(int keyHandle, ushort nodeId, int velocityMust, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetVelocityMust == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetVelocityMust not supported");
            }

            InternalSetVelocityMust func = (InternalSetVelocityMust)Marshal.GetDelegateForFunctionPointer(_addressOfSetVelocityMust, typeof(InternalSetVelocityMust));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, velocityMust, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetVelocityMust returns the velocity mode setting value.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="velocityMust">Velocity mode setting value</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetVelocityMust(int keyHandle, ushort nodeId, ref int velocityMust, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetVelocityMust == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetVelocityMust not supported");
            }

            InternalGetVelocityMust func = (InternalGetVelocityMust)Marshal.GetDelegateForFunctionPointer(_addressOfGetVelocityMust, typeof(InternalGetVelocityMust));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref velocityMust, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsActivateAnalogVelocitySetpoint configures the selected analog input for analog velocity setpoint.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="analogInputNumber">Number of the used analog input</param>
        /// <param name="scaling">The scaling factor for analog velocity setpoint functionality</param>
        /// <param name="offset">Offset for analog velocity setpoint functionality</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsActivateAnalogVelocitySetpoint(int keyHandle, ushort nodeId, ushort analogInputNumber, float scaling, int offset, ref uint errorCode)
        {
            int result = 0; if (_addressOfActivateAnalogVelocitySetpoint == IntPtr.Zero)
            {
                throw new Exception("Function VCS_ActivateAnalogVelocitySetpoint not supported");
            }

            InternalActivateAnalogVelocitySetpoint func = (InternalActivateAnalogVelocitySetpoint)Marshal.GetDelegateForFunctionPointer(_addressOfActivateAnalogVelocitySetpoint, typeof(InternalActivateAnalogVelocitySetpoint));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, analogInputNumber, scaling, offset, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsDeactivateAnalogVelocitySetpoint disable the selected analog input for analog velocity setpoint.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="analogInputNumber">Number of the used analog input</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsDeactivateAnalogVelocitySetpoint(int keyHandle, ushort nodeId, ushort analogInputNumber, ref uint errorCode)
        {
            int result = 0; if (_addressOfDeactivateAnalogVelocitySetpoint == IntPtr.Zero)
            {
                throw new Exception("Function VCS_DeactivateAnalogVelocitySetpoint not supported");
            }

            InternalDeactivateAnalogVelocitySetpoint func = (InternalDeactivateAnalogVelocitySetpoint)Marshal.GetDelegateForFunctionPointer(_addressOfDeactivateAnalogVelocitySetpoint, typeof(InternalDeactivateAnalogVelocitySetpoint));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, analogInputNumber, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsEnableAnalogVelocitySetpoint enable the execution mask for analog velocity setpoint.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsEnableAnalogVelocitySetpoint(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfEnableAnalogVelocitySetpoint == IntPtr.Zero)
            {
                throw new Exception("Function VCS_EnableAnalogVelocitySetpoint not supported");
            }

            InternalEnableAnalogVelocitySetpoint func = (InternalEnableAnalogVelocitySetpoint)Marshal.GetDelegateForFunctionPointer(_addressOfEnableAnalogVelocitySetpoint, typeof(InternalEnableAnalogVelocitySetpoint));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsDisableAnalogVelocitySetpoint disable the execution mask for analog velocity setpoint.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsDisableAnalogVelocitySetpoint(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfDisableAnalogVelocitySetpoint == IntPtr.Zero)
            {
                throw new Exception("Function VCS_DisableAnalogVelocitySetpoint not supported");
            }

            InternalDisableAnalogVelocitySetpoint func = (InternalDisableAnalogVelocitySetpoint)Marshal.GetDelegateForFunctionPointer(_addressOfDisableAnalogVelocitySetpoint, typeof(InternalDisableAnalogVelocitySetpoint));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsActivateCurrentMode change operational mode to current mode.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsActivateCurrentMode(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfActivateCurrentMode == IntPtr.Zero)
            {
                throw new Exception("Function VCS_ActivateCurrentMode not supported");
            }

            InternalActivateCurrentMode func = (InternalActivateCurrentMode)Marshal.GetDelegateForFunctionPointer(_addressOfActivateCurrentMode, typeof(InternalActivateCurrentMode));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsSetCurrentMust writes current mode setting value.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="currentMust">Current mode setting value</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetCurrentMust(int keyHandle, ushort nodeId, short currentMust, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetCurrentMust == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetCurrentMust not supported");
            }

            InternalSetCurrentMust func = (InternalSetCurrentMust)Marshal.GetDelegateForFunctionPointer(_addressOfSetCurrentMust, typeof(InternalSetCurrentMust));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, currentMust, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetCurrentMust reads the current mode setting value.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="currentMust">Current mode setting value</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetCurrentMust(int keyHandle, ushort nodeId, ref short currentMust, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetCurrentMust == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetCurrentMust not supported");
            }

            InternalGetCurrentMust func = (InternalGetCurrentMust)Marshal.GetDelegateForFunctionPointer(_addressOfGetCurrentMust, typeof(InternalGetCurrentMust));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref currentMust, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsActivateAnalogCurrentSetpoint configures the selected analog input for analog current setpoint.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="analogInputNumber">Number of the used analog input</param>
        /// <param name="scaling">The scaling factor for analog current setpoint functionality</param>
        /// <param name="offset">Offset for analog current setpoint functionality</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsActivateAnalogCurrentSetpoint(int keyHandle, ushort nodeId, ushort analogInputNumber, float scaling, short offset, ref uint errorCode)
        {
            int result = 0; if (_addressOfActivateAnalogCurrentSetpoint == IntPtr.Zero)
            {
                throw new Exception("Function VCS_ActivateAnalogCurrentSetpoint not supported");
            }

            InternalActivateAnalogCurrentSetpoint func = (InternalActivateAnalogCurrentSetpoint)Marshal.GetDelegateForFunctionPointer(_addressOfActivateAnalogCurrentSetpoint, typeof(InternalActivateAnalogCurrentSetpoint));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, analogInputNumber, scaling, offset, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsDeactivateAnalogCurrentSetpoint disable the selected analog input for analog current setpoint.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="analogInputNumber">Number of the used analog input</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsDeactivateAnalogCurrentSetpoint(int keyHandle, ushort nodeId, ushort analogInputNumber, ref uint errorCode)
        {
            int result = 0; if (_addressOfDeactivateAnalogCurrentSetpoint == IntPtr.Zero)
            {
                throw new Exception("Function VCS_DeactivateAnalogCurrentSetpoint not supported");
            }

            InternalDeactivateAnalogCurrentSetpoint func = (InternalDeactivateAnalogCurrentSetpoint)Marshal.GetDelegateForFunctionPointer(_addressOfDeactivateAnalogCurrentSetpoint, typeof(InternalDeactivateAnalogCurrentSetpoint));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, analogInputNumber, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsEnableAnalogCurrentSetpoint enable the execution mask for analog current setpoint.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsEnableAnalogCurrentSetpoint(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfEnableAnalogCurrentSetpoint == IntPtr.Zero)
            {
                throw new Exception("Function VCS_EnableAnalogCurrentSetpoint not supported");
            }

            InternalEnableAnalogCurrentSetpoint func = (InternalEnableAnalogCurrentSetpoint)Marshal.GetDelegateForFunctionPointer(_addressOfEnableAnalogCurrentSetpoint, typeof(InternalEnableAnalogCurrentSetpoint));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsDisableAnalogCurrentSetpoint disable the execution mask for analog current setpoint.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsDisableAnalogCurrentSetpoint(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfDisableAnalogCurrentSetpoint == IntPtr.Zero)
            {
                throw new Exception("Function VCS_DisableAnalogCurrentSetpoint not supported");
            }

            InternalDisableAnalogCurrentSetpoint func = (InternalDisableAnalogCurrentSetpoint)Marshal.GetDelegateForFunctionPointer(_addressOfDisableAnalogCurrentSetpoint, typeof(InternalDisableAnalogCurrentSetpoint));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsActivateMasterEncoderMode changes the operational mode to “master encoder mode”.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsActivateMasterEncoderMode(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfActivateMasterEncoderMode == IntPtr.Zero)
            {
                throw new Exception("Function VCS_ActivateMasterEncoderMode not supported");
            }

            InternalActivateMasterEncoderMode func = (InternalActivateMasterEncoderMode)Marshal.GetDelegateForFunctionPointer(_addressOfActivateMasterEncoderMode, typeof(InternalActivateMasterEncoderMode));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsSetMasterEncoderParameter writes all parameters for master encoder mode.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="scalingNumerator">Scaling numerator for position calculation</param>
        /// <param name="scalingDenominator">Scaling denominator for position calculation</param>
        /// <param name="polarity">Polarity of the direction input. 0: Positive 1: Negative</param>
        /// <param name="maxVelocity">This parameter is the maximal allowed speed during a profiled move.</param>
        /// <param name="maxAcceleration">Defines the maximal allowed acceleration.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetMasterEncoderParameter(int keyHandle, ushort nodeId, ushort scalingNumerator, ushort scalingDenominator, byte polarity, uint maxVelocity, uint maxAcceleration, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetMasterEncoderParameter == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetMasterEncoderParameter not supported");
            }

            InternalSetMasterEncoderParameter func = (InternalSetMasterEncoderParameter)Marshal.GetDelegateForFunctionPointer(_addressOfSetMasterEncoderParameter, typeof(InternalSetMasterEncoderParameter));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, scalingNumerator, scalingDenominator, polarity, maxVelocity, maxAcceleration, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetMasterEncoderParameter reads all parameters for master encoder mode.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="scalingNumerator">Scaling numerator for position calculation</param>
        /// <param name="scalingDenominator">Scaling denominator for position calculation</param>
        /// <param name="polarity">Polarity of the direction input. 0: Positive 1: Negative</param>
        /// <param name="maxVelocity">This parameter is the maximal allowed speed during a profiled move.</param>
        /// <param name="maxAcceleration">Defines the maximal allowed acceleration.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetMasterEncoderParameter(int keyHandle, ushort nodeId, ref ushort scalingNumerator, ref ushort scalingDenominator, ref byte polarity, ref uint maxVelocity, ref uint maxAcceleration, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetMasterEncoderParameter == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetMasterEncoderParameter not supported");
            }

            InternalGetMasterEncoderParameter func = (InternalGetMasterEncoderParameter)Marshal.GetDelegateForFunctionPointer(_addressOfGetMasterEncoderParameter, typeof(InternalGetMasterEncoderParameter));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref scalingNumerator, ref scalingDenominator, ref polarity, ref maxVelocity, ref maxAcceleration, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsActivateStepDirectionMode changes the operational mode to “step direction mode”.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsActivateStepDirectionMode(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfActivateStepDirectionMode == IntPtr.Zero)
            {
                throw new Exception("Function VCS_ActivateStepDirectionMode not supported");
            }

            InternalActivateStepDirectionMode func = (InternalActivateStepDirectionMode)Marshal.GetDelegateForFunctionPointer(_addressOfActivateStepDirectionMode, typeof(InternalActivateStepDirectionMode));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsSetStepDirectionParameter writes all parameters for step direction mode.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="scalingNumerator">Scaling numerator for position calculation</param>
        /// <param name="scalingDenominator">Scaling denominator for position calculation</param>
        /// <param name="polarity">Polarity of the direction input. 0: Positive 1: Negative</param>
        /// <param name="maxVelocity">This parameter is the maximal allowed speed during a profiled move.</param>
        /// <param name="maxAcceleration">Defines the maximal allowed acceleration.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetStepDirectionParameter(int keyHandle, ushort nodeId, ushort scalingNumerator, ushort scalingDenominator, byte polarity, uint maxVelocity, uint maxAcceleration, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetStepDirectionParameter == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetStepDirectionParameter not supported");
            }

            InternalSetStepDirectionParameter func = (InternalSetStepDirectionParameter)Marshal.GetDelegateForFunctionPointer(_addressOfSetStepDirectionParameter, typeof(InternalSetStepDirectionParameter));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, scalingNumerator, scalingDenominator, polarity, maxVelocity, maxAcceleration, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetStepDirectionParameter reads all parameters for step direction mode.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="scalingNumerator">Scaling numerator for position calculation</param>
        /// <param name="scalingDenominator">Scaling denominator for position calculation</param>
        /// <param name="polarity">Polarity of the direction input. 0: Positive 1: Negative</param>
        /// <param name="maxVelocity">This parameter is the maximal allowed speed during a profiled move.</param>
        /// <param name="maxAcceleration">Defines the maximal allowed acceleration.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetStepDirectionParameter(int keyHandle, ushort nodeId, ref ushort scalingNumerator, ref ushort scalingDenominator, ref byte polarity, ref uint maxVelocity, ref uint maxAcceleration, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetStepDirectionParameter == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetStepDirectionParameter not supported");
            }

            InternalGetStepDirectionParameter func = (InternalGetStepDirectionParameter)Marshal.GetDelegateForFunctionPointer(_addressOfGetStepDirectionParameter, typeof(InternalGetStepDirectionParameter));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref scalingNumerator, ref scalingDenominator, ref polarity, ref maxVelocity, ref maxAcceleration, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetAllDigitalInputs returns state of all digital inputs.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="inputs">Display the state of the digital input functionalities. If a bit is read as “1", the functionality is activated.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetAllDigitalInputs(int keyHandle, ushort nodeId, ref ushort inputs, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetAllDigitalInputs == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetAllDigitalInputs not supported");
            }

            InternalGetAllDigitalInputs func = (InternalGetAllDigitalInputs)Marshal.GetDelegateForFunctionPointer(_addressOfGetAllDigitalInputs, typeof(InternalGetAllDigitalInputs));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref inputs, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetAllDigitalOutputs returns state of all digital outputs.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="outputs">State of all digital outputs. Activated if a bit is read as “1”.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetAllDigitalOutputs(int keyHandle, ushort nodeId, ref ushort outputs, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetAllDigitalOutputs == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetAllDigitalOutputs not supported");
            }

            InternalGetAllDigitalOutputs func = (InternalGetAllDigitalOutputs)Marshal.GetDelegateForFunctionPointer(_addressOfGetAllDigitalOutputs, typeof(InternalGetAllDigitalOutputs));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref outputs, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsSetAllDigitalOutputs set state of all digital outputs.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="outputs">State of all digital outputs. If a bit is written as “1", the state is activated.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetAllDigitalOutputs(int keyHandle, ushort nodeId, ushort outputs, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetAllDigitalOutputs == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetAllDigitalOutputs not supported");
            }

            InternalSetAllDigitalOutputs func = (InternalSetAllDigitalOutputs)Marshal.GetDelegateForFunctionPointer(_addressOfSetAllDigitalOutputs, typeof(InternalSetAllDigitalOutputs));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, outputs, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetAnalogInput returns the value from an analog input.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="inputNumber">Analog input number</param>
        /// <param name="analogValue">Analog value from input</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetAnalogInput(int keyHandle, ushort nodeId, ushort inputNumber, ref ushort analogValue, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetAnalogInput == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetAnalogInput not supported");
            }

            InternalGetAnalogInput func = (InternalGetAnalogInput)Marshal.GetDelegateForFunctionPointer(_addressOfGetAnalogInput, typeof(InternalGetAnalogInput));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, inputNumber, ref analogValue, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetAnalogInputVoltage returns the voltage from an analog input.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="inputNumber">Analog input number</param>
        /// <param name="voltageValue">Analog voltage value from input</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetAnalogInputVoltage(int keyHandle, ushort nodeId, ushort inputNumber, ref int voltageValue, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetAnalogInputVoltage == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetAnalogInputVoltage not supported");
            }

            InternalGetAnalogInputVoltage func = (InternalGetAnalogInputVoltage)Marshal.GetDelegateForFunctionPointer(_addressOfGetAnalogInputVoltage, typeof(InternalGetAnalogInputVoltage));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, inputNumber, ref voltageValue, ref errorCode); }
            return result;

        }

        /// <summary>
        /// VcsGetAnalogInputVoltageState returns the state value from an analog input functionality.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="configuration">Analog input functionality configuration</param>
        /// <param name="stateValue">State value from input functionality</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetAnalogInputState(int keyHandle, ushort nodeId, EAnalogInputConfiguration configuration, ref int stateValue, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetAnalogInputState == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetAnalogInputState not supported");
            }

            InternalGetAnalogInputState func = (InternalGetAnalogInputState)Marshal.GetDelegateForFunctionPointer(_addressOfGetAnalogInputState, typeof(InternalGetAnalogInputState));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, (ushort)configuration, ref stateValue, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsSetAnalogOutput set the voltage level of an analog output.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="outputNumber">Analog output number</param>
        /// <param name="analogValue">Analog value for output</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetAnalogOutput(int keyHandle, ushort nodeId, ushort outputNumber, ushort analogValue, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetAnalogOutput == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetAnalogOutput not supported");
            }

            InternalSetAnalogOutput func = (InternalSetAnalogOutput)Marshal.GetDelegateForFunctionPointer(_addressOfSetAnalogOutput, typeof(InternalSetAnalogOutput));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, outputNumber, analogValue, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsSetAnalogOutputVoltage set the voltage level of an analog output.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="outputNumber">Analog output number</param>
        /// <param name="voltageValue">Analog voltage value for output</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetAnalogOutputVoltage(int keyHandle, ushort nodeId, ushort outputNumber, int voltageValue, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetAnalogOutputVoltage == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetAnalogOutputVoltage not supported");
            }

            InternalSetAnalogOutputVoltage func = (InternalSetAnalogOutputVoltage)Marshal.GetDelegateForFunctionPointer(_addressOfSetAnalogOutputVoltage, typeof(InternalSetAnalogOutputVoltage));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, outputNumber, voltageValue, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsSetAnalogOutputState set the voltage level of an analog output.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="configuration">Analog output functionality configuration</param>
        /// <param name="stateValue">State value for output functionality</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetAnalogOutputState(int keyHandle, ushort nodeId, EAnalogOutputConfiguration configuration, int stateValue, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetAnalogOutputState == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetAnalogOutputState not supported");
            }

            InternalSetAnalogOutputState func = (InternalSetAnalogOutputState)Marshal.GetDelegateForFunctionPointer(_addressOfSetAnalogOutputState, typeof(InternalSetAnalogOutputState));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, (ushort)configuration, stateValue, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsSetPositionCompareParameter write all parameters for position compare.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="operationalMode">Used operational mode in position sequence mode: 0: Single position mode 1: Position sequence mode</param>
        /// <param name="intervalMode">Used interval mode in position sequence mode:<br></br>0: Interval positions are set in negative direction relative to the position compare reference position<br></br>1: Interval positions are set in positive direction relative to the position compare reference position<br></br>2: Interval positions are set in positive and negative direction relative to the position compare reference position</param>
        /// <param name="directionDependency">Used direction dependency in position sequence mode:<br></br>0: Positions are compared only if actual motor direction is negative<br></br>1: Positions are compared only if actual motor direction is positive<br></br>2: Positions are compared regardless of the actual motor direction</param>
        /// <param name="intervalWidth">This object holds the width of the position intervals</param>
        /// <param name="intervalRepetitions">This object allows to configure the number of position intervals to be considered by position compare</param>
        /// <param name="pulseWidth">This object configures the pulse width of the trigger output</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetPositionCompareParameter(int keyHandle, ushort nodeId, EPositionCompareOperationalMode operationalMode, EPositionCompareIntervalMode intervalMode, EPositionCompareDirectionDependency directionDependency, ushort intervalWidth, ushort intervalRepetitions, ushort pulseWidth, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetPositionCompareParameter == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetPositionCompareParameter not supported");
            }

            InternalSetPositionCompareParameter func = (InternalSetPositionCompareParameter)Marshal.GetDelegateForFunctionPointer(_addressOfSetPositionCompareParameter, typeof(InternalSetPositionCompareParameter));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, (byte)operationalMode, (byte)intervalMode, (byte)directionDependency, intervalWidth, intervalRepetitions, pulseWidth, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetPositionCompareParameter read all parameters for position compare.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="operationalMode">Used operational mode in position sequence mode: 0: Single position mode 1: Position sequence mode</param>
        /// <param name="intervalMode">Used interval mode in position sequence mode:<br></br>0: Interval positions are set in negative direction relative to the position compare reference position<br></br>1: Interval positions are set in positive direction relative to the position compare reference position<br></br>2: Interval positions are set in positive and negative direction relative to the position compare reference position</param>
        /// <param name="directionDependency">Used direction dependency in position sequence mode:<br></br>0: Positions are compared only if actual motor direction is negative<br></br>1: Positions are compared only if actual motor direction is positive<br></br>2: Positions are compared regardless of the actual motor direction</param>
        /// <param name="intervalWidth">This object holds the width of the position intervals</param>
        /// <param name="intervalRepetitions">This object allows to configure the number of position intervals to be considered by position compare</param>
        /// <param name="pulseWidth">This object configures the pulse width of the trigger output</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetPositionCompareParameter(int keyHandle, ushort nodeId, ref EPositionCompareOperationalMode operationalMode, ref EPositionCompareIntervalMode intervalMode, ref EPositionCompareDirectionDependency directionDependency, ref ushort intervalWidth, ref ushort intervalRepetitions, ref ushort pulseWidth, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetPositionCompareParameter == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetPositionCompareParameter not supported");
            }

            InternalGetPositionCompareParameter func = (InternalGetPositionCompareParameter)Marshal.GetDelegateForFunctionPointer(_addressOfGetPositionCompareParameter, typeof(InternalGetPositionCompareParameter));

            byte om = 0;
            byte im = 0;
            byte dd = 0;

            lock (_syncObject)
            {
                result = func(new IntPtr(keyHandle), nodeId, ref om, ref im, ref dd, ref intervalWidth, ref intervalRepetitions, ref pulseWidth, ref errorCode);
            }

            operationalMode = (EPositionCompareOperationalMode)om;
            intervalMode = (EPositionCompareIntervalMode)im;
            directionDependency = (EPositionCompareDirectionDependency)dd;

            return result;
        }

        /// <summary>
        /// VcsActivatePositionCompare enable the output to position compare method.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="digitalOutputNumber">Selected digital output for position compare</param>
        /// <param name="polarity">Polarity of the selected output</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsActivatePositionCompare(int keyHandle, ushort nodeId, ushort digitalOutputNumber, int polarity, ref uint errorCode)
        {
            int result = 0; if (_addressOfActivatePositionCompare == IntPtr.Zero)
            {
                throw new Exception("Function VCS_ActivatePositionCompare not supported");
            }

            InternalActivatePositionCompare func = (InternalActivatePositionCompare)Marshal.GetDelegateForFunctionPointer(_addressOfActivatePositionCompare, typeof(InternalActivatePositionCompare));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, digitalOutputNumber, polarity, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsDeactivatePositionCompare disable the output to position compare method.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="digitalOutputNumber">Selected digital output for position compare</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsDeactivatePositionCompare(int keyHandle, ushort nodeId, ushort digitalOutputNumber, ref uint errorCode)
        {
            int result = 0; if (_addressOfDeactivatePositionCompare == IntPtr.Zero)
            {
                throw new Exception("Function VCS_DeactivatePositionCompare not supported");
            }

            InternalDeactivatePositionCompare func = (InternalDeactivatePositionCompare)Marshal.GetDelegateForFunctionPointer(_addressOfDeactivatePositionCompare, typeof(InternalDeactivatePositionCompare));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, digitalOutputNumber, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsEnablePositionCompare enable the output mask for position compare method.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsEnablePositionCompare(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfEnablePositionCompare == IntPtr.Zero)
            {
                throw new Exception("Function VCS_EnablePositionCompare not supported");
            }

            InternalEnablePositionCompare func = (InternalEnablePositionCompare)Marshal.GetDelegateForFunctionPointer(_addressOfEnablePositionCompare, typeof(InternalEnablePositionCompare));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsDisablePositionCompare disable the output mask from position compare method.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsDisablePositionCompare(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfDisablePositionCompare == IntPtr.Zero)
            {
                throw new Exception("Function VCS_DisablePositionCompare not supported");
            }

            InternalDisablePositionCompare func = (InternalDisablePositionCompare)Marshal.GetDelegateForFunctionPointer(_addressOfDisablePositionCompare, typeof(InternalDisablePositionCompare));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsSetPositionCompareReferencePosition writes the reference position for position compare method.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="referencePosition">This object holds the position that is compared with the position actual value</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetPositionCompareReferencePosition(int keyHandle, ushort nodeId, int referencePosition, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetPositionCompareReferencePosition == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetPositionCompareReferencePosition not supported");
            }

            InternalSetPositionCompareReferencePosition func =
                (InternalSetPositionCompareReferencePosition)
                Marshal.GetDelegateForFunctionPointer(_addressOfSetPositionCompareReferencePosition, typeof(InternalSetPositionCompareReferencePosition));
            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, referencePosition, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsSetPositionMarkerParameter write all parameters for position marker method.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="positionMarkerEdgeType">The value of this object defines on what kind of edge the position should be captured:<br></br>0: Both edges<br></br>1: Rising edge<br></br>2: Falling edge</param>
        /// <param name="positionMarkerMode">This object defines the position markercapturing mode:<br></br>0: Continuous<br></br>1: Single<br></br>2: Multiple</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetPositionMarkerParameter(int keyHandle, ushort nodeId, EPositionMarkerEdgeType positionMarkerEdgeType, EPositionMarkerMode positionMarkerMode, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetPositionMarkerParameter == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetPositionMarkerParameter not supported");
            }

            InternalSetPositionMarkerParameter func = (InternalSetPositionMarkerParameter)Marshal.GetDelegateForFunctionPointer(_addressOfSetPositionMarkerParameter, typeof(InternalSetPositionMarkerParameter));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, (byte)positionMarkerEdgeType, (byte)positionMarkerMode, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetPositionMarkerParameter read all parameters for position marker method.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="positionMarkerEdgeType">The value of this object defines on what kind of edge the position should be captured:<br></br>0: Both edges<br></br>1: Rising edge<br></br>2: Falling edge</param>
        /// <param name="positionMarkerMode">This object defines the position markercapturing mode:<br></br>0: Continuous<br></br>1: Single<br></br>2: Multiple</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetPositionMarkerParameter(int keyHandle, ushort nodeId, ref EPositionMarkerEdgeType positionMarkerEdgeType, ref EPositionMarkerMode positionMarkerMode, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetPositionMarkerParameter == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetPositionMarkerParameter not supported");
            }

            InternalGetPositionMarkerParameter func = (InternalGetPositionMarkerParameter)Marshal.GetDelegateForFunctionPointer(_addressOfGetPositionMarkerParameter, typeof(InternalGetPositionMarkerParameter));
            byte pmet = 0;
            byte pmmm = 0;

        lock (_syncObject)
        {
            result = func(new IntPtr(keyHandle), nodeId, ref pmet, ref pmmm, ref errorCode);
        }

            positionMarkerEdgeType = (EPositionMarkerEdgeType)pmet;
            positionMarkerMode = (EPositionMarkerMode)pmmm;

            return result;
        }

        /// <summary>
        /// VcsActivatePositionMarker enable the digital input to position marker method.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="digitalInputNumber">Selected digital input for position compare</param>
        /// <param name="polarity">Polarity of the selected input</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsActivatePositionMarker(int keyHandle, ushort nodeId, ushort digitalInputNumber, int polarity, ref uint errorCode)
        {
            int result = 0; if (_addressOfActivatePositionMarker == IntPtr.Zero)
            {
                throw new Exception("Function VCS_ActivatePositionMarker not supported");
            }

            InternalActivatePositionMarker func = (InternalActivatePositionMarker)Marshal.GetDelegateForFunctionPointer(_addressOfActivatePositionMarker, typeof(InternalActivatePositionMarker));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, digitalInputNumber, polarity, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsDeactivatePositionMarker disable the digital input to position marker method.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="digitalInputNumber">Selected digital input for position marker</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsDeactivatePositionMarker(int keyHandle, ushort nodeId, ushort digitalInputNumber, ref uint errorCode)
        {
            int result = 0; if (_addressOfDeactivatePositionMarker == IntPtr.Zero)
            {
                throw new Exception("Function VCS_DeactivatePositionMarker not supported");
            }

            InternalDeactivatePositionMarker func = (InternalDeactivatePositionMarker)Marshal.GetDelegateForFunctionPointer(_addressOfDeactivatePositionMarker, typeof(InternalDeactivatePositionMarker));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, digitalInputNumber, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsReadPositionMarkerCounter returns the number of the detected edges.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="count">This object counts the number of the detected edges.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsReadPositionMarkerCounter(int keyHandle, ushort nodeId, ref ushort count, ref uint errorCode)
        {
            int result = 0; if (_addressOfReadPositionMarkerCounter == IntPtr.Zero)
            {
                throw new Exception("Function VCS_ReadPositionMarkerCounter not supported");
            }

            InternalReadPositionMarkerCounter func = (InternalReadPositionMarkerCounter)Marshal.GetDelegateForFunctionPointer(_addressOfReadPositionMarkerCounter, typeof(InternalReadPositionMarkerCounter));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref count, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsReadPositionMarkerCapturedPosition returns the last captured position.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="counterIndex">0: Read position marker captured position<br></br>1: Read position marker history [1]<br></br>2: Read position marker history [2]</param>
        /// <param name="capturedPosition">This object contains the captured position or the position marker history</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsReadPositionMarkerCapturedPosition(int keyHandle, ushort nodeId, ushort counterIndex, ref int capturedPosition, ref uint errorCode)
        {
            int result = 0; if (_addressOfReadPositionMarkerCapturedPosition == IntPtr.Zero)
            {
                throw new Exception("Function VCS_ReadPositionMarkerCapturedPosition not supported");
            }

            InternalReadPositionMarkerCapturedPosition func = (InternalReadPositionMarkerCapturedPosition)Marshal.GetDelegateForFunctionPointer(_addressOfReadPositionMarkerCapturedPosition, typeof(InternalReadPositionMarkerCapturedPosition));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, counterIndex, ref capturedPosition, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsResetPositionMarkerCounter clears the counter and the captured positions by writing zero to object position marker counter.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsResetPositionMarkerCounter(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfResetPositionMarkerCounter == IntPtr.Zero)
            {
                throw new Exception("Function VCS_ResetPositionMarkerCounter not supported");
            }

            InternalResetPositionMarkerCounter func = (InternalResetPositionMarkerCounter)Marshal.GetDelegateForFunctionPointer(_addressOfResetPositionMarkerCounter, typeof(InternalResetPositionMarkerCounter));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsSetRecorderParameter write parameters for data recorder.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="samplingPeriod">Sampling Period as a multiple of the current regulator cycle (n-times 0.1ms)</param>
        /// <param name="numberOfPrecedingSamples">Number of preceding samples (data history).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSetRecorderParameter(int keyHandle, ushort nodeId, ushort samplingPeriod, ushort numberOfPrecedingSamples, ref uint errorCode)
        {
            int result = 0; if (_addressOfSetRecorderParameter == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SetRecorderParameter not supported");
            }

            InternalSetRecorderParameter func = (InternalSetRecorderParameter)Marshal.GetDelegateForFunctionPointer(_addressOfSetRecorderParameter, typeof(InternalSetRecorderParameter));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, samplingPeriod, numberOfPrecedingSamples, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsGetRecorderParameter read parameters for data recorder.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="samplingPeriod">Sampling Period as a multiple of the current regulator cycle (n-times 0.1ms)</param>
        /// <param name="numberOfPrecedingSamples">Number of preceding samples</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetRecorderParameter(int keyHandle, ushort nodeId, ref ushort samplingPeriod, ref ushort numberOfPrecedingSamples, ref uint errorCode)
        {
            int result = 0; if (_addressOfGetRecorderParameter == IntPtr.Zero)
            {
                throw new Exception("Function VCS_GetRecorderParameter not supported");
            }

            InternalGetRecorderParameter func = (InternalGetRecorderParameter)Marshal.GetDelegateForFunctionPointer(_addressOfGetRecorderParameter, typeof(InternalGetRecorderParameter));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref samplingPeriod, ref numberOfPrecedingSamples, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsEnableTrigger connect trigger(-s) for data recording.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="triggerType">Configuration of Auto Trigger functions. If a bit is write as one the trigger is activated:<br></br>1 (bit 0 = 1): Trigger at movement start<br></br>2 (bit 1 = 1): Trigger by error state 4<br></br>(bit 2 = 1): Trigger at digital input edge 8<br></br>(bit 3 = 1): Trigger at end of profile It is possible to activate more than one trigger at the same time.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsEnableTrigger(int keyHandle, ushort nodeId, EDataRecorderTriggerType triggerType, ref uint errorCode)
        {
            int result = 0; if (_addressOfEnableTrigger == IntPtr.Zero)
            {
                throw new Exception("Function VCS_EnableTrigger not supported");
            }

            InternalEnableTrigger func = (InternalEnableTrigger)Marshal.GetDelegateForFunctionPointer(_addressOfEnableTrigger, typeof(InternalEnableTrigger));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, (byte)triggerType, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsDisableAllTrigger set data recorder configuration for triggers to zero.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsDisableAllTriggers(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfDisableAllTriggers == IntPtr.Zero)
            {
                throw new Exception("Function VCS_DisableAllTriggers not supported");
            }

            InternalDisableAllTriggers func = (InternalDisableAllTriggers)Marshal.GetDelegateForFunctionPointer(_addressOfDisableAllTriggers, typeof(InternalDisableAllTriggers));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsActivateChannel connect object for data recording. Start with channel number one!
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="channelNumber">Channel number [1 … 4]</param>
        /// <param name="objectIndex">Object index for data recording</param>
        /// <param name="objectSubIndex">Object sub index for data recording</param>
        /// <param name="objectSize">Object size for data recording</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsActivateChannel(int keyHandle, ushort nodeId, byte channelNumber, ushort objectIndex, byte objectSubIndex, byte objectSize, ref uint errorCode)
        {
            int result = 0; if (_addressOfActivateChannel == IntPtr.Zero)
            {
                throw new Exception("Function VCS_ActivateChannel not supported");
            }

            InternalActivateChannel func = (InternalActivateChannel)Marshal.GetDelegateForFunctionPointer(_addressOfActivateChannel, typeof(InternalActivateChannel));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, channelNumber, objectIndex, objectSubIndex, objectSize, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsDeactivateAllChannel disconnect all data recording objects.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsDeactivateAllChannels(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfDeactivateAllChannels == IntPtr.Zero)
            {
                throw new Exception("Function VCS_DeactivateAllChannels not supported");
            }

            InternalDeactivateAllChannels func = (InternalDeactivateAllChannels)Marshal.GetDelegateForFunctionPointer(_addressOfDeactivateAllChannels, typeof(InternalDeactivateAllChannels));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsStartRecorder starts the data recording.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsStartRecorder(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfStartRecorder == IntPtr.Zero)
            {
                throw new Exception("Function VCS_StartRecorder not supported");
            }

            InternalStartRecorder func = (InternalStartRecorder)Marshal.GetDelegateForFunctionPointer(_addressOfStartRecorder, typeof(InternalStartRecorder));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsStopRecorder stops the data recording.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsStopRecorder(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfStopRecorder == IntPtr.Zero)
            {
                throw new Exception("Function VCS_StopRecorder not supported");
            }

            InternalStopRecorder func = (InternalStopRecorder)Marshal.GetDelegateForFunctionPointer(_addressOfStopRecorder, typeof(InternalStopRecorder));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsForceTrigger forces the data recording triggers.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsForceTrigger(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfForceTrigger == IntPtr.Zero)
            {
                throw new Exception("Function VCS_ForceTrigger not supported");
            }

            InternalForceTrigger func = (InternalForceTrigger)Marshal.GetDelegateForFunctionPointer(_addressOfForceTrigger, typeof(InternalForceTrigger));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsIsRecorderRunning returns data recorder status running.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="running">The p running.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsIsRecorderRunning(int keyHandle, ushort nodeId, ref int running, ref uint errorCode)
        {
            int result = 0; if (_addressOfIsRecorderRunning == IntPtr.Zero)
            {
                throw new Exception("Function VCS_IsRecorderRunning not supported");
            }

            InternalIsRecorderRunning func = (InternalIsRecorderRunning)Marshal.GetDelegateForFunctionPointer(_addressOfIsRecorderRunning, typeof(InternalIsRecorderRunning));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref running, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsIsRecorderTriggered returns data recorder status triggered.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="triggered">The p triggered.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsIsRecorderTriggered(int keyHandle, ushort nodeId, ref int triggered, ref uint errorCode)
        {
            int result = 0; if (_addressOfIsRecorderTriggered == IntPtr.Zero)
            {
                throw new Exception("Function VCS_IsRecorderTriggered not supported");
            }

            InternalIsRecorderTriggered func = (InternalIsRecorderTriggered)Marshal.GetDelegateForFunctionPointer(_addressOfIsRecorderTriggered, typeof(InternalIsRecorderTriggered));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref triggered, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsReadChannelVectorSize returns the maximal number of samples per variable. This parameter is dynamically calculated by the data recorder.</summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="vectorSize">Maximal number of samples per variable.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsReadChannelVectorSize(int keyHandle, ushort nodeId, ref uint vectorSize, ref uint errorCode)
        {
            int result = 0; if (_addressOfReadChannelVectorSize == IntPtr.Zero)
            {
                throw new Exception("Function VCS_ReadChannelVectorSize not supported");
            }

            InternalReadChannelVectorSize func = (InternalReadChannelVectorSize)Marshal.GetDelegateForFunctionPointer(_addressOfReadChannelVectorSize, typeof(InternalReadChannelVectorSize));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref vectorSize, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsReadChannelDataVector returns the data points of a selected channel.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="channelNumber">Selected channel</param>
        /// <param name="dataVector">Data points of selected channel</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static unsafe int VcsReadChannelDataVector(int keyHandle, ushort nodeId, byte channelNumber, ref byte[] dataVector, ref uint errorCode)
        {
            int result = 0; if (_addressOfReadChannelDataVector == IntPtr.Zero)
            {
                throw new Exception("Function VCS_ReadChannelDataVector not supported");
            }

            InternalReadChannelDataVector func = (InternalReadChannelDataVector)Marshal.GetDelegateForFunctionPointer(_addressOfReadChannelDataVector, typeof(InternalReadChannelDataVector));

        lock (_syncObject)
        {
            fixed (byte* dataBuffer = dataVector)
            {
                result = func(new IntPtr(keyHandle), nodeId, channelNumber, dataBuffer, (uint)dataVector.Length, ref errorCode);
            }
        }

            return result;
        }

        /// <summary>
        /// VcsShowChannelDataDlg opens the dialog to show the data channel(-s).
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsShowChannelDataDlg(int keyHandle, ushort nodeId, ref uint errorCode)
        {
            int result = 0; if (_addressOfShowChannelDataDlg == IntPtr.Zero)
            {
                throw new Exception("Function VCS_ShowChannelDataDlg not supported");
            }

            InternalShowChannelDataDlg func = (InternalShowChannelDataDlg)Marshal.GetDelegateForFunctionPointer(_addressOfShowChannelDataDlg, typeof(InternalShowChannelDataDlg));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsExportChannelDataToFile saves data point in a file.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsExportChannelDataToFile(int keyHandle, ushort nodeId, string fileName, ref uint errorCode)
        {
            int result = 0; if (_addressOfExportChannelDataToFile == IntPtr.Zero)
            {
                throw new Exception("Function VCS_ExportChannelDataToFile not supported");
            }

            InternalExportChannelDataToFile func = (InternalExportChannelDataToFile)Marshal.GetDelegateForFunctionPointer(_addressOfExportChannelDataToFile, typeof(InternalExportChannelDataToFile));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, fileName, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsReadDataBuffer returns the buffer data points.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="dataBuffer">Data points</param>
        /// <param name="bufferSizeRead">Size of read data buffer</param>
        /// <param name="vectorStartOffset">Offset to the start of the recorded data vector within the ring buffer.</param>
        /// <param name="maxNbOfSamples">Maximal number of samples per variable.</param>
        /// <param name="numberOfRecordedSamples">Number of recorded samples.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static unsafe int VcsReadDataBuffer(int keyHandle, ushort nodeId, ref byte[] dataBuffer, ref uint bufferSizeRead, ref ushort vectorStartOffset, ref ushort maxNbOfSamples, ref ushort numberOfRecordedSamples, ref uint errorCode)
        {
            int result = 0; if (_addressOfReadDataBuffer == IntPtr.Zero)
            {
                throw new Exception("Function VCS_ReadDataBuffer not supported");
            }

            InternalReadDataBuffer func = (InternalReadDataBuffer)Marshal.GetDelegateForFunctionPointer(_addressOfReadDataBuffer, typeof(InternalReadDataBuffer));

            lock (_syncObject)
            {
            fixed (byte* buffer = dataBuffer)
            {
                result = func(new IntPtr(keyHandle), nodeId, buffer, (uint)dataBuffer.Length, ref bufferSizeRead, ref vectorStartOffset, ref maxNbOfSamples, ref numberOfRecordedSamples, ref errorCode);
            }
            }

            return result;
        }

        /// <summary>
        /// VcsExtractChannelDataVector" returns the vector of one data channel.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="channelNumber">Selected channel</param>
        /// <param name="dataBuffer">Data points</param>
        /// <param name="bufferSize">Buffer size</param>
        /// <param name="dataVector">Data points of the channel</param>
        /// <param name="vectorSize">Vector size</param>
        /// <param name="vectorStartOffset">Offset to the start of the recorded data vector within the ring buffer.</param>
        /// <param name="maxNbOfSamples">Maximal number of samples per variable.</param>
        /// <param name="numberOfRecordedSamples">Number of recorded samples.</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static unsafe int VcsExtractChannelDataVector(int keyHandle, ushort nodeId, byte channelNumber, ref byte[] dataBuffer, uint bufferSize, ref byte[] dataVector, uint vectorSize, ushort vectorStartOffset, ushort maxNbOfSamples, ushort numberOfRecordedSamples, ref uint errorCode)
        {
            int result = 0; if (_addressOfExtractChannelDataVector == IntPtr.Zero)
            {
                throw new Exception("Function VCS_ExtractChannelDataVector not supported");
            }

            InternalExtractChannelDataVector func = (InternalExtractChannelDataVector)Marshal.GetDelegateForFunctionPointer(_addressOfExtractChannelDataVector, typeof(InternalExtractChannelDataVector));

        lock (_syncObject)
        {

            fixed (byte* buffer = dataBuffer)
            {
                fixed (byte* vector = dataVector)
                {
                    result = func(new IntPtr(keyHandle), nodeId, channelNumber, buffer, bufferSize, vector, vectorSize, vectorStartOffset, maxNbOfSamples, numberOfRecordedSamples, ref errorCode);
                }
            }
    }
            return result;
        }

        /// <summary>
        /// VcsSendCANFrame sends a general CAN Frame to the CAN bus.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="cobId">CAN Frame 11-bit Identifier</param>
        /// <param name="length">CAN Frame Data Length Code (DLC)</param>
        /// <param name="data">CAN Frame Data</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSendCanFrame(int keyHandle, ushort cobId, ushort length, [MarshalAs(UnmanagedType.LPArray)] byte[] data, ref uint errorCode)
        {
            int result = 0; if (_addressOfSendCanFrame == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SendCANFrame not supported");
            }

            InternalSendCanFrame func = (InternalSendCanFrame)Marshal.GetDelegateForFunctionPointer(_addressOfSendCanFrame, typeof(InternalSendCanFrame));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), cobId, length, data, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsReadCANFrame reads a general CAN Frame from the CAN bus.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="cobId">CAN Frame 11-bit Identifier</param>
        /// <param name="length">CAN Frame Data Length Code (DLC)</param>
        /// <param name="data">CAN Frame Data</param>
        /// <param name="timeout">Max. wait time</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsReadCanFrame(int keyHandle, ushort cobId, ushort length, [MarshalAs(UnmanagedType.LPArray)] byte[] data, uint timeout, ref uint errorCode)
        {
            int result = 0; if (_addressOfReadCanFrame == IntPtr.Zero)
            {
                throw new Exception("Function VCS_ReadCANFrame not supported");
            }

            InternalReadCanFrame func = (InternalReadCanFrame)Marshal.GetDelegateForFunctionPointer(_addressOfReadCanFrame, typeof(InternalReadCanFrame));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), cobId, length, data, timeout, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsRequestCANFrame requests a general CAN Frame from the CAN bus using Remote Transmit Request (RTR).
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="cobId">CAN Frame 11-bit Identifier</param>
        /// <param name="length">CAN Frame Data Length Code (DLC)</param>
        /// <param name="data">CAN Frame Data</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsRequestCanFrame(int keyHandle, ushort cobId, ushort length, [MarshalAs(UnmanagedType.LPArray)] byte[] data, ref uint errorCode)
        {
            int result = 0; if (_addressOfRequestCanFrame == IntPtr.Zero)
            {
                throw new Exception("Function VCS_RequestCANFrame not supported");
            }

            InternalRequestCanFrame func = (InternalRequestCanFrame)Marshal.GetDelegateForFunctionPointer(_addressOfRequestCanFrame, typeof(InternalRequestCanFrame));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), cobId, length, data, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsSendNMTService is used to send a NMT protocol from a master to one slave/all slaves in a network. Command is without acknowledge.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="commandSpecifier">Following NMT services are available:<br></br>   1 Start Remote Node<br></br>   2 Stop Remote Node<br></br>   128 Enter Pre-Operational<br></br>   129 Reset Node<br></br>   130 Reset Communication</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsSendNmtService(int keyHandle, ushort nodeId, ECommandSpecifier commandSpecifier, ref uint errorCode)
        {
            int result = 0; if (_addressOfSendNmtService == IntPtr.Zero)
            {
                throw new Exception("Function VCS_SendNMTService not supported");
            }

            InternalSendNmtService func = (InternalSendNmtService)Marshal.GetDelegateForFunctionPointer(_addressOfSendNmtService, typeof(InternalSendNmtService));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, (ushort)commandSpecifier, ref errorCode); }
            return result;
        }

        /// <summary>
        /// VcsWaitForTargetReached is waiting until the state is changed to target reached or the time is up.
        /// </summary>
        /// <param name="keyHandle">Handle for port access.</param>
        /// <param name="nodeId">Node ID of the addressed device. ID is given from hardware switches or the layer setting services (LSS).</param>
        /// <param name="timeout">Max. wait time until target reached</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsWaitForTargetReached(int keyHandle, ushort nodeId, uint timeout, ref uint errorCode)
        {
            int result = 0; if (_addressOfWaitForTargetReached == IntPtr.Zero)
            {
                throw new Exception("Function VCS_WaitForTargetReached not supported");
            }

            InternalWaitForTargetReached func = (InternalWaitForTargetReached)Marshal.GetDelegateForFunctionPointer(_addressOfWaitForTargetReached, typeof(InternalWaitForTargetReached));

            lock (_syncObject) { result = func(new IntPtr(keyHandle), nodeId, timeout, ref errorCode); }
            return result;
        }

        #endregion

        #region Wrappers

        #region Version Info

        /// <summary>
        /// VcsGetDriverInfo returns the name and version from the Windows DLL.
        /// </summary>
        /// <param name="libraryName">Name from DLL</param>
        /// <param name="libraryVersion">Version from DLL</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetDriverInfo(ref string libraryName, ref string libraryVersion, ref uint errorCode)
        {
            const ushort MaxSize = 100;

            StringBuilder bufferName = new StringBuilder(MaxSize);
            StringBuilder bufferVersion = new StringBuilder(MaxSize);
            int result;

            if ((result = VcsGetDriverInfo(bufferName, MaxSize, bufferVersion, MaxSize, ref errorCode)) == 1)
            {
                libraryName = bufferName.ToString();
                libraryVersion = bufferVersion.ToString();
            }

            return result;
        }

        #endregion

        #region Advanced Functions

        /// <summary>
        /// VcsGetDeviceNameSelection returns all available device names.
        /// </summary>
        /// <param name="startOfSelection">True: Get first selection string False: Get next selection string</param>
        /// <param name="deviceNameSel">Device name</param>
        /// <param name="endOfSelection">1: No more selection string available 0: More string available</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetDeviceNameSelection(int startOfSelection, ref string deviceNameSel, ref int endOfSelection, ref uint errorCode)
        {
            if (deviceNameSel == null)
            {
                throw new ArgumentNullException("deviceNameSel");
            }

            const ushort MaxSize = 100;

            byte[] buffer = new byte[MaxSize];

            int result = VcsGetDeviceNameSelection(startOfSelection, ref buffer, MaxSize, ref endOfSelection, ref errorCode);

            deviceNameSel = string.Empty;
            foreach (byte b in buffer)
            {
                if (b == 0)
                {
                    break;
                }

                deviceNameSel += (char)b;
            }

            return result;
        }

        /// <summary>
        /// VcsGetProtocolStackNameSelection returns all available protocol stack names.
        /// </summary>
        /// <param name="deviceName">Device Name</param>
        /// <param name="startOfSelection">
        /// 1: Get first selection value<br></br>
        /// 0: Get next selection value
        /// </param>
        /// <param name="protocolStackNameSel">Available protocol stack name</param>
        /// <param name="endOfSelection">
        /// 1: No more string available<br></br>
        /// 0: More string available
        /// </param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetProtocolStackNameSelection(string deviceName, int startOfSelection, ref string protocolStackNameSel, ref int endOfSelection, ref uint errorCode)
        {
            const ushort MaxSize = 100;

            StringBuilder buffer = new StringBuilder(MaxSize);
            int result;

            if ((result = VcsGetProtocolStackNameSelection(deviceName, startOfSelection, buffer, MaxSize, ref endOfSelection, ref errorCode)) == 1)
            {
                protocolStackNameSel = buffer.ToString();
            }

            return result;
        }

        /// <summary>
        /// VcsGetInterfaceNameSelection returns all available interface names.
        /// </summary>
        /// <param name="deviceName">Device name</param>
        /// <param name="protocolStackName">Protocol stack name</param>
        /// <param name="startOfSelection">
        /// 1: Get first selection value<br></br>
        /// 0: Get next selection value
        /// </param>
        /// <param name="interfaceNameSel">Interface name</param>
        /// <param name="endOfSelection">
        /// 1: No more string available<br></br>
        /// 0: More string available
        /// </param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetInterfaceNameSelection(string deviceName, string protocolStackName, int startOfSelection, ref string interfaceNameSel, ref int endOfSelection, ref uint errorCode)
        {
            const ushort MaxSize = 100;

            StringBuilder buffer = new StringBuilder(MaxSize);
            int result;

            if ((result = VcsGetInterfaceNameSelection(deviceName, protocolStackName, startOfSelection, buffer, MaxSize, ref endOfSelection, ref errorCode)) == 1)
            {
                interfaceNameSel = buffer.ToString();
            }

            return result;
        }

        /// <summary>
        /// VcsGetPortNameSelection returns all available port names.
        /// </summary>
        /// <param name="deviceName">Device name</param>
        /// <param name="protocolStackName">Protocol stack name</param>
        /// <param name="interfaceName">Interface name</param>
        /// <param name="startOfSelection">
        /// 1: Get first selection string<br></br>
        /// 0: Get next selection string</param>
        /// <param name="portNameSel">Port name</param>
        /// <param name="endOfSelection">
        /// 1: No more string available<br></br>
        /// 0: More string available
        /// </param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>1 if successful, 0 otherwise
        /// </returns>
        public static int VcsGetPortNameSelection(string deviceName, string protocolStackName, string interfaceName, int startOfSelection, ref string portNameSel, ref int endOfSelection, ref uint errorCode)
        {
            const ushort MaxSize = 100;
            int result = 0;

            lock (_syncObject)
            {
                StringBuilder buffer = new StringBuilder(MaxSize);
            
                if ((result = VcsGetPortNameSelection(deviceName, protocolStackName, interfaceName, startOfSelection, buffer, MaxSize, ref endOfSelection, ref errorCode)) == 1)
                {
                    portNameSel = buffer.ToString();
                }
            }

            return result;
        }

        /// <summary>
        /// VcsResetPortNameSelection reinitializes the port enumeration.
        /// </summary>
        /// <param name="deviceName">Device name</param>
        /// <param name="protocolStackName">Protocol stack name</param>
        /// <param name="interfaceName">Interface name</param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>1 if successful, 0 otherwise
        /// </returns>
        public static int VcsResetPortNameSelection(string deviceName, string protocolStackName, string interfaceName, ref uint errorCode)
        {
            int result = 0; if (_addressOfResetPortNameSelection == IntPtr.Zero)
            {
                throw new Exception("Function VCS_ResetPortNameSelection not supported");
            }

            InternalResetPortNameSelection func = (InternalResetPortNameSelection)Marshal.GetDelegateForFunctionPointer(_addressOfResetPortNameSelection, typeof(InternalResetPortNameSelection));

            lock(_syncObject)
            {
                result = func(deviceName, protocolStackName, interfaceName, ref errorCode);
            }

            return result;
        }

        /// <summary>
        /// VcsGetBaudrateSelection returns all available baud rates for the connected port.
        /// </summary>
        /// <param name="deviceName">Device name</param>
        /// <param name="protocolStackName">Protocol stack name</param>
        /// <param name="interfaceName">Interface name</param>
        /// <param name="portName">Port name</param>
        /// <param name="startOfSelection">
        ///  1: Get first selection value<br></br>
        ///  0: Get next selection value
        /// </param>
        /// <param name="baudrateSel">Baud rate [Bit/s]</param>
        /// <param name="endOfSelection">
        ///  1: No more value available<br></br>
        ///  0: More value available
        /// </param>
        /// <param name="errorCode">Error information on the executed function.</param>
        /// <returns>Nonzero if successful; otherwise “0”.</returns>
        public static int VcsGetBaudrateSelection(string deviceName, string protocolStackName, string interfaceName, string portName, int startOfSelection, ref uint baudrateSel, ref int endOfSelection, ref uint errorCode)
        {
            return VcsGetBaudrateSelectionInt(deviceName, protocolStackName, interfaceName, portName, startOfSelection, ref baudrateSel, ref endOfSelection, ref errorCode);
        }

        #endregion

        #endregion

        #region Helpers

// ReSharper disable UnusedMember.Local
        private static void ConvertByteArrayToString(byte[] data)
// ReSharper restore UnusedMember.Local
        {
            string text = string.Empty;
            ConvertByteArrayToString(data, ref text);
        }

        /// <summary>
        /// Converts the byte array to string.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="text">The text.</param>
        private static void ConvertByteArrayToString(byte[] data, ref string text)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            ASCIIEncoding enc = new ASCIIEncoding();
            string resText = enc.GetString(data);
            text = resText.Replace('\0'.ToString(), string.Empty);
        }

        #endregion
    }
}