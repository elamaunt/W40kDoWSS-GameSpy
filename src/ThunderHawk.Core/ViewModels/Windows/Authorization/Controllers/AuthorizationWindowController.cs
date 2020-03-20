using System;
using System.Threading.Tasks;
using Framework;

namespace ThunderHawk.Core
{
    class AuthorizationWindowController : FrameController<AuthorizationWindowViewModel>
    {
        public AuthorizationWindowController(MainPageController mainPageController)
        {
            _mainPageController = mainPageController;
        }

        private MainPageController _mainPageController;

        protected override void OnBind()
        {
            if (!CoreContext.MasterServer.IsConnected)
            {
                Frame.AccountsLabel.Text = "Server is unavailable";
            }

            CoreContext.MasterServer.RequestAllUserNicks("это тут нах не нужно, по стим ID придет");
            Frame.Authorize.Action = AuthorizeOnServer;
            Frame.CreateAnotherAccount.Action = CreateAnotherAccountWindow;
            CoreContext.MasterServer.NicksReceived += PrintUserNicks;
            CoreContext.MasterServer.CanAuthorizeReceived += CanAuthorizeReceiveInWindow;
        }

        void AuthorizeOnServer()
        {
            CoreContext.AccountService.CanAuthorizeRequest(CoreContext.AccountService.AuthInputFieldLogin,
                CoreContext.AccountService.AuthInputFieldPassword);
            Frame.AccountsLabel.Text = "";
        }

        void CreateAnotherAccountWindow()
        {
            CoreContext.MasterServer.NicksReceived -= PrintUserNicks;
            CoreContext.MasterServer.CanAuthorizeReceived -= CanAuthorizeReceiveInWindow;
            Frame.GlobalNavigationManager.CloseWindow("Authorization");
            Frame.GlobalNavigationManager.OpenWindow<RegistrationWindowViewModel>();
        }

        void PrintUserNicks(string[] nicks)
        {
            Frame.AccountsLabel.Text =
                CoreContext.LangService.GetString("YourAccounts") + ": " + string.Join(", ", nicks);
        }



        void CanAuthorizeReceiveInWindow(bool canAuthorize)
        {
            if (!canAuthorize)
            {
                Frame.AccountsLabel.Text = "Wrong login or password";
            }
            else
            {
                CoreContext.AccountService.WritePlayerCfgLoginPass(CoreContext.AccountService.AuthInputFieldLogin, CoreContext.AccountService.AuthInputFieldPassword);
                CoreContext.MasterServer.NicksReceived -= PrintUserNicks;
                CoreContext.MasterServer.CanAuthorizeReceived -= CanAuthorizeReceiveInWindow;
                Frame.GlobalNavigationManager.CloseWindow("Authorization");
                _mainPageController.LaunchThunderhawk();
            }
        }
    }
}