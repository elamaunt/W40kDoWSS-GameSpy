using Framework;

namespace ThunderHawk.Core
{
    public class NewsItemViewModel : ItemViewModel
    {
        public TextFrame Title { get; } = new TextFrame();
        public TextFrame Annotation { get; } = new TextFrame();
        
        public NewsItemDTO NewsItem { get; }

        public NewsItemViewModel(NewsItemDTO dto)
        {
            NewsItem = dto;

            Title.Text = dto.RussianTitle;
            Annotation.Text = dto.RussianAnnotation;
        }
    }
}
