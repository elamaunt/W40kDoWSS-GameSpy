using Framework;
using System;

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
                Frame.ActiveProfile.Value = Frame.Info.IsProfileActive;

                if (steamId == Frame.Info.SteamId)
                    Frame.Name.Text = Frame.Info.UIName;
            });
        }

        void OnUserChanged(UserInfo userInfo)
        {
            RunOnUIThread(() =>
            {
                try
                {
                    Frame.ActiveProfile.Value = Frame.Info.IsProfileActive;

                    if (userInfo.SteamId == Frame.Info.SteamId)
                    {
                        Frame.State.Value = userInfo.State;
                        Frame.Name.Text = userInfo.UIName;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
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
