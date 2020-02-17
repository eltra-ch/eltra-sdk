using EltraCloudContracts.Contracts.CommandSets;

namespace EltraCloudStorage.Items
{
    class DbExecCommands
    {
        public int CommandId { get; set; }
        public string CommandName { get; set; }
        public ExecuteCommand Command { get; set; }
        public ExecCommandStatus Status { get; set; }
    }
}