﻿using System;
using EltraConnector.Master.Status;

namespace EltraConnector.Master.Events
{
    public class MasterStatusEventArgs : EventArgs
    {
        public MasterStatus Status { get; set; }
    }
}
