using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ThunderHawk.Core
{
    public interface IAccountService
    {
        bool IsAuthorized { get; }
        
        string AuthInputFieldLogin { get; set; }
        string AuthInputFieldPassword { get; set; }
        string RegInputFieldLogin { get; set; }
        string RegInputFieldPassword { get; set; }
        string RegInputFieldConfirmPassword { get; set; }
        void SendCheckAuthorizedOnSsProfile();

        void WritePlayerCfgLoginPass(string login, string password);
        void CanAuthorizeRequest(string login, string password);
        void RegisterRequest(string login, string password);
    }
}
