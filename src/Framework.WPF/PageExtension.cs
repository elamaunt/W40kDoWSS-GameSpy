using System.Windows.Controls;

namespace Framework.WPF
{
    public abstract class PageExtension : IPageExtension
    {
        protected Page Page { get; private set; }

        public virtual void OnExtended(Page view, PageViewModel viewModel)
        {
            Page = view;
        }

        public virtual void CleanUp()
        {
            Page = null;
        }
    }
}
