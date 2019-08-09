using System;

namespace Framework
{
    public interface IActionFrame : IControlFrame
    {
        Action Action { get; set; }
    }
}