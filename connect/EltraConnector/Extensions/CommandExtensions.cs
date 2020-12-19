using EltraCommon.Contracts.Channels;
using EltraCommon.Contracts.CommandSets;

namespace EltraConnector.Extensions
{
    static class CommandExtensions
    {
        public static bool IsValid(this ExecuteCommand command)
        {
            bool result = false;

            if(command!=null)
            {
                if(!string.IsNullOrEmpty(command.CommandId))
                {
                    if (!string.IsNullOrEmpty(command.SourceChannelId))
                    {
                        if(command.Command != null)
                        {
                            result = true;
                        }
                    }
                }
            }

            return result;
        }

        public static bool IsValid(this ExecuteCommandStatus status)
        {
            bool result = false;

            if (status != null)
            {
                if (!string.IsNullOrEmpty(status.CommandId))
                {
                    if (!string.IsNullOrEmpty(status.ChannelId))
                    {
                        if (!string.IsNullOrEmpty(status.CommandName))
                        {
                            result = true;
                        }
                    }
                }
            }

            return result;
        }

        public static bool IsValid(this ChannelStatusUpdate status)
        {
            bool result = false;

            if(status!=null)
            {
                if (!string.IsNullOrEmpty(status.ChannelId))
                {
                    if (!string.IsNullOrEmpty(status.LocalHost))
                    {
                        result = true;
                    }                        
                }   
            }

            return result;
        }
    }
}
