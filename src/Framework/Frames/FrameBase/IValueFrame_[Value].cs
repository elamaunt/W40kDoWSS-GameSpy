namespace Framework
{
    public interface IValueFrame<ValueType> : ITextFrame
    {
        ValueType Value { get; set; }
    }
}
