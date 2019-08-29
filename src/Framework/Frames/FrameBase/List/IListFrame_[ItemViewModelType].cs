namespace Framework
{
    public interface IListFrame<ItemViewModelType> : IListFrameIn<ItemViewModelType>, IListFrameOut<ItemViewModelType>
      where ItemViewModelType : ViewModel
    {
    }
}
