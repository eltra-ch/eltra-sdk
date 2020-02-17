namespace EltraCloud.Services
{
#pragma warning disable CS1591

    /// <summary>
    /// AuthState
    /// </summary>
    public enum AuthState
    {
        Undefined,
        Success,
        NoUser,
        NoAuth,
        UserAlreadyRegistered,
        Error
    }
}
