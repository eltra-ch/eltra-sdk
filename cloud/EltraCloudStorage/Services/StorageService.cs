using System;
using System.Collections.Generic;
using EltraCloudContracts.Contracts.CommandSets;
using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.Contracts.Parameters;
using EltraCloudContracts.Contracts.Sessions;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.DeviceDescription;
using EltraCloudStorage.Items;

namespace EltraCloudStorage.Services
{
    public class StorageService : IStorageService
    {
        #region Private fields

        private readonly string _engine;
        
        #endregion

        #region Constructors

        public StorageService()
        {
            _engine = "mysql"; //or sqlite
        }

        #endregion

        #region Properties

        public string ConnectionString { get; set; }

        #endregion

        #region Methods
        
        #region Factory
        
        private SessionStorageItem CreateSessionStorage()
        {
            var result = new SessionStorageItem {Engine = _engine, ConnectionString = ConnectionString };

            return result;
        }

        #endregion

        #region Service interface

        public override IpLocation GetSessionLocation(string uuid)
        {
            IpLocation result = null;

            using (var sessionStorage = CreateSessionStorage())
            {
                if (sessionStorage != null)
                {
                    result = sessionStorage.GetSessionLocation(uuid);
                }
            }

            return result;
        }

        public override List<EltraDevice> GetSessionDevices(string uuid)
        {
            var result = new List<EltraDevice>();

            using (var sessionStorage = CreateSessionStorage())
            {
                if (sessionStorage != null)
                {
                    result = sessionStorage.GetSessionDevices(uuid);
                }
            }

            return result;
        }

        public override bool GetSessionDevicesCount(Session session, out int count)
        {
            bool result = false;

            count = 0;

            if (session != null)
            {
                using (var sessionStorage = CreateSessionStorage())
                {
                    if (sessionStorage != null)
                    {
                        result = sessionStorage.GetSessionDevicesCount(session.Uuid, out count);
                    }
                }
            }

            return result;
        }

        public override bool UpdateSession(Session session)
        {
            bool result = false;

            using (var sessionStorage = CreateSessionStorage())
            {
                if (sessionStorage != null)
                {
                    result = sessionStorage.UpdateSession(session);
                }
            }

            return result;
        }
        
        public override bool SetSessionLinkStatus(string sessionUuid, SessionStatus status)
        {
            bool result = false;

            using (var sessionStorage = CreateSessionStorage())
            {
                if (sessionStorage != null)
                {
                    result = sessionStorage.UpdateSessionLinksStatus(sessionUuid, status);
                }
            }

            return result;
        }

        public override bool SetSessionStatus(string sessionUuid, SessionStatus sessionStatus)
        {
            bool result = false;

            using (var sessionStorage = CreateSessionStorage())
            { 
                if (sessionStorage != null)
                {
                    result = sessionStorage.UpdateSessionStatus(sessionUuid, sessionStatus);
                }
            }

            return result;
        }
        
        public override bool RemoveDevice(ulong serialNumber)
        {
            bool result = false;
            using (var sessionStorage = CreateSessionStorage())
            {
                if (sessionStorage != null)
                {
                    result = sessionStorage.RemoveDevice(serialNumber);
                }
            }

            return result;
        }

        public override bool UpdateParameter(ParameterUpdate parameterUpdate)
        {
            bool result = false;
            using (var sessionStorage = CreateSessionStorage())
            {
                if (sessionStorage != null)
                {
                    result = sessionStorage.UpdateParameter(parameterUpdate);
                }
            }

            return result;
        }

        public override ParameterValue GetParameterValue(ulong serialNumber, ushort index, byte subIndex)
        {
            ParameterValue result = null;

            using (var sessionStorage = CreateSessionStorage())
            {
                if (sessionStorage != null)
                {
                    result = sessionStorage.GetParameterValue(serialNumber, index, subIndex);
                }
            }

            return result;
        }

        public override List<ParameterValue> GetParameterHistory(ulong serialNumber, string uniqueId, DateTime from, DateTime to)
        {
            var result = new List<ParameterValue>();

            using (var sessionStorage = CreateSessionStorage())
            {
                if (sessionStorage != null)
                {
                    result = sessionStorage.GetParameterHistory(serialNumber, uniqueId, from, to);
                }
            }

            return result;
        }

        public override List<ParameterUniqueIdValuePair> GetParameterPairHistory(ulong serialNumber, string uniqueId1, string uniqueId2, DateTime from, DateTime to)
        {
            var result = new List<ParameterUniqueIdValuePair>();

            using (var sessionStorage = CreateSessionStorage())
            {
                if (sessionStorage != null)
                {
                    result = sessionStorage.GetParameterPairHistory(serialNumber, uniqueId1, uniqueId2, from, to);
                }
            }

            return result;
        }

