using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using EltraCloudStorage.Services;
using EltraCommon.Helpers;
using EltraCloud.DataSource;
using EltraCloudContracts.Contracts.CommandSets;
using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.Contracts.Parameters;
using EltraCloudContracts.Contracts.Sessions;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.DeviceDescription;
using EltraCloudContracts.ObjectDictionary.DeviceDescription.Events;
using EltraCloudContracts.ObjectDictionary.DeviceDescription.Factory;
using EltraCloudContracts.ObjectDictionary.Factory;
using EltraCloud.Services.Events;
using EltraCommon.Logger;
using Microsoft.AspNetCore.Http;

#pragma warning disable CS1591, CA1063

namespace EltraCloud.Services
{
    /// <summary>
    /// Session Service
    /// </summary>    
    public class SessionService : ISessionService, IDisposable
    {
        #region Private fields

        private IStorageService _storageService;
        private Task _runTask;
        private readonly CancellationTokenSource _runTokenSource;  

        #endregion

        #region Constructors

        public SessionService(Storage storage)
        {
            Storage = storage;

            _runTokenSource = new CancellationTokenSource();
        }

        #endregion

        #region Properties

        public Storage Storage { get; }

        #endregion

        #region Methods

        #region Initialize

        public void Start()
        {
            _storageService = Storage.CreateStorageService();

            _runTask = Task.Run(() => ValidateSessions(_runTokenSource.Token));
        }

        #endregion

        #region Interface

        public override Session GetSession(string uuid)
        {
            Session result = null;

            if (_storageService != null)
            {
                result = _storageService.GetSession(uuid);
            }

            return result;
        }

        public override bool IsUserValid(string user, string password)
        {
            bool result = false;

            if (_storageService != null)
            {
                if (_storageService.IsUserValid(user, password))
                {
                    result = true;
                }
            }

            return result;
        }

        public override List<Session> GetSessions()
        {
            return _storageService.GetSessions(SessionStatus.Online, false);
        }

        public override List<Session> GetSessions(string login, string password)
        {
            return _storageService.GetSessions(login, password, SessionStatus.Online);
        }

        public override List<Session> GetSessions(SessionStatus status, bool devicesOnly)
        {
            var result = _storageService.GetSessions(status, devicesOnly);

            return result;
        }

        public override bool AddSession(Session session)
        {
            bool result = false;

            if (session != null)
            {
                result = _storageService.UpdateSession(session);
            }

            return result;
        }

        public override bool CreateSessionLink(string uuid, List<Session> sessions)
        {
            return _storageService.CreateSessionLink(uuid, sessions);
        }

        public override List<string> GetLinkedSessionUuids(string uuid, bool isMaster)
        {
            return _storageService.GetLinkedSessionUuids(uuid, isMaster);
        }

        public override EltraDevice FindDevice(ulong serialNumber)
        {
            var start = MsgLogger.BeginTimeMeasure();

            var result = FindSessionDevice(serialNumber);

            MsgLogger.EndTimeMeasure($"{GetType().Name} - FindDevice", start, $"Find device, serial number = 0x{serialNumber:X4}, result={result != null}");

            return result;
        }


        public EltraDevice FindSessionDevice(ulong serialNumber)
        {
            var start = MsgLogger.BeginTimeMeasure();

            var result = _storageService.GetDevice(serialNumber, SessionStatus.Online);

            MsgLogger.EndTimeMeasure($"{GetType().Name} - FindSessionDevice", start, $"Find device, serial number = 0x{serialNumber:X4}, result={result!=null}");

            return result;
        }

        public override DeviceCommandSet GetDeviceCommands(ulong serialNumber)
        {
            var device = FindDevice(serialNumber);

            var result = device.CommandSet;

            return result;
        }

        public override DeviceCommand GetDeviceCommand(ulong serialNumber, string commandName)
        {
            var command = FindDevice(serialNumber)?.FindCommand(commandName);

            return command;
        }

        public override ExecuteCommandStatus GetCommandStatus(string uuid, string sessionUuid, ulong serialNumber, string commandName)
        {
            return _storageService.GetCommandStatus(uuid, sessionUuid, serialNumber, commandName);
        }

        public override bool SessionExists(string id)
        {
            bool result = false;
            var session = GetSession(id);

            if (session != null)
            {
                result = true;
            }

            return result;
        }

