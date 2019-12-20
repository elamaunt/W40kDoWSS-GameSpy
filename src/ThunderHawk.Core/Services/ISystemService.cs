using System;
using System.Threading.Tasks;

namespace ThunderHawk.Core
{
    public interface ISystemService
    {
        bool IsSteamRunning { get; }

        string GetSteamExePath();
        bool CheckIsItInStartup();
        void AddInStartup();
        void RemoveFromStartup();
        void OpenLink(Uri uri);
        void NotifyAsSystemToastMessage(MessageInfo info);
        void NotifyAsSystemToastMessage(string title, string text);
        Task<bool> AskUser(string question);
        void ShowMessageWindow(string message);
    }
}
