using System;
using System.Threading;

namespace EltraUiCommon.Controls.Definitions
{
    public class AutoUpdateRegistration
    {
        public Thread Thread { get; set; }

        public Guid Id { get; set; }

        internal void Wait()
        {
            if (Thread.ThreadState == ThreadState.Running ||
                Thread.ThreadState == ThreadState.Background)
            {
                Thread.Join();
            }
        }
    }
}