        public override bool SetSessionStatus(string sessionId, string loginName, SessionStatus status)
        {
            bool result = false;
            var currentStatus = _storageService.GetSessionStatus(sessionId);

            if (_storageService.SetSessionStatus(loginName, sessionId, status))
            {
                result = true;

                if (currentStatus != status)
                {
                    OnSessionStatusChanged(sessionId, status);

                    if (status == SessionStatus.Offline)
                    {
                        result = _storageService.SetSessionLinkStatus(sessionId, status);
                    }
                }
            }

            return result;
        }

        public override bool SetSessionStatus(string sessionId, SessionStatus status)
        {
            bool result = false;

            if (_storageService.SetSessionStatus(sessionId, status))
            {
                OnSessionStatusChanged(sessionId, status);

                if (status == SessionStatus.Offline)
                {
                    result = _storageService.SetSessionLinkStatus(sessionId, status);
                }
                else
                {
                    result = true;
                }
            }

            return result;
        }

        public override bool CanPushCommand(ExecuteCommand executeCommand)
        {
            bool result = true;

            var serialNumber = executeCommand.SerialNumber;
            var agentUuid = executeCommand.SessionUuid;
            var device = FindDevice(serialNumber);

            if (device != null)
            {
                if (device.SessionUuid != executeCommand.SessionUuid)
                {
                    string owner = _storageService.GetDeviceOwner(device.SessionUuid, serialNumber);

                    result = string.IsNullOrEmpty(owner) || owner == agentUuid;
                }
            }

            return result;
        }

        public override bool PushCommand(ExecuteCommand executeCommand)
        {
            Task.Run(()=>
            {
                if (executeCommand != null)
                {
                    AssignDeviceToExecCommand(executeCommand);

                    bool result = _storageService.SetCommand(executeCommand);

                    if (result)
                    {
                        OnExecCommandAdded(new ExecCommandEventArgs() { ExecuteCommand = executeCommand });
                    }
                }
            });
            
            return true;    
        }

        private void AssignDeviceToExecCommand(ExecuteCommand executeCommand)
        {
            var device = FindSessionDevice(executeCommand.SerialNumber);

            var deviceCommand = executeCommand?.Command;

            if (deviceCommand != null)
            {
                deviceCommand.Device = device;
            }
        }

        public override bool SetCommandStatus(ExecuteCommandStatus commandStatus)
        {
            Task.Run(() =>
            {
                bool result = _storageService.SetCommandStatus(commandStatus);

                if (result)
                {
                    OnExecCommandStatusChanged(new ExecCommandStatusEventArgs() { Status = commandStatus });
                }
            });

            return true; 
        }

        public override bool SetCommandCommStatus(ExecuteCommandStatus commandStatus)
        {
            _storageService.SetSessionStatus(commandStatus.SessionUuid, SessionStatus.Online);

            return _storageService.SetCommandCommStatus(commandStatus);
        }

        public override List<ExecuteCommand> PopCommands(ulong serialNumber, ExecCommandStatus status)
        {
            return _storageService.PopCommands(serialNumber, status);
        }

        private ulong[] GetSessionDevicesSerialNumbers(SessionIdentification sessionIdentification)
        {
            var sessionDevices = GetSessionDevices(sessionIdentification);
            var serialNumbers = new List<ulong>();

            foreach (var device in sessionDevices)
            {
                serialNumbers.Add(device.Identification.SerialNumber);
            }

            return serialNumbers.ToArray();
        }

        public override List<ExecuteCommand> PopCommands(SessionIdentification sessionIdentification, ExecCommandStatus[] statusArray)
        {
            var serialNumbers = GetSessionDevicesSerialNumbers(sessionIdentification);

            var result = _storageService.PopCommands(serialNumbers, statusArray);

            return result;
        }

        public override List<ExecuteCommand> GetExecCommands(SessionIdentification sessionIdentification, ExecCommandStatus[] statusArray)
        {
            var result = _storageService.GetExecCommands(sessionIdentification.Uuid, statusArray);

            return result;
        }
                
        public override ExecuteCommand PopCommand(string uuid, ulong serialNumber, ExecCommandStatus status)
        {
            return _storageService.PopCommand(uuid, serialNumber, status);
        }
        
        public override bool DeviceExists(string id, ulong serialNumber)
        {
            bool result = false;
            var deviceSession = GetDeviceSession(serialNumber);

            if (deviceSession != null && deviceSession.Uuid == id)
            {
                result = true;
            }

            return result;
        }

