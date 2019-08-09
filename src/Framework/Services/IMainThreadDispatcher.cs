using System;

namespace Framework
{
    public interface IMainThreadDispatcher
    {
        bool IsMainThread { get; }
        
        void InvokeOnMainThread(Action action);
    }
}
