using System;
using System.Collections.Generic;
using EltraCloud.Services.Events;
using EltraCloudContracts.Contracts.CommandSets;
using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.Contracts.Parameters;
using EltraCloudContracts.Contracts.Sessions;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.DeviceDescription;
using EltraCommon.Logger;
using Microsoft.AspNetCore.Http;

#pragma warning disable CS1591

namespace EltraCloud.Services
{
    public abstract class ISessionService
    {
        #region Events

        public event EventHandler<ParameterValueChangedEventArgs> ParameterValueChanged;        
        
        public event EventHandler<ExecCommandEventArgs> ExecCommandAdded;
        public event EventHandler<ExecCommandStatusEventArgs> ExecCommandStatusChanged;
        
        public event EventHandler<SessionStatusChangedEventArgs> SessionStatusChanged;

        #endregion

        #region Events handling

        protected virtual void OnParameterValueChanged(ParameterValueChangedEventArgs e)
        {
            ParameterValueChanged?.Invoke(this, e);
        }

        protected virtual void OnExecCommandAdded(ExecCommandEventArgs e)
        {
            ExecCommandAdded?.Invoke(this, e);
        }

        protected virtual void OnExecCommandStatusChanged(ExecCommandStatusEventArgs e)
        {
            ExecCommandStatusChanged?.Invoke(this, e);
        }

        protected void OnSessionStatusChanged(string sessionId, SessionStatus status)
        {
            MsgLogger.Print($"request session '{sessionId}' status changed to {status}!");

            SessionStatusChanged?.Invoke(this, new SessionStatusChangedEventArgs() { Uuid = sessionId, Status = status });
        }

        #endregion

        #region Interfaces

        #region Session

        public abstract bool AddSession(Session session);
        public abstract Session GetSession(string uuid);
        public abstract List<Session> GetSessions();
        public abstract List<Session> GetSessions(string login, string password);
        public abstract List<EltraDevice> GetSessionDevices(string sessionUuid);
        public abstract bool SessionExists(string id);
        public abstract bool SetSessionStatus(string sessionId, string loginName, SessionStatus status);
        public abstract bool SetSessionStatus(string sessionId, SessionStatus status);
        public abstract List<Session> GetSessions(SessionStatus status, bool devicesOnly);
        public abstract bool CreateSessionLink(string uuid, List<Session> sessions);
        public abstract List<string> GetLinkedSessionUuids(string uuid, bool isMaster);

        #endregion

        #region Parameters

        public abstract bool UpdateParameterValue(ParameterUpdate parameterUpdate);
        public abstract Parameter GetParameter(ulong serialNumber, ushort index, byte subIndex);
        public abstract ParameterValue GetParameterValue(ulong serialNumber, ushort index, byte subIndex);
        public abstract List<ParameterValue> GetParameterHistory(ulong serialNumber, string uniqueId, DateTime from, DateTime to);
        public abstract List<ParameterUniqueIdValuePair> GetParameterPairHistory(ulong serialNumber, string uniqueId1, string uniqueId2, DateTime from, DateTime to);
        
        #endregion

        #region Devices

        public abstract Session GetDeviceSession(ulong serialNumber);
        public abstract EltraDevice FindDevice(Session session, ulong serialNumber);
        public abstract bool RemoveDevice(Session session, ulong serialNumber);
        public abstract bool RemoveDevice(string sessionId, ulong serialNumber);
        public abstract bool RegisterDevice(ConnectionInfo connection, SessionDevice sessionDevice);
        public abstract bool SessionDevicesCount(Session session, out int count);
        
        public abstract EltraDevice FindDevice(ulong serialNumber);
        public abstract bool DeviceExists(string id, ulong serialNumber);
        public abstract bool LockDevice(string agentUuid, ulong serialNumber);
        public abstract bool UnlockDevice(string agentUuid, ulong serialNumber);
        public abstract bool CanLockDevice(string agentUuid, ulong serialNumber);
        public abstract bool IsDeviceLockedByAgent(string agentUuid, ulong serialNumber);
        public abstract bool IsDeviceUsedByAgent(string sesionUuid, ulong serialNumber);

        #endregion

        #region Device Description

        public abstract DeviceDescription DownloadDeviceDescription(DeviceVersion version);
        public abstract bool UploadDeviceDescription(DeviceDescription deviceDescription);
        public abstract bool DeviceDescriptionExists(ulong serialNumber, string hashCode);

        #endregion

        #region Commands

        public abstract bool PushCommand(ExecuteCommand executeCommand);
        public abstract bool CanPushCommand(ExecuteCommand executeCommand);
        public abstract bool SetCommandStatus(ExecuteCommandStatus commandStatus);
        public abstract bool SetCommandCommStatus(ExecuteCommandStatus commandStatus);

        public abstract ExecuteCommand PopCommand(string uuid, ulong serialNumber, ExecCommandStatus status);        
        public abstract List<ExecuteCommand> PopCommands(ulong serialNumber, ExecCommandStatus status);

        public abstract List<ExecuteCommand> PopCommands(SessionIdentification sessionIdent, ExecCommandStatus[] status);
        public abstract List<ExecuteCommand> GetExecCommands(SessionIdentification sessionIdent, ExecCommandStatus[] status);
        public abstract DeviceCommandSet GetDeviceCommands(ulong serialNumber);
        public abstract DeviceCommand GetDeviceCommand(ulong serialNumber, string deviceCommand);
        public abstract ExecuteCommandStatus GetCommandStatus(string uuid, string sessionUuid, ulong serialNumber, string commandName);

        #endregion

        #region Users

        public abstract bool IsUserValid(string user, string password);

        #endregion

        #endregion
    }
}
