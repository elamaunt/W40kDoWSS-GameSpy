using Framework;

namespace ThunderHawk.Core
{
    public class ChatUserController : FrameController<ChatUserItemViewModel>
    {
        protected override void OnBind()
        {
            CoreContext.SteamApi.UserRichPresenceChanged += OnUserRichPresenceChanged;
            CoreContext.MasterServer.UserChanged += OnUserChanged;
            OnUserChanged(Frame.Info);
        }

        void OnUserRichPresenceChanged(ulong steamId)
        {
            RunOnUIThread(() =>
            {
                if (steamId == Frame.Info.SteamId)
                    Frame.Name.Text = Frame.Info.UIName;
            });
        }

        void OnUserChanged(UserInfo userInfo)
        {
            RunOnUIThread(() =>
            {
                if (userInfo.SteamId == Frame.Info.SteamId)
                    Frame.Name.Text = userInfo.UIName;
            });
        }

        protected override void OnUnbind()
        {
            CoreContext.MasterServer.UserChanged -= OnUserChanged;
            CoreContext.SteamApi.UserRichPresenceChanged -= OnUserRichPresenceChanged;
            base.OnUnbind();
        }
    }
}
