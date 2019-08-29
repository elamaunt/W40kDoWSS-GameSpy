namespace Framework
{
    public class ItemViewModel : ViewModel
    {
        public override string GetViewStyle()
        {
            return this.GetItemViewModelName();
        }
    }
}
