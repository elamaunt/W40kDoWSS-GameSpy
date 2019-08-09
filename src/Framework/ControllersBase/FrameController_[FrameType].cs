namespace Framework
{
    public abstract class FrameController<FrameType> : BindingController<object, FrameType>
        where FrameType : class
    {
    }
}
