using System;

namespace EltraNavigoEnka.Views.Login.Events
{
    public class SignStatusEventArgs : EventArgs
    {
        public SignStatus Status { get; set; }
    }
}
