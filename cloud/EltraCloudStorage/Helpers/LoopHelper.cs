using System;
using System.Threading;

namespace EltraCloudStorage.Helpers
{
    class LoopHelper
    {
        #region Private fields

        private uint _loops;

        #endregion

        #region Constructors

        public LoopHelper()
        {
            SleepTime = 1;
            SpinTime = 20;
        }

        #endregion

        #region Properties

        public int SleepTime { get; set; }

        public int SpinTime { get; set; }

        #endregion

        #region Methods

        public void Wait(bool forceSleep = false)
        {
            if (forceSleep || Environment.ProcessorCount == 1 || (++_loops % 100) == 0)
            {
                Thread.Sleep(SleepTime);
            }
            else
            {
                Thread.SpinWait(SpinTime);
            }
        }

        public void Reset()
        {
            _loops = 0;
        }
        
        #endregion
    }
}
