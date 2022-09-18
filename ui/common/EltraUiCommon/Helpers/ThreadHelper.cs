using EltraUiCommon.Framework;
using System;

namespace EltraUiCommon.Helpers
{
    public static class ThreadHelper
    {
        public static int MainThreadId { get; private set; }

        public static IInvokeOnMainThread InvokeOnMainThread { get; private set; }

        public static void Initialize(int mainThreadId, IInvokeOnMainThread invokeOnMainThread)
        {
            MainThreadId = mainThreadId;
            InvokeOnMainThread = invokeOnMainThread;
        }

        public static bool IsOnMainThread => Environment.CurrentManagedThreadId == MainThreadId;

        public static void RunOnMainThread(Action function)
        {
            if (IsOnMainThread || InvokeOnMainThread == null)
            {
                function?.Invoke();
            }
            else if(InvokeOnMainThread!=null)
            {
                InvokeOnMainThread.BeginInvokeOnMainThread(function);
            }
        }
    }
}