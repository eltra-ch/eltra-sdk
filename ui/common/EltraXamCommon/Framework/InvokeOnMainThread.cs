using EltraUiCommon.Framework;
using System;

namespace EltraXamCommon.Framework
{
    public class InvokeOnMainThread : IInvokeOnMainThread
    {
        public void BeginInvokeOnMainThread(Action action)
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(action);    
        }
    }
}
