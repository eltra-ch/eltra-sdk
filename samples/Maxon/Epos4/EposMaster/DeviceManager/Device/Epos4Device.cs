using EposMaster.DeviceManager.Device.Epos4.Commands;
using EposMaster.DeviceManager.Device.Epos4.Tools;

namespace EposMaster.DeviceManager.Device
{
    sealed class Epos4Device : EposDevice
    {
        #region Constructors

        public Epos4Device(string deviceDescriptionFile, uint updateInterval, uint timeout, int nodeId)
            : base("EPOS4", deviceDescriptionFile, updateInterval, timeout, nodeId)
        {
            CreateCommandSet();
            CreateToolSet();
        }

        #endregion

        #region Methods

        private void CreateCommandSet()
        {            
            //State
            AddCommand(new GetEnableStateCommand(this));
            AddCommand(new SetEnableStateCommand(this));
            AddCommand(new GetDisableStateCommand(this));
            AddCommand(new SetDisableStateCommand(this));
            AddCommand(new GetFaultStateCommand(this));
            AddCommand(new GetQuickStopStateCommand(this));
            AddCommand(new SetQuickStopStateCommand(this));
            AddCommand(new ClearFaultCommand(this));
            
            //Error handling
            AddCommand(new GetErrorInfoCommand(this));
            AddCommand(new GetNbOfDeviceErrorCommand(this));
            
            //Mode
            AddCommand(new SetOperationModeCommand(this));
            AddCommand(new GetOperationModeCommand(this));
            AddCommand(new ActivateHomingModeCommand(this));
            AddCommand(new ActivateCurrentModeCommand(this));
            AddCommand(new ActivateVelocityModeCommand(this));
            AddCommand(new ActivateProfileVelocityModeCommand(this));
            AddCommand(new ActivateProfilePositionModeCommand(this));
            AddCommand(new ActivatePositionModeCommand(this));

            //Current
            AddCommand(new GetCurrentIsCommand(this));
            AddCommand(new GetCurrentIsAveragedCommand(this));
            AddCommand(new GetCurrentMustCommand(this));
            AddCommand(new SetCurrentMustCommand(this));

            //Velocity
            AddCommand(new GetVelocityIsCommand(this));
            AddCommand(new GetVelocityIsAveragedCommand(this));
            AddCommand(new GetVelocityMustCommand(this));
            AddCommand(new SetVelocityMustCommand(this));
            AddCommand(new GetVelocityProfileCommand(this));
            AddCommand(new SetVelocityProfileCommand(this));
            AddCommand(new MoveWithVelocityCommand(this));
            AddCommand(new HaltVelocityMovementCommand(this));
            
            //Position
            AddCommand(new GetPositionIsCommand(this));
            AddCommand(new GetPositionMustCommand(this));
            AddCommand(new SetPositionMustCommand(this));
            AddCommand(new SetPositionProfileCommand(this));
            AddCommand(new MoveToPositionCommand(this));
            AddCommand(new HaltPositionMovementCommand(this));
            AddCommand(new WaitForTargetReachedCommand(this));
           
            //Homing
            AddCommand(new FindHomeCommand(this));
            AddCommand(new DefinePositionCommand(this));
            AddCommand(new GetHomingParameterCommand(this));
            AddCommand(new SetHomingParameterCommand(this));
            AddCommand(new WaitForHomingAttainedCommand(this));
            AddCommand(new StopHomingCommand(this));

            //Motor
            AddCommand(new GetMotorTypeCommand(this));
            AddCommand(new SetMotorTypeCommand(this));

            //Persistence
            AddCommand(new StoreCommand(this));
            AddCommand(new RestoreCommand(this));
            AddCommand(new ResetDeviceCommand(this));

            //State
            AddCommand(new GetStateCommand(this));
            AddCommand(new SetStateCommand(this));

            //Limits
            AddCommand(new GetMaxAccelerationCommand(this));
            AddCommand(new SetMaxAccelerationCommand(this));
            AddCommand(new GetMaxFollowingErrorCommand(this));
            AddCommand(new SetMaxFollowingErrorCommand(this));
            AddCommand(new GetMaxProfileVelocityCommand(this));
            AddCommand(new SetMaxProfileVelocityCommand(this));

            //Digital Inputs
            AddCommand(new GetAllDigitalInputsCommand(this));
            AddCommand(new GetAllDigitalOutputsCommand(this));
            AddCommand(new SetAllDigitalOutputsCommand(this));
            
            //Analog input/output
            AddCommand(new GetAnalogInputCommand(this));
            AddCommand(new SetAnalogOutputCommand(this));

            AddCommand(new GetAnalogInputStateCommand(this));
            AddCommand(new GetAnalogInputVoltageCommand(this));

            AddCommand(new SetAnalogOutputStateCommand(this));
            AddCommand(new SetAnalogOutputVoltageCommand(this));
            
            //Data recorder
            AddCommand(new StartRecorderCommand(this));
            AddCommand(new StopRecorderCommand(this));
            AddCommand(new IsRecorderRunningCommand(this));
            AddCommand(new IsRecorderTriggeredCommand(this));
            AddCommand(new ActivateChannelCommand(this));
            AddCommand(new DeactivateAllChannelsCommand(this));
            AddCommand(new GetRecorderParameterCommand(this));
            AddCommand(new SetRecorderParameterCommand(this));
            AddCommand(new EnableTriggerCommand(this));
            AddCommand(new GetTriggerConfigurationCommand(this));
            AddCommand(new SetTriggerConfigurationCommand(this));
            AddCommand(new ReadChannelDataVectorCommand(this));
        }
        
        private void CreateToolSet()
        {
            AddTool(new Epos4ObjectDictionaryTool());
            AddTool(new Epos4ProfilePositionModeTool());
            AddTool(new Epos4ProfileVelocityModeTool());
            AddTool(new Epos4HomingModeTool());
            AddTool(new Epos4DataRecorderTool());
        }

        protected override void CreateCommunication()
        {
            Communication = new Epos4DeviceCommunication(this);
        }

        #endregion
    }
}
