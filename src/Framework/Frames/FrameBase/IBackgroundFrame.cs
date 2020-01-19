namespace Framework
{
    public interface IBackgroundFrame : IFrame
    {
        string BackgroundColor { get; set; }
        double BackgroundOpacity { get; set; }
    }
}
