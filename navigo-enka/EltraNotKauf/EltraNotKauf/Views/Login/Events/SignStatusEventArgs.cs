using System;

namespace EltraNotKauf.Views.Login.Events
{
    public class SignStatusEventArgs : EventArgs
    {
        public SignStatus Status { get; set; }
    }
}
