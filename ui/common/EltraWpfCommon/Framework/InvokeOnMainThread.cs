using EltraUiCommon.Framework;
using System;

namespace EltraWpfCommon.Framework
{
    public class InvokeOnMainThread : IInvokeOnMainThread
    {
        public void BeginInvokeOnMainThread(Action action)
        {
            action?.Invoke();
        }
    }
}
