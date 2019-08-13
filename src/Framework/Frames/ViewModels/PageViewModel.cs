namespace Framework
{
    [PageViewModel]
    public abstract class PageViewModel : ViewModel
    {
        public IGlobalNavigationManager GlobalNavigationManager => Service<IGlobalNavigationManager>.Get();

        public IUserInteractions UserInteractions => GetExtension<IUserInteractions>();

        public virtual ITextFrame Title { get; } = new TextFrame() { Text = "Page" };

        public PageViewModel()
        {
            RequestViewExtention<IUserInteractions>();
        }

        public void PassData(IDataBundle bundle)
        {
            OnPassData(bundle);
        }

        protected virtual void OnPassData(IDataBundle bundle)
        {
            // Nothing
        }

        public override string GetViewStyle()
        {
            return PageHelper.GetPageViewModelName(this);
        }

        public override string GetPrefix()
        {
            return "page";
        }
    }
}