        public override bool AddSessionDevice(SessionDevice sessionDevice)
        {
            bool result = false;
            using (var sessionStorage = CreateSessionStorage())
            {
                if (sessionStorage != null)
                {
                    result = sessionStorage.AddSessionDevice(sessionDevice);
                }
            }

            return result;
        }

        public override bool SetCommand(ExecuteCommand executeCommand)
        {
            bool result = false;
            using (var sessionStorage = CreateSessionStorage())
            {
                if (sessionStorage != null)
                {
                    result = sessionStorage.SetCommand(executeCommand);
                }
            }

            return result;
        }

        public override bool SetCommandStatus(ExecuteCommandStatus executeCommandStatus)
        {
            bool result = false;

            using (var sessionStorage = CreateSessionStorage())
            {
                if (sessionStorage != null)
                {
                    result = sessionStorage.SetCommandStatus(executeCommandStatus);
                }
            }

            return result;
        }

        public override bool SetCommandCommStatus(ExecuteCommandStatus executeCommandStatus)
        {
            bool result = false;

            using (var sessionStorage = CreateSessionStorage())
            {
                if (sessionStorage != null)
                {
                    result = sessionStorage.SetCommandCommStatus(executeCommandStatus);
                }
            }

            return result;
        }
        
        public override List<Session> GetSessions(SessionStatus status, bool deviceOnly)
        {
            var result = new List<Session>();

            using (var sessionStorage = CreateSessionStorage())
            {
                result.AddRange(sessionStorage?.GetSessions(status, deviceOnly));
            }

            return result;
        }

        public override List<Session> GetSessions(string login, string password, SessionStatus status)
        {
            var result = new List<Session>();

            using (var sessionStorage = CreateSessionStorage())
            {
                result.AddRange(sessionStorage?.GetSessions(login, password, status));
            }

            return result;
        }

        public override Session GetSession(string uuid)
        {
            Session result;

            using (var sessionStorage = CreateSessionStorage())
            {
                result = sessionStorage?.GetSession(uuid);
            }

            return result;
        }

        public override SessionStatus GetSessionStatus(string uuid)
        {
            var result = SessionStatus.Undefined;

            using (var sessionStorage = CreateSessionStorage())
            {
                result = sessionStorage.GetSessionStatus(uuid);
            }

            return result;
        }

        public override bool CreateSessionLink(string uuid, List<Session> sessions)
        {
            bool result = false;

            using (var sessionStorage = CreateSessionStorage())
            {
                result = sessionStorage.CreateSessionLink(uuid, sessions);
            }

            return result;
        }

        public override List<string> GetLinkedSessionUuids(string uuid, bool isMaster)
        {
            var result = new List<string>();

            using (var sessionStorage = CreateSessionStorage())
            {
                result = sessionStorage.GetLinkedSessionUuids(uuid, isMaster);
            }

            return result;
        }

        public override Session GetDeviceSession(ulong serialNumber, SessionStatus status)
        {
            Session result;

            using (var sessionStorage = CreateSessionStorage())
            {
                result = sessionStorage?.GetDeviceSession(serialNumber, status);
            }

            return result;
        }

        public override ExecuteCommandStatus GetCommandStatus(string uuid, string sessionUuid, ulong serialNumber, string commandName)
        {
            ExecuteCommandStatus result;

            using (var sessionStorage = CreateSessionStorage())
            {
                result = sessionStorage?.GetCommandStatus(uuid, sessionUuid, serialNumber, commandName);
            }

            return result;
        }

        public override EltraDevice GetDevice(ulong serialNumber, SessionStatus status)
        {
            EltraDevice result;

            using (var sessionStorage = CreateSessionStorage())
            {
                result = sessionStorage?.GetDevice(serialNumber, status);
            }

            return result;
        }
        
        public override ExecuteCommand PopCommand(string uuid, ulong serialNumber, ExecCommandStatus status)
        {
            ExecuteCommand result;

            using (var sessionStorage = CreateSessionStorage())
            {
                result = sessionStorage?.PopCommand(uuid, serialNumber, status);
            }
            
            return result;
        }

        public override List<ExecuteCommand> PopCommands(ulong serialNumber, ExecCommandStatus status)
        {
            List<ExecuteCommand> result;

            using (var sessionStorage = CreateSessionStorage())
            {
                result = sessionStorage?.PopCommands(serialNumber, status);
            }
            
            return result;
        }

