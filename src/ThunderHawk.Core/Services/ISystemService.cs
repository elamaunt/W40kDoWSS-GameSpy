using System;

namespace ThunderHawk.Core
{
    public interface ISystemService
    {
        void OpenLink(Uri uri);
        void NotifyAsSystemToastMessage(MessageInfo info);
        void NotifyAsSystemToastMessage(string title, string text);
    }
}
