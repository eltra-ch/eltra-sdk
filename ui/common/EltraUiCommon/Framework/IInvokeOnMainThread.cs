using System;

namespace EltraUiCommon.Framework
{
    public interface IInvokeOnMainThread
    {
        void BeginInvokeOnMainThread(Action action);
    }
}
