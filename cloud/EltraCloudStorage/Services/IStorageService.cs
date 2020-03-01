using System;
using System.Collections.Generic;
using EltraCloudContracts.Contracts.CommandSets;
using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.Contracts.Parameters;
using EltraCloudContracts.Contracts.Sessions;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.DeviceDescription;

namespace EltraCloudStorage.Services
{
    public abstract class IStorageService
    {
        public abstract bool UpdateSession(Session session);
                        
        public abstract bool SetSessionStatus(string sessionUuid, SessionStatus sessionStatus);

        public abstract bool RemoveDevice(ulong serialNumber);

        public abstract bool UpdateParameter(ParameterUpdate parameterUpdate);
        public abstract ParameterValue GetParameterValue(ulong serialNumber, ushort index, byte subIndex);

        public abstract bool AddSessionDevice(SessionDevice sessionDevice);
        
        public abstract List<ExecuteCommand> PopCommands(ulong serialNumber, ExecCommandStatus status);

        public abstract List<ExecuteCommand> PopCommands(ulong[] serialNumber, ExecCommandStatus[] status);

        public abstract List<ExecuteCommand> GetExecCommands(string sessionUuid, ExecCommandStatus[] status);

        public abstract ExecuteCommand PopCommand(string uuid, ulong serialNumber, ExecCommandStatus status);

        public abstract bool SetCommand(ExecuteCommand executeCommand);

        public abstract bool SetCommandStatus(ExecuteCommandStatus commandStatus);
        
        public abstract bool SetCommandCommStatus(ExecuteCommandStatus commandStatus);

        public abstract List<Session> GetSessions(SessionStatus status, bool deviceOnly);
        public abstract List<Session> GetSessions(string login, string password, SessionStatus status);

        public abstract EltraDevice GetDevice(ulong serialNumber, SessionStatus status);

        public abstract Session GetSession(string uuid);

        public abstract SessionStatus GetSessionStatus(string uuid);

        public abstract Session GetDeviceSession(ulong serialNumber, SessionStatus status);

        public abstract ExecuteCommandStatus GetCommandStatus(string uuid, string sessionUuid, ulong serialNumber, string commandName);

        public abstract bool IsDeviceUsedByAgent(string agentSesionUuid, ulong serialNumber);

        //Lock device
        public abstract string GetDeviceOwner(string deviceSessionUuid, ulong serialNumber);

        public abstract bool LockDevice(string deviceSessionUuid, ulong serialNumber, string agentUuid);

        public abstract bool UnlockDevice(string deviceSessionUuid, ulong serialNumber);

        //users
        public abstract bool UserExists(string userName);
        
        public abstract bool IsUserValid(string user, string password);

        public abstract bool RegisterUser(string loginName, string userName, string password);
        
        public abstract bool SignInUser(string user, string password, out string token);

        public abstract bool SignOutUser(string token);
        public abstract List<ParameterValue> GetParameterHistory(ulong serialNumber, string uniqueId, DateTime from, DateTime to);
        public abstract List<ParameterUniqueIdValuePair> GetParameterPairHistory(ulong serialNumber, string uniqueId1, string uniqueId2, DateTime from, DateTime to);
        public abstract IpLocation GetSessionLocation(string uuid);
        public abstract DeviceDescriptionPayload DownloadDeviceDescription(DeviceVersion version);
        public abstract bool UploadDeviceDescription(DeviceDescriptionPayload deviceDescription);
        public abstract bool DeviceDescriptionExists(ulong serialNumber, string hashCode);
        public abstract List<EltraDevice> GetSessionDevices(string uuid);
        public abstract bool GetSessionDevicesCount(Session session, out int count);
        public abstract bool CreateSessionLink(string uuid, List<Session> sessions);
        public abstract List<string> GetLinkedSessionUuids(string uuid, bool isMaster);
        public abstract bool SetSessionLinkStatus(string sessionUuid, SessionStatus status);
    }
}
