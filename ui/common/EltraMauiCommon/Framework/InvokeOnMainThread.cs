using EltraUiCommon.Framework;
using Microsoft.Maui.ApplicationModel;
using System;

namespace EltraMauiCommon.Framework
{
    public class InvokeOnMainThread : IInvokeOnMainThread
    {
        public void BeginInvokeOnMainThread(Action action)
        {
            MainThread.BeginInvokeOnMainThread(action);
        }
    }
}
