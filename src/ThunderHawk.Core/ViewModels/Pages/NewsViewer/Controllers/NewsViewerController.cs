using Framework;
using System.Globalization;

namespace ThunderHawk.Core
{
    public class NewsViewerController : FrameController<NewsViewerPageViewModel>
    {
        protected override void OnBind()
        {
            CoreContext.LangService.CultureChanged += UpdateText;
            UpdateText(CoreContext.LangService.CurrentCulture);
        }

        void UpdateText(CultureInfo culture)
        {
            if (culture.TwoLetterISOLanguageName == "ru")
            {
                Frame.TitleButton.Text = Frame.NewsItem.Russian.Title?.ToUpperInvariant();
                Frame.Annotation.Text = Frame.NewsItem.Russian.Annotation;
                Frame.Text.Text = Frame.NewsItem.Russian.Body;
            }
            else
            {
                Frame.TitleButton.Text = Frame.NewsItem.English.Title?.ToUpperInvariant();
                Frame.Annotation.Text = Frame.NewsItem.English.Annotation;
                Frame.Text.Text = Frame.NewsItem.English.Body;
            }
        }

        protected override void OnUnbind()
        {
            CoreContext.LangService.CultureChanged -= UpdateText;
            base.OnUnbind();
        }
    }
}
