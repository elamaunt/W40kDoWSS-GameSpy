using System;

namespace Framework
{
    public static class Dispatcher
    {
        static IMainThreadDispatcher Service => Service<IMainThreadDispatcher>.Get();

        public static bool IsMainThread => Service.IsMainThread;

        public static void RunOnMainThread(Action action)
        {
            Service.InvokeOnMainThread(action);
        }
    }
}
