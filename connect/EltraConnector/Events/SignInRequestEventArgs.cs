using System;

namespace EltraConnector.Events
{
    /// <summary>
    /// SignInRequestEventArgs
    /// </summary>
    internal class SignInRequestEventArgs : EventArgs
    {
        public bool SignInResult { get; set; }
    }
}
