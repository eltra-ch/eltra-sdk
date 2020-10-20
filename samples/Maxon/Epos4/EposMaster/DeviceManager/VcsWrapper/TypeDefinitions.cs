namespace EposMaster.DeviceManager.VcsWrapper
{
    /// <summary>
    /// Kind of dialog mode
    /// </summary>
    public enum EDialogMode
    {
        /// <summary>
        /// Show Progress Dialog
        /// </summary>
        DmProgressDlg = 0,

        /// <summary>
        /// Show Progress and Confirmation Dialog
        /// </summary>
        DmProgressAndConfirmDlg = 1,

        /// <summary>
        /// Show Confirmation Dialog
        /// </summary>
        DmConfirmDlg = 2,

        /// <summary>
        /// Dont' show any Dialog
        /// </summary>
        DmNoDlg = 3,
    }

    /// <summary>
    /// Kind of motor type
    /// </summary>
    public enum EMotorType
    {
        /// <summary>
        /// Brushed DC motor
        /// </summary>
        MtDcMotor = 1,

        /// <summary>
        /// EC motor sinus commutated
        /// </summary>
        MtEcSinusCommutatedMotor = 10,

        /// <summary>
        /// EC motor block commutated
        /// </summary>
        MtEcBlockCommutatedMotor = 11
    }

    /// <summary>
    /// Kind of position sensor type
    /// </summary>
    public enum ESensorType
    {
        /// <summary>
        /// Unknown sensor (undefined)
        /// </summary>
        StUnknown = 0,

        /// <summary>
        /// No sensor
        /// </summary>
        StNone = 0,

        /// <summary>
        /// Incremental Encoder 1 with index (3-channel)
        /// </summary>
        StIncEncoder3Channel = 1,

        /// <summary>
        /// Incremental Encoder 1 without index (2-channel)
        /// </summary>
        StIncEncoder2Channel = 2,

        /// <summary>
        /// Hall Sensors
        /// </summary>
        StHallSensors = 3,

        /// <summary>
        /// SSI Encoder binary coded
        /// </summary>
        StSsiAbsEncoderBinary = 4,

        /// <summary>
        /// SSI Encoder grey coded
        /// </summary>
        StSsiAbsEncoderGrey = 5,

        /// <summary>
        /// Incremental Encoder 2 with index (3-channel)
        /// </summary>
        StIncEncoder23Channel = 6,

        /// <summary>
        /// Incremental Encoder 2 without index (2-channel)
        /// </summary>
        StIncEncoder22Channel = 7,

        /// <summary>
        /// Analog Incremental Encoder with index (3-channel)
        /// </summary>
        StAnalogIncEncoder3Channel = 8,

        /// <summary>
        /// Analog Incremental Encoder without index (2-channel)
        /// </summary>
        StAnalogIncEncoder2Channel = 9
    }

    /// <summary>
    /// Kind of digital input configuration
    /// </summary>
    public enum EDigitalInputConfiguration
    {
        /// <summary>
        /// Negative Limit Switch
        /// </summary>
        DicNegativeLimitSwitch = 0,

        /// <summary>
        /// Positive Limit Switch
        /// </summary>
        DicPositiveLimitSwitch = 1,

        /// <summary>
        /// Home Switch
        /// </summary>
        DicHomeSwitch = 2,

        /// <summary>
        /// Position Marker
        /// </summary>
        DicPositionMarker = 3,

        /// <summary>
        /// Drive Enable
        /// </summary>
        DicDriveEnable = 4,

        /// <summary>
        /// Quick Stop
        /// </summary>
        DicQuickStop = 5,

        /// <summary>
        /// General Purpose J
        /// </summary>
        DicGeneralPurposeJ = 6,

        /// <summary>
        /// General Purpose I
        /// </summary>
        DicGeneralPurposeI = 7,

        /// <summary>
        /// General Purpose H
        /// </summary>
        DicGeneralPurposeH = 8,

        /// <summary>
        /// General Purpose G
        /// </summary>
        DicGeneralPurposeG = 9,

        /// <summary>
        /// General Purpose F
        /// </summary>
        DicGeneralPurposeF = 10,

        /// <summary>
        /// General Purpose E
        /// </summary>
        DicGeneralPurposeE = 11,

        /// <summary>
        /// General Purpose D
        /// </summary>
        DicGeneralPurposeD = 12,

        /// <summary>
        /// General Purpose C
        /// </summary>
        DicGeneralPurposeC = 13,

        /// <summary>
        /// General Purpose B
        /// </summary>
        DicGeneralPurposeB = 14,

        /// <summary>
        /// General Purpose A
        /// </summary>
        DicGeneralPurposeA = 15,

        /// <summary>
        /// Touch Probe 1
        /// </summary>
        DicTouchProbe1 = 26,

        /// <summary>
        /// No Functionality
        /// </summary>
        DicNoFunctionality = 255
    }

    /// <summary>
    /// Kind of digital output configuration
    /// </summary>
    public enum EDigitalOutputConfiguration
    {
        /// <summary>
        /// Ready Fault
        /// </summary>
        DocReadyFault = 0,

        /// <summary>
        /// Position Compare
        /// </summary>
        DocPositionCompare = 1,

        /// <summary>
        /// General Purpose H
        /// </summary>
        DocGeneralPurposeH = 8,

        /// <summary>
        /// General Purpose G
        /// </summary>
        DocGeneralPurposeG = 9,

        /// <summary>
        /// General Purpose F
        /// </summary>
        DocGeneralPurposeF = 10,

        /// <summary>
        /// General Purpose E
        /// </summary>
        DocGeneralPurposeE = 11,

        /// <summary>
        /// General Purpose D
        /// </summary>
        DocGeneralPurposeD = 12,

        /// <summary>
        /// General Purpose C
        /// </summary>
        DocGeneralPurposeC = 13,

        /// <summary>
        /// General Purpose B
        /// </summary>
        DocGeneralPurposeB = 14,

        /// <summary>
        /// General Purpose A
        /// </summary>
        DocGeneralPurposeA = 15,

        /// <summary>
        /// No functionality
        /// </summary>
        DocNoFunctionality = 255
    }

    /// <summary>
    /// Kind of analog input configuration
    /// </summary>
    public enum EAnalogInputConfiguration
    {
        /// <summary>
        /// Analog Current Setpoint
        /// </summary>
        AicAnalogCurrentSetpoint = 0,

        /// <summary>
        /// Analog Velocity Setpoint
        /// </summary>
        AicAnalogVelocitySetpoint = 1,

        /// <summary>
        /// Analog Position Setpoint
        /// </summary>
        AicAnalogPositionSetpoint = 2,

        /// <summary>
        /// Analog General Purpose H
        /// </summary>
        AicAnalogGeneralPurposeH = 8,

        /// <summary>
        /// Analog General Purpose G
        /// </summary>
        AicAnalogGeneralPurposeG = 9,

        /// <summary>
        /// Analog General Purpose F
        /// </summary>
        AicAnalogGeneralPurposeF = 10,

        /// <summary>
        /// Analog General Purpose E
        /// </summary>
        AicAnalogGeneralPurposeE = 11,

        /// <summary>
        /// Analog General Purpose D
        /// </summary>
        AicAnalogGeneralPurposeD = 12,

        /// <summary>
        /// Analog General Purpose C
        /// </summary>
        AicAnalogGeneralPurposeC = 13,

        /// <summary>
        /// Analog General Purpose B
        /// </summary>
        AicAnalogGeneralPurposeB = 14,

        /// <summary>
        /// Analog General Purpose A
        /// </summary>
        AicAnalogGeneralPurposeA = 15
    }

    /// <summary>
    /// Kind of analog input configuration
    /// </summary>
    public enum EAnalogOutputConfiguration
    {
        /// <summary>
        /// Analog General Purpose A
        /// </summary>
        AocAnalogGeneralPurposeA = 0,

        /// <summary>
        /// Analog General Purpose B
        /// </summary>
        AocAnalogGeneralPurposeB = 1,

        /// <summary>
        /// No functionality
        /// </summary>
        AocNoFunctionality = 255
    }

    /// <summary>
    /// Velocity dimension index
    /// </summary>
    public enum EVelocityDimension
    {
        /// <summary>
        /// Velocity dimension rpm
        /// </summary>
        VdRpm = 0xA4
    }

    /// <summary>
    /// Velocity notation index
    /// </summary>
    public enum EVelocityNotation
    {
        /// <summary>
        /// Velocity notation standard
        /// </summary>
        VnStandard = 0,

        /// <summary>
        /// Velocity notation deci
        /// </summary>
        VnDeci = -1,

        /// <summary>
        /// Velocity notation centi
        /// </summary>
        VnCenti = -2,

        /// <summary>
        /// Velocity notation milli
        /// </summary>
        VnMilli = -3
    }

    /// <summary>
    /// Kind of operation mode
    /// </summary>
    public enum EOperationMode
    {
        /// <summary>
        /// Profile Position Mode
        /// </summary>
        OmdProfilePositionMode = 1,

        /// <summary>
        /// Profile Velocity Mode
        /// </summary>
        OmdProfileVelocityMode = 3,

        /// <summary>
        /// Homing Mode
        /// </summary>
        OmdHomingMode = 6,

        /// <summary>
        /// Interpolated Position Mode
        /// </summary>
        OmdInterpolatedPositionMode = 7,

        /// <summary>
        /// Cyclic Synchronous Position Mode
        /// </summary>
        OmdCyclicSynchronousPositionMode = 8,

        /// <summary>
        /// Cyclic Synchronous Velocity Mode
        /// </summary>
        OmdCyclicSynchronousVelocityMode = 9,

        /// <summary>
        /// Cyclic Synchronous Torque Mode
        /// </summary>
        OmdCyclicSyncronicTorqueMode = 10,

        /// <summary>
        /// Position Mode
        /// </summary>
        OmdPositionMode = -1,

        /// <summary>
        /// Velocity Mode
        /// </summary>
        OmdVelocityMode = -2,

        /// <summary>
        /// Current Mode
        /// </summary>
        OmdCurrentMode = -3,

        /// <summary>
        /// Master Encoder Mode
        /// </summary>
        OmdMasterEncoderMode = -5,

        /// <summary>
        /// Step Direction Mode
        /// </summary>
        OmdStepDirectionMode = -6
    }

    /// <summary>
    /// Controller state
    /// </summary>
    public enum EStates
    {
        /// <summary>
        /// Disabled State
        /// </summary>
        StDisabled = 0,

        /// <summary>
        /// Enabled State
        /// </summary>
        StEnabled = 1,

        /// <summary>
        /// Quickstop State
        /// </summary>
        StQuickStop = 2,

        /// <summary>
        /// Fault State
        /// </summary>
        StFault = 3
    }

    /// <summary>
    /// Homing method
    /// </summary>
    public enum EHomingMethod
    {
        /// <summary>
        /// Actual Position
        /// </summary>
        HmActualPosition = 35,

        /// <summary>
        /// Negative Limit Switch
        /// </summary>
        HmNegativeLimitSwitch = 17,

        /// <summary>
        /// Negative Limit Switch and Index
        /// </summary>
        HmNegativeLimitSwitchAndIndex = 1,

        /// <summary>
        /// Positive Limit Switch
        /// </summary>
        HmPositiveLimitSwitch = 18,

        /// <summary>
        /// Positive Limit Switch and Index
        /// </summary>
        HmPositiveLimitSwitchAndIndex = 2,

        /// <summary>
        /// Home Switch Positive Speed
        /// </summary>
        HmHomeSwitchPositiveSpeed = 23,

        /// <summary>
        /// Home Switch Positive Speed and Index
        /// </summary>
        HmHomeSwitchPositiveSpeedAndIndex = 7,

        /// <summary>
        /// Home Switch Negative Speed
        /// </summary>
        HmHomeSwitchNegativeSpeed = 27,

        /// <summary>
        /// Home Switch Negative Speed and Index
        /// </summary>
        HmHomeSwitchNegativeSpeedAndIndex = 11,

        /// <summary>
        /// Current Threshold Positive Speed
        /// </summary>
        HmCurrentThresholdPositiveSpeed = -3,

        /// <summary>
        /// Current Threshold Positive Speed and Index
        /// </summary>
        HmCurrentThresholdPositiveSpeedAndIndex = -1,

        /// <summary>
        /// Current Threshold Negative Speed
        /// </summary>
        HmCurrentThresholdNegativeSpeed = -4,

        /// <summary>
        /// Current Threshold Negative Speed and Index
        /// </summary>
        HmCurrentThresholdNegativeSpeedAndIndex = -2,

        /// <summary>
        /// Index Positive Speed
        /// </summary>
        HmIndexPositiveSpeed = 34,

        /// <summary>
        /// Index Negative Speed
        /// </summary>
        HmIndexNegativeSpeed = 33
    }

    /// <summary>
    /// Kind of position marker edge type
    /// </summary>
    public enum EPositionMarkerEdgeType
    {
        /// <summary>
        /// Both edges
        /// </summary>
        PetBothEdges = 0,

        /// <summary>
        /// Rising edge
        /// </summary>
        PetRisingEdge = 1,

        /// <summary>
        /// Falling edge
        /// </summary>
        PetFallingEdge = 2
    }

    /// <summary>
    /// Position marker-capturing mode
    /// </summary>
    public enum EPositionMarkerMode
    {
        /// <summary>
        /// Position marker mode continuous
        /// </summary>
        PmContinuous = 0,

        /// <summary>
        /// Position marker mode single
        /// </summary>
        PmSingle = 1,

        /// <summary>
        /// Position marker mode multiple
        /// </summary>
        PmMultiple = 2,

        /// <summary>
        /// Position marker mode single and Stop
        /// </summary>
        PmSingleAndStop = 3,
    }

    /// <summary>
    /// Position compare operational mode
    /// </summary>
    public enum EPositionCompareOperationalMode
    {
        /// <summary>
        /// Single position mode
        /// </summary>
        PcoSinglePositionMode = 0,

        /// <summary>
        /// Position sequence mode
        /// </summary>
        PcoPositionSequenceMode = 1,

        /// <summary>
        /// Reserved value
        /// </summary>
        PcoReserved = 2
    }

    /// <summary>
    /// Position compare interval mode
    /// </summary>
    public enum EPositionCompareIntervalMode
    {
        /// <summary>
        /// Interval positions are set in negative direction relative to the position compare reference position
        /// </summary>
        PciNegativeDirToRefpos = 0,

        /// <summary>
        /// Interval positions are set in positive direction relative to the position compare reference position
        /// </summary>
        PciPositiveDirToRefpos = 1,

        /// <summary>
        ///  Interval positions are set in positive and negative direction relative to the position compare reference position
        /// </summary>
        PciBothDirToRefpos = 2,

        /// <summary>
        /// Reserved value
        /// </summary>
        PciReserved = 3
    }

    /// <summary>
    /// Position compare direction dependency
    /// </summary>
    public enum EPositionCompareDirectionDependency
    {
        /// <summary>
        ///  Positions are compared only if actual motor direction is negative
        /// </summary>
        PcdMotorDirectionNegative = 0,

        /// <summary>
        /// Positions are compared only if actual motor direction is positive
        /// </summary>
        PcdMotorDirectionPositive = 1,

        /// <summary>
        /// Positions are compared regardless of the actual motor direction
        /// </summary>
        PcdMotorDirectionBoth = 2,

        /// <summary>
        /// Reserved value
        /// </summary>
        PcdReserved = 3
    }

    /// <summary>
    /// Data recorder trigger type
    /// </summary>
    public enum EDataRecorderTriggerType
    {
        /// <summary>
        ///  Movement Trigger
        /// </summary>
        DrMovementTrigger = 1,

        /// <summary>
        /// Error Trigger
        /// </summary>
        DrErrorTrigger = 2,

        /// <summary>
        /// Digital Input Trigger
        /// </summary>
        DrDigitalInputTrigger = 4,

        /// <summary>
        /// Movement End Trigger
        /// </summary>
        DrMovementEndTrigger = 8
    }

    /// <summary>
    /// NMT services command
    /// </summary>
    public enum ECommandSpecifier
    {
        /// <summary>
        /// Start remote node
        /// </summary>
        NcsStartRemoteNode = 1,

        /// <summary>
        /// Stop remote node
        /// </summary>
        NcsStopRemoteNode = 2,

        /// <summary>
        /// Enter Pre-Operational
        /// </summary>
        NcsEnterPreOperational = 128,

        /// <summary>
        /// Reset Node
        /// </summary>
        NcsResetNode = 129,

        /// <summary>
        /// Reset Communication
        /// </summary>
        NcsResetCommunication = 130
    }

    /// <summary>
    /// Controller (regulation tuning)
    /// </summary>
    public enum EController
    {
        /// <summary>
        /// PI current controller
        /// </summary>
        EcPiCurrentController = 1,

        /// <summary>
        /// PI velocity controller
        /// </summary>
        EcPiVelocityController = 10,

        /// <summary>
        /// PI velocity controller with observer
        /// </summary>
        EcPiVelocityControllerWithObserver = 11,

        /// <summary>
        /// PID position controller
        /// </summary>
        EcPidPositionController = 20,

        /// <summary>
        /// Dual loop position controller
        /// </summary>
        EcDualLoopPositionController = 21
    }

    /// <summary>
    /// Gain (regulation tuning)
    /// </summary>
    public enum EGain
    {
        /// <summary>
        /// P gain
        /// </summary>
        EgPGain = 1,

        /// <summary>
        /// I gain
        /// </summary>
        EgIGain = 2,

        /// <summary>
        /// D gain
        /// </summary>
        EgDGain = 3,

        /// <summary>
        /// Feedforward velocity gain
        /// </summary>
        EgFeedforwardVelocityGain = 10,

        /// <summary>
        /// Feedforward acceleration gain
        /// </summary>
        EgFeedforwardAccelerationGain = 11,

        /// <summary>
        /// Observer theta gain
        /// </summary>
        EgObserverThetaGain = 20,

        /// <summary>
        /// Observer omega gain
        /// </summary>
        EgObserverOmegaGain = 21,

        /// <summary>
        /// Observer tau gain
        /// </summary>
        EgObserverTauGain = 22,

        /// <summary>
        /// P gain low
        /// </summary>
        EgPGainLow = 101,

        /// <summary>
        /// P gain high
        /// </summary>
        EgPGainHigh = 102,

        /// <summary>
        /// Gain scheduling weight
        /// </summary>
        EgGainSchedulingWeight = 110,

        /// <summary>
        /// Ffilter coefficient A
        /// </summary>
        EgFilterCoefficientA = 120,

        /// <summary>
        /// Filter coefficient B
        /// </summary>
        EgFilterCoefficientB = 121,

        /// <summary>
        /// Filter coefficient C
        /// </summary>
        EgFilterCoefficientC = 122,

        /// <summary>
        /// Filter coefficient D
        /// </summary>
        EgFilterCoefficientD = 123,

        /// <summary>
        /// Filter coefficient E
        /// </summary>
        EgFilterCoefficientE = 124
    }
}
