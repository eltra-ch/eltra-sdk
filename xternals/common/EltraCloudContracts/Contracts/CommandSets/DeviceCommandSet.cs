using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EltraCloudContracts.Contracts.CommandSets
{
    [DataContract]
    public class DeviceCommandSet
    {
        #region Private fields

        private List<DeviceCommand> _commands;

        #endregion

        #region Properties

        [DataMember]
        public List<DeviceCommand> Commands => _commands ?? (_commands = new List<DeviceCommand>());

        #endregion

        #region Methods
        
        public bool AddCommand(DeviceCommand command)
        {
            bool result = false;

            if (!CommandExists(command))
            {
                Commands.Add(command);
                result = true;
            }

            return result;
        }

        public bool CommandExists(DeviceCommand command)
        {
            return FindCommandByName(command.Name) != null;
        }

        public DeviceCommand FindCommandByName(string name)
        {
            DeviceCommand result = null;

            foreach (var command in Commands)
            {
                if (command.Name.ToLower() == name.ToLower())
                {
                    result = command;
                    break;
                }
            }

            return result;
        }

        #endregion
    }
}
