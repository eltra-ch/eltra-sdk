namespace MPlayerMaster
{
    internal static class MPlayerDefinitions
    {
        #region Enums

        internal enum StatusWordEnums
        {
            Undefined = 0x0,
            Waiting = 0x0001,
            PendingExecution = 0x0010,
            ExecutedSuccessfully = 0x0020,
            ExecutionFailed = 0x8000
        }

        #endregion
    }
}
