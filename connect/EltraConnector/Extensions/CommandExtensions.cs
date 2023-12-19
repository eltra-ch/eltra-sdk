using EltraCommon.Contracts.Channels;
using EltraCommon.Contracts.CommandSets;

namespace EltraConnector.Extensions
{
    static class CommandExtensions
    {
        public static bool IsValid(this ExecuteCommand command)
        {
            bool result = false;

            if (command != null && 
                !string.IsNullOrEmpty(command.CommandId) && 
                !string.IsNullOrEmpty(command.SourceChannelId) && 
                command.Command != null)
            {
                result = true;
            }

            return result;
        }

        public static bool IsValid(this ExecuteCommandStatus status)
        {
            bool result = false;

            if (status != null && 
                !string.IsNullOrEmpty(status.CommandId) && 
                !string.IsNullOrEmpty(status.ChannelId) && 
                !string.IsNullOrEmpty(status.CommandName))
            {
                result = true;
            }

            return result;
        }

        public static bool IsValid(this ChannelStatusUpdate status)
        {
            bool result = false;

            if (status != null && !string.IsNullOrEmpty(status.ChannelId))
            {
                result = true;
            }

            return result;
        }
    }
}
