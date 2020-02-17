using System;
using System.Runtime.Serialization;

namespace EltraCloudContracts.Contracts.CommandSets
{
    [DataContract]
    public class ExecuteCommandStatus
    {
        public ExecuteCommandStatus()
        {
        }

        public ExecuteCommandStatus(ExecuteCommand execCommand)
        {
            SessionUuid = execCommand.SessionUuid;
            SerialNumber = execCommand.SerialNumber;
            CommandUuid = execCommand.CommandUuid;
            
            var command = execCommand.Command;

            if (command != null)
            {
                if (!string.IsNullOrEmpty(command.Uuid))
                {
                    CommandUuid = command.Uuid;
                }

                CommandName = command.Name;
            }
        }

        [DataMember]
        public string CommandUuid { get; set; }

        [DataMember]
        public string SessionUuid { get; set; }

        [DataMember]
        public ulong SerialNumber { get; set; }

        [DataMember]
        public string CommandName { get; set; }

        [DataMember]
        public ExecCommandStatus Status { get; set; }

        [DataMember]
        public ExecCommandCommStatus CommStatus { get; set; }

        [DataMember]
        public DateTime Modified { get; set; }
    }
}
