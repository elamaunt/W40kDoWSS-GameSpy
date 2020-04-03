namespace Framework
{
    public interface IControlFrame : IFrame
    {
        bool Visible { get; set; }
        bool Enabled { get; set; }
        IMenuFrame ContextMenu { get; set; }
    }
}
