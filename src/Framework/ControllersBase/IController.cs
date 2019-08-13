namespace Framework
{
    public interface IController
    {
        bool Bind(object view, object frame);
        void Unbind();
    }
}