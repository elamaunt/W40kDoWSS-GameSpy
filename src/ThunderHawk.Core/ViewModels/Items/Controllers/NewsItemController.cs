using System;
using System.Globalization;
using Framework;

namespace ThunderHawk.Core
{
    public class NewsItemController : FrameController<NewsItemViewModel>
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
                Frame.Title.Text = Frame.NewsItem.Russian.Title;
                Frame.Annotation.Text = Frame.NewsItem.Russian.Annotation;
            }
            else
            {
                Frame.Title.Text = Frame.NewsItem.English.Title;
                Frame.Annotation.Text = Frame.NewsItem.English.Annotation;
            }
        }

        protected override void OnUnbind()
        {
            CoreContext.LangService.CultureChanged -= UpdateText;
            base.OnUnbind();
        }
    }
}
