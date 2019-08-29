namespace Framework
{
    public interface IViewFactory
    {
        object CreateView(string prefix, string name);
    }
}
