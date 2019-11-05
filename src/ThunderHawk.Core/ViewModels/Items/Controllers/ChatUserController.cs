using System;
using Framework;

namespace ThunderHawk.Core
{
    public class ChatUserController : FrameController<ChatUserItemViewModel>
    {
        protected override void OnBind()
        {
            CoreContext.MasterServer.UserChanged += OnUserChanged;
            OnUserChanged(Frame.Info);
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
            base.OnUnbind();
        }
    }
}