        public override List<ExecuteCommand> PopCommands(ulong[] serialNumber, ExecCommandStatus[] status)
        {
            List<ExecuteCommand> result;

            using (var sessionStorage = CreateSessionStorage())
            {
                result = sessionStorage?.PopCommands(serialNumber, status);
            }

            return result;
        }

        public override List<ExecuteCommand> GetExecCommands(string sessionUuid, ExecCommandStatus[] status)
        {
            List<ExecuteCommand> result;

            using (var sessionStorage = CreateSessionStorage())
            {
                result = sessionStorage?.GetExecCommands(sessionUuid, status);
            }

            return result;
        }

        public override string GetDeviceOwner(string sessionUuid, ulong serialNumber)
        {
            string result = string.Empty;

            using (var sessionStorage = CreateSessionStorage())
            {
                if (sessionStorage != null)
                {
                    result = sessionStorage.GetDeviceOwnerSessionId(serialNumber);
                }
            }

            return result;
        }

        public override bool LockDevice(string sessionUuid, ulong serialNumber, string agentUuid)
        {
            bool result = false;
            using (var sessionStorage = CreateSessionStorage())
            {
                if (sessionStorage != null)
                {
                    result = sessionStorage.LockDevice(serialNumber, agentUuid);
                }
            }

            return result;
        }

        public override bool UnlockDevice(string sessionUuid, ulong serialNumber)
        {
            bool result = false;
            
            using (var sessionStorage = CreateSessionStorage())
            {
                if (sessionStorage != null)
                {
                    result = sessionStorage.UnlockDevice(serialNumber);
                }
            }

            return result;
        }

        public override bool IsDeviceUsedByAgent(string agentSesionUuid, ulong serialNumber)
        {
            bool result = false;

            using (var sessionStorage = CreateSessionStorage())
            {
                if (sessionStorage != null)
                {
                    result = sessionStorage.IsDeviceUsedByAgent(agentSesionUuid, serialNumber);
                }
            }

            return result;
        }

        #region Device Description

        public override DeviceDescriptionPayload DownloadDeviceDescription(DeviceVersion version)
        {
            DeviceDescriptionPayload result = null;

            using (var sessionStorage = CreateSessionStorage())
            {
                if (sessionStorage != null)
                {
                    result = sessionStorage.DownloadDeviceDescription(version);
                }
            }

            return result;
        }

        public override bool UploadDeviceDescription(DeviceDescriptionPayload deviceDescription)
        {
            bool result = false;

            using (var sessionStorage = CreateSessionStorage())
            {
                if (sessionStorage != null)
                {
                    result = sessionStorage.UploadDeviceDescription(deviceDescription);
                }
            }

            return result;
        }

        public override bool DeviceDescriptionExists(ulong serialNumber, string hashCode)
        {
            bool result = false;

            using (var sessionStorage = CreateSessionStorage())
            {
                if (sessionStorage != null)
                {
                    result = sessionStorage.DeviceDescriptionExists(serialNumber, hashCode);
                }
            }

            return result;
        }

        #endregion

        #endregion

        #region Users

        public override bool UserExists(string userName)
        {
            bool result = false;

            using (var sessionStorage = CreateSessionStorage())
            {
                if (sessionStorage != null)
                {
                    result = sessionStorage.UserExists(userName);
                }
            }

            return result;
        }

        public override bool IsUserValid(string userName, string password)
        {
            bool result = false;

            using (var sessionStorage = CreateSessionStorage())
            {
                if (sessionStorage != null)
                {
                    result = sessionStorage.IsUserValid(userName, password);
                }
            }

            return result;
        }

        public override bool RegisterUser(string loginName, string userName, string password)
        {
            bool result = false;

            using (var sessionStorage = CreateSessionStorage())
            {
                if (sessionStorage != null)
                {
                    result = sessionStorage.RegisterUser(loginName, userName, password);
                }
            }

            return result;
        }

        public override bool SignInUser(string userName, string password, out string token)
        {
            bool result = false;

            token = string.Empty;

            using (var sessionStorage = CreateSessionStorage())
            {
                if (sessionStorage != null)
                {
                    result = sessionStorage.SignInUser(userName, password, out token);
                }
            }

            return result;
        }

        public override bool SignOutUser(string token)
        {
            bool result = false;

            using (var sessionStorage = CreateSessionStorage())
            {
                if (sessionStorage != null)
                {
                    result = sessionStorage.SignOutUser(token);
                }
            }

            return result;
        }

        #endregion
    }
    #endregion
}