        public override bool RemoveDevice(string sessionId, ulong serialNumber)
        {
            bool result = false;
            var session = GetSession(sessionId);

            if (session != null)
            {
                result = _storageService.RemoveDevice(serialNumber);

                if (result)
                {
                    result = RemoveDevice(session, serialNumber);

                    if (result && SessionDevicesCount(session, out var devicesCount) && devicesCount == 0)
                    {
                        result = _storageService.SetSessionStatus(session.Uuid, SessionStatus.Offline);

                        if (result)
                        {
                            session.Status = SessionStatus.Offline;
                        }
                    }
                }
            }

            return result;
        }

        public override bool RegisterDevice(ConnectionInfo connection, SessionDevice sessionDevice)
        {
            var result = _storageService.AddSessionDevice(sessionDevice);

            if (result)
            {
                UpdateDeviceDescription(sessionDevice.Device);
            }

            return result;
        }

        public override Session GetDeviceSession(ulong serialNumber)
        {
            var result = _storageService.GetDeviceSession(serialNumber, SessionStatus.Online);

            return result;
        }

        public override bool RemoveDevice(Session session, ulong serialNumber)
        {
            bool result = _storageService.RemoveDevice(serialNumber);

            return result;
        }

        public override List<EltraDevice> GetSessionDevices(string sessionUuid)
        {
            var result = _storageService.GetSessionDevices(sessionUuid);

            foreach(var device in result)
            {
                device.SessionUuid = sessionUuid;

                UpdateDeviceDescription(device);
            }

            return result;
        }

        private List<EltraDevice> GetSessionDevices(SessionIdentification sessionIdent)
        {
            var result = _storageService.GetSessionDevices(sessionIdent.Uuid);

            return result;
        }

        public override bool UpdateParameterValue(ParameterUpdate parameterUpdate)
        {
            bool result = _storageService.UpdateParameter(parameterUpdate);

            if (result)
            {
                var device = FindDevice(parameterUpdate.SerialNumber);
                var objectDictionary = device?.ObjectDictionary;

                if (objectDictionary == null)
                {
                    if (UpdateDeviceDescription(device))
                    {
                        objectDictionary = device?.ObjectDictionary;
                    }
                }

                if (objectDictionary?.SearchParameter(parameterUpdate.Parameter.Index, parameterUpdate.Parameter.SubIndex) is Parameter parameter)
                {
                    result = parameter.SetValue(parameterUpdate.Parameter.ActualValue);
                }

                if (result)
                {
                    MsgLogger.WriteDebug($"{GetType().Name} - Update", $"Update Parameter Value, {parameterUpdate.Parameter.UniqueId}, result = {result}");

                    OnParameterValueChanged(new ParameterValueChangedEventArgs(parameterUpdate));

                    MsgLogger.WriteDebug($"{GetType().Name} - Update", $"Send Parameter Value notification, {parameterUpdate.Parameter.UniqueId}");
                }
            }

            return result;
        }
        public override Parameter GetParameter(ulong serialNumber, ushort index, byte subIndex)
        {
            Parameter result = null;
            var device = FindDevice(serialNumber);
            var objectDictionary = device?.ObjectDictionary;

            if(objectDictionary==null)
            {
                if(UpdateDeviceDescription(device))
                {
                    objectDictionary = device?.ObjectDictionary;
                }
            }

            if (objectDictionary?.SearchParameter(index, subIndex) is Parameter parameter)
            {
                result = parameter;
            }

            return result;
        }

        private ParameterValue GetParameterValue(EltraDevice device, Parameter parameter)
        {
            ParameterValue result = null;

            if (device != null && parameter != null && device.Identification!=null)
            {
                var serialNumber = device.Identification.SerialNumber;
                var index = parameter.Index;
                var subIndex = parameter.SubIndex;

                result = _storageService.GetParameterValue(serialNumber, index, subIndex);
            }

            return result;
        }

        public override ParameterValue GetParameterValue(ulong serialNumber, ushort index, byte subIndex)
        {
            var result = _storageService.GetParameterValue(serialNumber, index, subIndex);

            return result;
        }

        public override List<ParameterValue> GetParameterHistory(ulong serialNumber, string uniqueId, DateTime from, DateTime to)
        {
            var result = _storageService.GetParameterHistory(serialNumber, uniqueId, from, to);

            return result;
        }

