using System;

namespace Framework
{
    public interface IUriFrame : ITextFrame
    {
        Uri Uri { get; set; }
    }
}
