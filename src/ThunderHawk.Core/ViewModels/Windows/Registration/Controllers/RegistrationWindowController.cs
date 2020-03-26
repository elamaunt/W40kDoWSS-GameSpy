using Framework;

namespace ThunderHawk.Core
{
    class RegistrationWindowController : FrameController<RegistrationWindowViewModel>
    {
        private MainPageController _mainPageController;

        protected override void OnBind()
        {
            if (!CoreContext.MasterServer.IsConnected) Frame.HelpLabel.Text = "Server is unavailable";

            Frame.Register.Action = RegisterAction;
            CoreContext.MasterServer.RegistrationByLauncherReceived += RegistrationByLauncherReceivedInWindow;
        }

        void ChangePath()
        {
            // TODO: проработать авторизацию
        }

        void RegisterAction()
        {
            if (CoreContext.AccountService.RegInputFieldPassword == "")
            {
                Frame.HelpLabel.Text = CoreContext.LangService.GetString("PasswordsNotInput");
                return;
            }

            if (!CoreContext.AccountService.RegInputFieldPassword.Equals(CoreContext.AccountService
                .RegInputFieldConfirmPassword))
            {
                Frame.HelpLabel.Text = CoreContext.LangService.GetString("PasswordsNotMatch");
                return;
            }

            CoreContext.AccountService.RegisterRequest(CoreContext.AccountService.RegInputFieldLogin,
                CoreContext.AccountService.RegInputFieldPassword);
            Frame.HelpLabel.Text = "";
        }

        public RegistrationWindowController(MainPageController mainPageController)
        {
            _mainPageController = mainPageController;
        }

        void RegistrationByLauncherReceivedInWindow(bool registrationSuccess)
        {
            if (!registrationSuccess)
            {
                Frame.HelpLabel.Text = CoreContext.LangService.GetString("AccountAlreadyUse");
            }
            else
            {
                CoreContext.AccountService.WritePlayerCfgLoginPass(CoreContext.AccountService.RegInputFieldLogin,
                    CoreContext.AccountService.RegInputFieldPassword);
                CoreContext.MasterServer.RegistrationByLauncherReceived -= RegistrationByLauncherReceivedInWindow;
                Frame.GlobalNavigationManager.CloseWindow("Registration");
                _mainPageController.LaunchThunderhawk();
            }
        }
    }
}