        public override List<ParameterUniqueIdValuePair> GetParameterPairHistory(ulong serialNumber, string uniqueId1, string uniqueId2, DateTime from, DateTime to)
        {
            var result = _storageService.GetParameterPairHistory(serialNumber, uniqueId1, uniqueId2, from, to);

            return result;
        }

        public override EltraDevice FindDevice(Session session, ulong serialNumber)
        {
            EltraDevice result = _storageService.GetDevice(serialNumber, session.Status);

            return result;
        }

        #endregion

        #region Private 
        
        private async Task ValidateSessions(CancellationToken token)
        {
            const int defaultTimeoutInSec = 60;
            const int tickDelay = 100;
            
            do
            {
                double validateDelay = TimeSpan.FromSeconds(defaultTimeoutInSec).TotalMilliseconds;

                var sessions = _storageService.GetSessions(SessionStatus.Online, false);

                foreach (var session in sessions)
                {
                    var timeoutInSec = TimeSpan.FromSeconds(session.Timeout).TotalMilliseconds;

                    if (validateDelay > timeoutInSec)
                    {
                        validateDelay = timeoutInSec;
                    }

                    var duration = (DateTime.Now - session.Modified).TotalSeconds;

                    if (duration > session.Timeout)
                    {
                        MsgLogger.WriteLine($"timeout {session.Timeout} elapsed, set session '{session.Uuid}' to offline state");

                        if (!SetSessionStatus(session.Uuid, SessionStatus.Offline))
                        {
                            MsgLogger.WriteError($"ValidateSessions", $"cannot set session '{session.Uuid}' to offline!");

                            break;
                        }
                    }
                }

                var watch = new Stopwatch();

                watch.Start();

                while (watch.ElapsedMilliseconds < validateDelay && !token.IsCancellationRequested)
                {
                    await Task.Delay(tickDelay);
                }

            } while (!token.IsCancellationRequested);
        }

        private bool UpdateDeviceDescription(EltraDevice device)
        {
            bool result = true;

            if (device != null && device.DeviceDescription == null)
            {
                var deviceDescription = DeviceDescriptionFactory.CreateDeviceDescription(device);

                if(deviceDescription != null)
                {
                    var content = DownloadDeviceDescription(device.Version);

                    if(content!=null)
                    {
                        deviceDescription.Content = content.PlainContent;

                        device.DeviceDescription = deviceDescription;

                        result = CreateObjectDictionary(device, deviceDescription);
                    }
                }
            }

            UpdateDeviceImage(device);

            return result;
        }

        private bool UpdateParameters(EltraDevice device)
        {
            bool result = true;

            if (device != null)
            {
                var objectDictionary = device?.ObjectDictionary;

                if (objectDictionary == null)
                {
                    if (UpdateDeviceDescription(device))
                    {
                        objectDictionary = device?.ObjectDictionary;
                    }
                }

                var parameters = objectDictionary?.Parameters;

                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        if (parameter is Parameter parameterEntry && parameterEntry.ActualValue.IsValid)
                        {
                            var parameterValue = GetParameterValue(device, parameterEntry);

                            if (parameterValue != null)
                            {
                                result = parameterEntry.SetValue(parameterValue);
                            }
                            else
                            {
                                MsgLogger.WriteWarning($"{GetType().Name} - UpdateParameters", $"cannot update parameter value id='{parameterEntry.UniqueId}'");
                                result = false;
                                break;
                            }
                        }
                        else if (parameter is StructuredParameter structuredParameter)
                        {
                            var subParameters = structuredParameter.Parameters;
                            if (subParameters != null)
                                foreach (var subParameter in subParameters)
                                {
                                    if (subParameter is Parameter subParameterEntry && subParameterEntry.ActualValue.IsValid)
                                    {
                                        var parameterValue = GetParameterValue(device, subParameterEntry);

                                        if (parameterValue != null)
                                        {
                                            result = subParameterEntry.SetValue(parameterValue);
                                        }
                                        else
                                        {
                                            result = false;
                                            MsgLogger.WriteWarning($"{GetType().Name} - UpdateParameters", $"cannot update parameter value id='{subParameterEntry.UniqueId}'");
                                            break;
                                        }
                                    }
                                }
                        }
                    }
                }
            }

