using EltraCloudContracts.Contracts.DataRecorder;
using EltraCloudContracts.Contracts.Users;
using EltraConnector.Classes;
using EltraConnector.Definitions;
using EltraConnector.UserAgent;
using EltraConnector.UserAgent.Vcs;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace EltraNavigo.Device.Vcs
{
    public class EposVcs : DeviceVcs
    {
        #region Constructors

        public EposVcs(string url, string uuid, UserAuthData authData, uint updateInterval, uint timeout)
            : base(url, uuid, authData, updateInterval, timeout)
        {            
        }

        public EposVcs(DeviceAgent agent, EltraCloudContracts.Contracts.Devices.EltraDevice device)
            : base(agent, device)
        {
        }

        #endregion

        #region Methods

        public async Task<bool> SetEnableState()
        {
            bool result = false;
            uint lastErrorCode = 0;

            var command = await GetVcsCommand("SetEnableState");

            if (command != null)
            {
                var responseCommand = await Agent.ExecuteCommand(command);

                if (responseCommand != null)
                {
                    responseCommand.GetParameterValue("Result", ref result);
                    responseCommand.GetParameterValue("ErrorCode", ref lastErrorCode);
                }
            }

            return result;
        }

        public async Task<bool> SetDisableState()
        {
            bool result = false;
            uint lastErrorCode = 0;

            var command = await GetVcsCommand("SetDisableState");

            if (command != null)
            {
                var responseCommand = await Agent.ExecuteCommand(command);

                if (responseCommand != null)
                {
                    responseCommand.GetParameterValue("Result", ref result);
                    responseCommand.GetParameterValue("ErrorCode", ref lastErrorCode);
                }
            }

            return result;
        }

        public async Task<bool> SetQuickStopState()
        {
            bool result = false;
            uint lastErrorCode = 0;

            var command = await GetVcsCommand("SetQuickStopState");

            if (command != null)
            {
                var responseCommand = await Agent.ExecuteCommand(command);

                if (responseCommand != null)
                {
                    responseCommand.GetParameterValue("Result", ref result);
                    responseCommand.GetParameterValue("ErrorCode", ref lastErrorCode);
                }
            }

            return result;
        }

        public async Task<ExecuteResult> GetMotorType()
        {
            ExecuteResult result = null;

            var command = await GetVcsCommand("GetMotorType");

            if (command != null)
            {
                var responseCommand = await Agent.ExecuteCommand(command);

                if (responseCommand != null)
                {
                    bool commandResult = false;
                    uint lastErrorCode = 0;

                    responseCommand.GetParameterValue("Result", ref commandResult);
                    responseCommand.GetParameterValue("ErrorCode", ref lastErrorCode);
                    var motorType = responseCommand.GetParameter("MotorType");

                    if (motorType != null)
                    {
                        result = new ExecuteResult { Result = commandResult, ErrorCode = lastErrorCode };

                        result.Parameters.Add(motorType);
                    }
                }
            }

            return result;
        }

        public async Task<ExecuteResult> GetCurrentIs()
        {
            ExecuteResult result = null;

            var command = await GetVcsCommand("GetCurrentIs");

            if (command != null)
            {
                var responseCommand = await Agent.ExecuteCommand(command);

                bool commandResult = false;
                uint lastErrorCode = 0;

                responseCommand?.GetParameterValue("ErrorCode", ref lastErrorCode);
                responseCommand?.GetParameterValue("Result", ref commandResult);

                var current = responseCommand?.GetParameter("Current");

                if (current != null)
                {
                    result = new ExecuteResult { Result = commandResult, ErrorCode = lastErrorCode };
                    result.Parameters.Add(current);
                }
            }

            return result;
        }

        public async Task<ExecuteResult> GetCurrentIsAveraged()
        {
            ExecuteResult result = null;

            var command = await GetVcsCommand("GetCurrentIsAveraged");

            if (command != null)
            {
                var responseCommand = await Agent.ExecuteCommand(command);

                bool commandResult = false;
                uint lastErrorCode = 0;

                responseCommand?.GetParameterValue("ErrorCode", ref lastErrorCode);
                responseCommand?.GetParameterValue("Result", ref commandResult);
                var current = responseCommand?.GetParameter("Current");

                if (current != null)
                {
                    result = new ExecuteResult { Result = commandResult, ErrorCode = lastErrorCode };

                    result.Parameters.Add(current);
                }
            }

            return result;
        }

        public async Task<ExecuteResult> GetVelocityIsAveraged()
        {
            ExecuteResult result = null;

            var command = await GetVcsCommand("GetVelocityIsAveraged");

            if (command != null)
            {
                var responseCommand = await Agent.ExecuteCommand(command);

                bool commandResult = false;
                uint lastErrorCode = 0;

                responseCommand?.GetParameterValue("ErrorCode", ref lastErrorCode);
                responseCommand?.GetParameterValue("Result", ref commandResult);
                var velocity = responseCommand?.GetParameter("Velocity");

                if (velocity != null)
                {
                    result = new ExecuteResult { Result = commandResult, ErrorCode = lastErrorCode };

                    result.Parameters.Add(velocity);
                }
            }

            return result;
        }

        public async Task<bool> SetMotorType(short motorType)
        {
            bool result = false;
            var command = await GetVcsCommand("SetMotorType");

            if (command != null)
            {
                command.SetParameterValue("MotorType", motorType);

                var responseCommand = await Agent.ExecuteCommand(command);

                responseCommand?.GetParameterValue("Result", ref result);
            }

            return result;
        }

        public async Task<ExecuteResult> ClearFault()
        {
            var result = new ExecuteResult();
            var command = await GetVcsCommand("ClearFault");

            if (command != null)
            {
                var responseCommand = await Agent.ExecuteCommand(command);

                bool commandResult = false;
                uint lastErrorCode = 0;

                responseCommand?.GetParameterValue("ErrorCode", ref lastErrorCode);
                responseCommand?.GetParameterValue("Result", ref commandResult);

                result.Result = commandResult;
                result.ErrorCode = lastErrorCode;
            }

            return result;
        }

        public async Task<ExecuteResult> GetPositionIs()
        {
            var result = new ExecuteResult();
            var command = await GetVcsCommand("GetPositionIs");
            bool commandResult = false;
            uint lastErrorCode = 0;

            if (command != null)
            {
                var responseCommand = await Agent.ExecuteCommand(command);

                responseCommand.GetParameterValue("ErrorCode", ref lastErrorCode);
                responseCommand.GetParameterValue("Result", ref commandResult);

                result.Result = commandResult;
                result.ErrorCode = lastErrorCode;

                result.Parameters.Add(responseCommand.GetParameter("Position"));
            }

            return result;
        }

        public async Task<ExecuteResult> HaltVelocityMovement()
        {
            var result = new ExecuteResult();
            var command = await GetVcsCommand("HaltVelocityMovement");
            bool commandResult = false;
            uint lastErrorCode = 0;

            if (command != null)
            {
                var responseCommand = await Agent.ExecuteCommand(command);

                if (responseCommand != null)
                {
                    responseCommand.GetParameterValue("ErrorCode", ref lastErrorCode);
                    responseCommand.GetParameterValue("Result", ref commandResult);
                }

                result.Result = commandResult;
                result.ErrorCode = lastErrorCode;
            }

            return result;
        }

        public async Task<ExecuteResult> HaltPositionMovement()
        {
            var result = new ExecuteResult();
            var command = await GetVcsCommand("HaltPositionMovement");
            bool commandResult = false;
            uint lastErrorCode = 0;

            if (command != null)
            {
                var responseCommand = await Agent.ExecuteCommand(command);

                if (responseCommand != null)
                {
                    responseCommand.GetParameterValue("ErrorCode", ref lastErrorCode);
                    responseCommand.GetParameterValue("Result", ref commandResult);
                }

                result.Result = commandResult;
                result.ErrorCode = lastErrorCode;
            }

            return result;
        }

        public async Task<ExecuteResult> GetAllDigitalInputs()
        {
            var result = new ExecuteResult();
            var command = await GetVcsCommand("GetAllDigitalInputs");
            bool commandResult = false;
            uint lastErrorCode = 0;

            if (command != null)
            {
                var responseCommand = await Agent.ExecuteCommand(command);

                if (responseCommand != null)
                {
                    responseCommand.GetParameterValue("ErrorCode", ref lastErrorCode);
                    responseCommand.GetParameterValue("Result", ref commandResult);

                    result.Result = commandResult;
                    result.ErrorCode = lastErrorCode;
                    result.Parameters.Add(responseCommand.GetParameter("DigitalInputs"));
                }
            }

            return result;
        }

        public async Task<ExecuteResult> GetAllDigitalOutputs()
        {
            var result = new ExecuteResult();
            var command = await GetVcsCommand("GetAllDigitalOutputs");
            bool commandResult = false;
            uint lastErrorCode = 0;

            if (command != null)
            {
                var responseCommand = await Agent.ExecuteCommand(command);

                responseCommand?.GetParameterValue("ErrorCode", ref lastErrorCode);
                responseCommand?.GetParameterValue("Result", ref commandResult);

                result.Result = commandResult;
                result.ErrorCode = lastErrorCode;

                result.Parameters.Add(responseCommand?.GetParameter("DigitalOutputs"));
            }

            return result;
        }

        public async Task<ExecuteResult> GetAnalogInput()
        {
            var result = new ExecuteResult();
            var command = await GetVcsCommand("GetAnalogInput");
            bool commandResult = false;
            uint lastErrorCode = 0;

            if (command != null)
            {
                command.SetParameterValue("InputNumber", (ushort)0);

                var responseCommand = await Agent.ExecuteCommand(command);

                responseCommand?.GetParameterValue("ErrorCode", ref lastErrorCode);
                responseCommand?.GetParameterValue("Result", ref commandResult);

                result.Result = commandResult;
                result.ErrorCode = lastErrorCode;
                result.Parameters.Add(responseCommand?.GetParameter("AnalogValue"));
            }

            return result;
        }

        public async Task<ExecuteResult> GetAnalogInputVoltage()
        {
            ExecuteResult result = null;

            bool commandResult = false;
            uint lastErrorCode = 0;
            var command = await GetVcsCommand("GetAnalogInputVoltage");

            if (command != null)
            {
                command.SetParameterValue("InputNumber", (ushort)0);

                var responseCommand = await Agent.ExecuteCommand(command);

                responseCommand?.GetParameterValue("ErrorCode", ref lastErrorCode);
                responseCommand?.GetParameterValue("Result", ref commandResult);

                var voltage = responseCommand?.GetParameter("VoltageValue");

                if (voltage != null)
                {
                    result = new ExecuteResult { Result = commandResult, ErrorCode = lastErrorCode };

                    result.Parameters.Add(voltage);
                }
            }

            return result;
        }

        public async Task<ExecuteResult> GetState()
        {
            ExecuteResult result = null;

            bool commandResult = false;
            uint lastErrorCode = 0;
            var command = await GetVcsCommand("GetState");

            if (command != null)
            {
                var responseCommand = await Agent.ExecuteCommand(command);

                responseCommand?.GetParameterValue("ErrorCode", ref lastErrorCode);
                responseCommand?.GetParameterValue("Result", ref commandResult);
                var state = responseCommand?.GetParameter("State");

                if (state != null)
                {
                    result = new ExecuteResult { Result = commandResult, ErrorCode = lastErrorCode };

                    result.Parameters.Add(state);
                }
            }

            return result;
        }

        public async Task<ExecuteResult> GetFaultState()
        {
            ExecuteResult result = null;

            bool commandResult = false;
            uint lastErrorCode = 0;
            var command = await GetVcsCommand("GetFaultState");

            if (command != null)
            {
                var responseCommand = await Agent.ExecuteCommand(command);

                responseCommand?.GetParameterValue("ErrorCode", ref lastErrorCode);
                responseCommand?.GetParameterValue("Result", ref commandResult);
                var fault = responseCommand?.GetParameter("Fault");

                if (fault != null)
                {
                    result = new ExecuteResult { Result = commandResult, ErrorCode = lastErrorCode };

                    result.Parameters.Add(fault);
                }
            }

            return result;
        }

        public async Task<bool> SetState(EStates state)
        {
            bool result = false;
            bool commandResult = false;
            uint lastErrorCode = 0;
            var command = await GetVcsCommand("SetState");

            if (command != null)
            {
                command.SetParameterValue("State", (ushort)state);

                var responseCommand = await Agent.ExecuteCommand(command);

                responseCommand?.GetParameterValue("ErrorCode", ref lastErrorCode);
                responseCommand?.GetParameterValue("Result", ref commandResult);

                result = commandResult;
            }

            return result;
        }

        public async Task<bool> ResetDevice()
        {
            bool result = false;
            bool commandResult = false;
            uint lastErrorCode = 0;
            var command = await GetVcsCommand("ResetDevice");

            if (command != null)
            {
                var responseCommand = await Agent.ExecuteCommand(command);

                responseCommand?.GetParameterValue("ErrorCode", ref lastErrorCode);
                responseCommand?.GetParameterValue("Result", ref commandResult);

                result = commandResult;
            }

            return result;
        }

        public async Task<bool> StartRecorder(bool forceTrigger = false)
        {
            bool result = false;
            bool commandResult = false;
            uint lastErrorCode = 0;
            var command = await GetVcsCommand("StartRecorder");

            if (command != null)
            {
                command.SetParameterValue("ForceTrigger", forceTrigger);

                var responseCommand = await Agent.ExecuteCommand(command);

                responseCommand?.GetParameterValue("ErrorCode", ref lastErrorCode);
                responseCommand?.GetParameterValue("Result", ref commandResult);

                result = commandResult;
            }

            return result;
        }

        public async Task<bool> StopRecorder()
        {
            bool result = false;
            bool commandResult = false;
            uint lastErrorCode = 0;
            var command = await GetVcsCommand("StopRecorder");

            if (command != null)
            {
                var responseCommand = await Agent.ExecuteCommand(command);

                responseCommand?.GetParameterValue("ErrorCode", ref lastErrorCode);
                responseCommand?.GetParameterValue("Result", ref commandResult);

                result = commandResult;
            }

            return result;
        }

        public async Task<ExecuteResult> IsRecorderRunning()
        {
            ExecuteResult result = null;

            bool commandResult = false;
            uint lastErrorCode = 0;
            var command = await GetVcsCommand("IsRecorderRunning");

            if (command != null)
            {
                var responseCommand = await Agent.ExecuteCommand(command);

                responseCommand?.GetParameterValue("ErrorCode", ref lastErrorCode);
                responseCommand?.GetParameterValue("Result", ref commandResult);

                var running = responseCommand?.GetParameter("Running");

                if (running != null)
                {
                    result = new ExecuteResult { Result = commandResult, ErrorCode = lastErrorCode };

                    result.Parameters.Add(running);
                }
            }

            return result;
        }

        public async Task<ExecuteResult> IsRecorderTriggered()
        {
            ExecuteResult result = null;

            bool commandResult = false;
            uint lastErrorCode = 0;
            var command = await GetVcsCommand("IsRecorderTriggered");

            if (command != null)
            {
                var responseCommand = await Agent.ExecuteCommand(command);

                responseCommand?.GetParameterValue("ErrorCode", ref lastErrorCode);
                responseCommand?.GetParameterValue("Result", ref commandResult);

                var triggered = responseCommand?.GetParameter("Triggered");

                if (triggered != null)
                {
                    result = new ExecuteResult { Result = commandResult, ErrorCode = lastErrorCode };

                    result.Parameters.Add(triggered);
                }
            }

            return result;
        }

        public async Task<bool> ActivateChannel(byte channelNumber, ushort index, byte subIndex)
        {
            bool result = false;
            bool commandResult = false;
            uint lastErrorCode = 0;
            var command = await GetVcsCommand("ActivateChannel");

            if (command != null)
            {
                command.SetParameterValue("Channel", channelNumber);
                command.SetParameterValue("Index", index);
                command.SetParameterValue("SubIndex", subIndex);

                var responseCommand = await Agent.ExecuteCommand(command);

                responseCommand?.GetParameterValue("ErrorCode", ref lastErrorCode);
                responseCommand?.GetParameterValue("Result", ref commandResult);

                result = commandResult;
            }

            return result;
        }

        public async Task<bool> DeactivateChannel(byte channelNumber)
        {
            bool result = false;
            bool commandResult = false;
            uint lastErrorCode = 0;
            var command = await GetVcsCommand("ActivateChannel");

            if (command != null)
            {
                command.SetParameterValue("Channel", channelNumber);
                command.SetParameterValue("Index", 0);
                command.SetParameterValue("SubIndex", 0);

                var responseCommand = await Agent.ExecuteCommand(command);

                responseCommand?.GetParameterValue("ErrorCode", ref lastErrorCode);
                responseCommand?.GetParameterValue("Result", ref commandResult);

                result = commandResult;
            }

            return result;
        }

        public async Task<ExecuteResult> GetRecorderParameter()
        {
            ExecuteResult result = null;

            bool commandResult = false;
            uint lastErrorCode = 0;
            var command = await GetVcsCommand("GetRecorderParameter");

            if (command != null)
            {
                var responseCommand = await Agent.ExecuteCommand(command);

                responseCommand?.GetParameterValue("ErrorCode", ref lastErrorCode);
                responseCommand?.GetParameterValue("Result", ref commandResult);

                var samplingPeriod = responseCommand?.GetParameter("SamplingPeriod");
                var maxNumberOfSamples = responseCommand?.GetParameter("MaxNumberOfSamples");
                var precedingSamples = responseCommand?.GetParameter("PrecedingSamples");

                if (samplingPeriod != null && maxNumberOfSamples != null && precedingSamples != null)
                {
                    result = new ExecuteResult { Result = commandResult, ErrorCode = lastErrorCode };

                    result.Parameters.Add(samplingPeriod);
                    result.Parameters.Add(maxNumberOfSamples);
                    result.Parameters.Add(precedingSamples);
                }
            }

            return result;
        }

        public async Task<bool> SetRecorderParameter(ushort samplingPeriod, ushort maxNumberOfSamples, ushort precedingSamples)
        {
            bool result = false;
            uint lastErrorCode = 0;
            var command = await GetVcsCommand("SetRecorderParameter");

            if (command != null)
            {
                command.SetParameterValue("SamplingPeriod", samplingPeriod);
                command.SetParameterValue("MaxNumberOfSamples", maxNumberOfSamples);
                command.SetParameterValue("PrecedingSamples", precedingSamples);

                var responseCommand = await Agent.ExecuteCommand(command);

                responseCommand?.GetParameterValue("ErrorCode", ref lastErrorCode);
                responseCommand?.GetParameterValue("Result", ref result);
            }

            return result;
        }

        public async Task<ExecuteResult> GetTriggerConfiguration()
        {
            ExecuteResult result = null;
            bool commandResult = false;
            uint lastErrorCode = 0;
            var command = await GetVcsCommand("GetTriggerConfiguration");

            if (command != null)
            {
                var responseCommand = await Agent.ExecuteCommand(command);

                responseCommand?.GetParameterValue("ErrorCode", ref lastErrorCode);
                responseCommand?.GetParameterValue("Result", ref commandResult);

                var index = responseCommand?.GetParameter("Index");
                var subIndex = responseCommand?.GetParameter("SubIndex");
                var mask = responseCommand?.GetParameter("Mask");
                var mode = responseCommand?.GetParameter("Mode");
                var highValue = responseCommand?.GetParameter("HighValue");
                var lowValue = responseCommand?.GetParameter("LowValue");

                if (index != null && subIndex != null && mode != null)
                {
                    result = new ExecuteResult { Result = commandResult, ErrorCode = lastErrorCode };

                    result.Parameters.Add(index);
                    result.Parameters.Add(subIndex);
                    result.Parameters.Add(mask);
                    result.Parameters.Add(mode);
                    result.Parameters.Add(highValue);
                    result.Parameters.Add(lowValue);
                }
            }

            return result;
        }

        public async Task<bool> SetTriggerConfiguration(ushort index, byte subIndex, uint mask, byte mode, uint highValue, uint lowValue)
        {
            bool result = false;
            uint lastErrorCode = 0;
            var command = await GetVcsCommand("SetTriggerConfiguration");

            if (command != null)
            {
                command.SetParameterValue("Index", index);
                command.SetParameterValue("SubIndex", subIndex);
                command.SetParameterValue("Mask", mask);
                command.SetParameterValue("Mode", mode);
                command.SetParameterValue("HighValue", highValue);
                command.SetParameterValue("LowValue", lowValue);

                var responseCommand = await Agent.ExecuteCommand(command);

                responseCommand?.GetParameterValue("ErrorCode", ref lastErrorCode);
                responseCommand?.GetParameterValue("Result", ref result);
            }

            return result;
        }

        public async Task<DataRecorderSamples> ReadChannelDataVector(int channelNumber)
        {
            DataRecorderSamples result = null;

            bool commandResult = false;
            uint lastErrorCode = 0;
            var command = await GetVcsCommand("ReadChannelDataVector");

            if (command != null)
            {
                command.SetParameterValue("Channel", (byte)channelNumber);

                var responseCommand = await Agent.ExecuteCommand(command);

                responseCommand?.GetParameterValue("ErrorCode", ref lastErrorCode);
                responseCommand?.GetParameterValue("Result", ref commandResult);

                var samples = responseCommand?.GetParameter("Samples");

                if (samples != null)
                {
                    string json = string.Empty;

                    samples.GetValue(ref json);

                    result = JsonConvert.DeserializeObject<DataRecorderSamples>(json);
                }
            }

            return result;
        }

        public async Task<bool> MoveWithVelocity(int targetVelocity)
        {
            bool result = false;
            uint lastErrorCode = 0;
            var command = await GetVcsCommand("MoveWithVelocity");

            if (command != null)
            {
                command.SetParameterValue("Velocity", targetVelocity);

                var responseCommand = await Agent.ExecuteCommand(command);

                responseCommand?.GetParameterValue("ErrorCode", ref lastErrorCode);
                responseCommand?.GetParameterValue("Result", ref result);
            }

            return result;
        }

        public async Task<bool> FindHome(int method)
        {
            bool result = false;
            uint lastErrorCode = 0;
            var command = await GetVcsCommand("FindHome");

            if (command != null)
            {
                command.SetParameterValue("Method", method);

                var responseCommand = await Agent.ExecuteCommand(command);

                responseCommand?.GetParameterValue("ErrorCode", ref lastErrorCode);
                responseCommand?.GetParameterValue("Result", ref result);
            }

            return result;
        }

        public async Task<bool> DefinePosition(int targetPosition)
        {
            bool result = false;
            uint lastErrorCode = 0;
            var command = await GetVcsCommand("DefinePosition");

            if (command != null)
            {
                command.SetParameterValue("Position", targetPosition);

                var responseCommand = await Agent.ExecuteCommand(command);

                responseCommand?.GetParameterValue("ErrorCode", ref lastErrorCode);
                responseCommand?.GetParameterValue("Result", ref result);
            }

            return result;
        }

        public async Task<bool> MoveToPosition(int targetPosition, bool absolute, bool immediately)
        {
            bool result = false;
            uint lastErrorCode = 0;
            var command = await GetVcsCommand("MoveToPosition");

            if (command != null)
            {
                command.SetParameterValue("Position", targetPosition);
                command.SetParameterValue("Absolute", absolute);
                command.SetParameterValue("Immediately", immediately);

                var responseCommand = await Agent.ExecuteCommand(command);

                responseCommand?.GetParameterValue("ErrorCode", ref lastErrorCode);
                responseCommand?.GetParameterValue("Result", ref result);
            }

            return result;
        }

        public async Task<bool> ActivateProfileVelocityMode()
        {
            bool result = false;
            uint lastErrorCode = 0;
            var command = await GetVcsCommand("ActivateProfileVelocityMode");

            if (command != null)
            {
                var responseCommand = await Agent.ExecuteCommand(command);

                responseCommand?.GetParameterValue("ErrorCode", ref lastErrorCode);
                responseCommand?.GetParameterValue("Result", ref result);
            }

            return result;
        }

        public async Task<bool> ActivateHomingMode()
        {
            bool result = false;
            uint lastErrorCode = 0;
            var command = await GetVcsCommand("ActivateHomingMode");

            if (command != null)
            {
                var responseCommand = await Agent.ExecuteCommand(command);

                responseCommand?.GetParameterValue("ErrorCode", ref lastErrorCode);
                responseCommand?.GetParameterValue("Result", ref result);
            }

            return result;
        }

        public async Task<bool> ActivateProfilePositionMode()
        {
            bool result = false;
            uint lastErrorCode = 0;
            var command = await GetVcsCommand("ActivateProfilePositionMode");

            if (command != null)
            {
                var responseCommand = await Agent.ExecuteCommand(command);

                responseCommand?.GetParameterValue("ErrorCode", ref lastErrorCode);
                responseCommand?.GetParameterValue("Result", ref result);
            }

            return result;
        }


        #endregion
    }
}
