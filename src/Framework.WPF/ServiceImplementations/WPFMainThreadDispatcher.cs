using System;
using System.Threading;
using System.Windows;

namespace Framework.WPF
{
    internal class WPFMainThreadDispatcher : IMainThreadDispatcher
    {
        public bool IsMainThread => SynchronizationContext.Current != null;

        public void InvokeOnMainThread(Action action)
        {
            if (IsMainThread)
                action();
            else
                Application.Current.Dispatcher.BeginInvoke(action);
        }
    }
}
