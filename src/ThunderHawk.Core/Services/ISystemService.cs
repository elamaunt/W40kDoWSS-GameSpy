using System;

namespace ThunderHawk.Core
{
    public interface ISystemService
    {
        void OpenLink(Uri uri);
        void NotifyAboutMessage(MessageInfo info);
    }
}
