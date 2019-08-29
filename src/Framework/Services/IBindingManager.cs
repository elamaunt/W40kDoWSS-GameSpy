namespace Framework
{
    public interface IBindingManager
    {
        IBinding Bind(object view, object frame, ComponentBatch batch);
        IBinding GetBinding(object view);
    }
}