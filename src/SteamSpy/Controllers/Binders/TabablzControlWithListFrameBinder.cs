using Dragablz;
using Framework;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ThunderHawk
{
    public class TabablzControlWithListFrameBinder : BindingController<TabablzControl, IListFrame>
    {
        readonly List<IBindableView> _bindedElements = new List<IBindableView>();

        protected override void OnBind()
        {
            SubscribeOnPropertyChanged(Frame, nameof(IListFrame.DataSource), OnDataSourceChanged);
            OnDataSourceChanged();

            View.InterTabController = new InterTabController()
            {
                InterTabClient = new DefaultInterTabClient() //new CustomTabClient()
            };

            View.SelectedIndex = 0;
        }

        private void OnDataSourceChanged()
        {
            View.Items.Clear();
            foreach (ViewModel item in Frame.DataSource)
                View.Items.Add(CreateTabItem(item));
        }

        private TabItem CreateTabItem(ViewModel model)
        {
            var item = new TabItem()
            {
                Header = model.GetName()
            };

            var view = (IBindableView)Service<IViewFactory>.Get().CreateView(model.GetPrefix(), model.GetViewStyle());
            view.ViewModel = model;
            item.Content = view;
            
            return item;
        }

        protected override void OnUnbind()
        {
            for (int i = 0; i < _bindedElements.Count; i++)
                _bindedElements[i].ViewModel = null;

            _bindedElements.Clear();

            base.OnUnbind();
        }

        private class CustomTabClient : IInterTabClient
        {
            CustomTabHost _host;

            public INewTabHost<Window> GetNewHost(IInterTabClient interTabClient, object partition, TabablzControl source)
            {
                return _host ?? (_host = new CustomTabHost(source));
            }

            public TabEmptiedResponse TabEmptiedHandler(TabablzControl tabControl, Window window)
            {
                return TabEmptiedResponse.CloseWindowOrLayoutBranch;
            }

            private class CustomTabHost : INewTabHost<Window>
            {
                readonly TabablzControl _source;
                public Window Container { get; } = new Window();
                public TabablzControl TabablzControl => _source;

                public CustomTabHost(TabablzControl source)
                {
                    _source = source;
                }
            }
        }
    }
}
