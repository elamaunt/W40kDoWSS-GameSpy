using System;
using ThunderHawk.Core;

namespace ThunderHawk
{
    public class SystemService : ISystemService
    {
        public void OpenLink(Uri uri)
        {
            try
            {
                System.Diagnostics.Process.Start(uri.ToString());
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}
