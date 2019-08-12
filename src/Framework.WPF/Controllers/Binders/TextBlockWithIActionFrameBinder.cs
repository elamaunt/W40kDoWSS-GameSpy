using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Framework.WPF
{
    public class TextBlockWithIActionFrameBinder : BindingController<TextBlock, IActionFrame>
    {
        protected override void OnBind()
        {
            foreach (var item in View.Inlines)
            {
                var link = item as Hyperlink;

                if (link != null)
                    link.Click += OnClick;
            }
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            Frame.Action?.Invoke();
        }

        protected override void OnUnbind()
        {
            foreach (var item in View.Inlines)
            {
                var link = item as Hyperlink;

                if (link != null)
                    link.Click -= OnClick;
            }
            base.OnUnbind();
        }
    }
}
