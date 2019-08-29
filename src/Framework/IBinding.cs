using System;

namespace Framework
{
    public interface IBinding : IDisposable
    {
        object Frame { get; }

        void Unbind();
    }
}