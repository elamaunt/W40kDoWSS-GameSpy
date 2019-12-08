using System;

namespace ThunderHawk.Core
{
    public interface ISystemService
    {
        void AddInStartup();
        void OpenLink(Uri uri);
        void NotifyAsSystemToastMessage(MessageInfo info);
        void NotifyAsSystemToastMessage(string title, string text);
    }
}