            return result;
        }

        private bool CreateObjectDictionary(EltraDevice device, DeviceDescriptionFile _)
        {
            bool result = false;

            try
            {
                device.ObjectDictionary = ObjectDictionaryFactory.CreateObjectDictionary(device);

                if (device.ObjectDictionary.Open())
                {
                    if (!UpdateParameters(device))
                    {
                        MsgLogger.WriteWarning($"{GetType().Name} - CreateObjectDictionary", $"update parameters for device '0x{device.Identification.SerialNumber:X4}' failed, first registration?");
                    }

                    result = true;
                }
            }
            catch(Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - CreateObjectDictionary", e);
            }

            return result;
        }
        
        private string DeviceToPictureFileNameConverter(EltraDevice device)
        {
            var version = device?.Version;

            var result = $"{version?.HardwareVersion:X4}.png";

            return result;
        }
        
        private void UpdateDeviceImage(EltraDevice device)
        {
            if(device!=null)
            { 
                string fileName = DeviceToPictureFileNameConverter(device);
            
                if (ResourceHelper.GetBase64ImageFromResources(fileName, out var base64Image))
                {
                    device.ImageSrc = base64Image;
                }
                else
                {
                    if(device.DeviceDescription != null)
                    {
                        string url = $"{device?.DeviceDescription?.Url}";

                        if(!string.IsNullOrEmpty(url))
                        {
                            if(!url.EndsWith('/'))
                            {
                                url += "/";                                
                            }

                            device.ImageSrc = $"{url}../thumbnails/{fileName}";
                        }
                    }
                }
            }
        }

        public override bool LockDevice(string agentUuid, ulong serialNumber)
        {
            bool result = false;
            var device = FindDevice(serialNumber);

            if (device != null)
            {
                string owner = _storageService.GetDeviceOwner(device.SessionUuid, serialNumber);

                if (string.IsNullOrEmpty(owner))
                {
                    result = _storageService.LockDevice(device.SessionUuid, serialNumber, agentUuid);
                }
                else if(owner == agentUuid)
                {
                    result = true;
                }
            }

            return result;
        }

        public override bool UnlockDevice(string agentUuid, ulong serialNumber)
        {
            bool result = false;
            var device = FindDevice(serialNumber);

            if (device != null)
            {
                string owner = _storageService.GetDeviceOwner(device.SessionUuid, serialNumber);

                if (agentUuid == owner)
                {
                    result = _storageService.UnlockDevice(device.SessionUuid, serialNumber);
                }
            }

            return result;
        }

        public override bool CanLockDevice(string agentUuid, ulong serialNumber)
        {
            bool result = false;
            var device = FindDevice(serialNumber);

            if (device != null)
            {
                string owner = _storageService.GetDeviceOwner(device.SessionUuid, serialNumber);

                result = string.IsNullOrEmpty(owner) || owner == agentUuid;
            }

            return result;
        }

        public override bool IsDeviceLockedByAgent(string agentUuid, ulong serialNumber)
        {
            bool result = false;
            var device = FindDevice(serialNumber);

            if (device != null && _storageService!=null)
            {
                string owner = _storageService.GetDeviceOwner(device.SessionUuid, serialNumber);

                result = (owner == agentUuid);
            }

            return result;
        }

        public override bool IsDeviceUsedByAgent(string agentSesionUuid, ulong serialNumber)
        {
            bool result = false;

            if (_storageService != null)
            {
                result = _storageService.IsDeviceUsedByAgent(agentSesionUuid, serialNumber);                
            }

            return result;
        }


        public override DeviceDescription DownloadDeviceDescription(DeviceVersion version)
        {
            DeviceDescription result = null;

            if (_storageService != null)
            {
                result = _storageService.DownloadDeviceDescription(version);
            }

            return result;
        }

        public override bool UploadDeviceDescription(DeviceDescription deviceDescription)
        {
            bool result = false;

            if (_storageService != null)
            {
                result = _storageService.UploadDeviceDescription(deviceDescription);
            }

            return result;
        }

        public override bool DeviceDescriptionExists(string hashCode)
        {
            bool result = false;

            if (_storageService != null)
            {
                result = _storageService.DeviceDescriptionExists(hashCode);
            }

            return result;
        }

        #endregion

        #endregion

        #region Dispose

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            _runTokenSource.Cancel();

            _runTask.Wait();
        }

        public override bool SessionDevicesCount(Session session, out int count)
        {
            var result = _storageService.GetSessionDevicesCount(session, out count);

            return result;
        }

        #endregion
    }
}
