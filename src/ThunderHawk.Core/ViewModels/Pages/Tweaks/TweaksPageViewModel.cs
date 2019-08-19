using Framework;

namespace ThunderHawk.Core
{
    public class TweaksPageViewModel: EmbeddedPageViewModel
    {
        public ListFrame<TweakItemViewModel> Tweaks { get; } = new ListFrame<TweakItemViewModel>();

        public TweaksPageViewModel()
        {
            Tweaks.DataSource.Add(new TweakItemViewModel("Разблокировщик рас", "Мы заметили, что у вас доступны не все расы. " +
                "Это может создать значительный дискомфорт в игре и неравные возможности с вашим противником.", "Разблокировать расы!", "Заблокировать расы обратно"));
        }
    }
}
