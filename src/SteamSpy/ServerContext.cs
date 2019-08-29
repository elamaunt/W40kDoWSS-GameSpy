using GSMasterServer.Servers;
using System;
using System.Net;
using System.Runtime.CompilerServices;
using ThunderHawk.Core;

namespace ThunderHawk
{
    internal static class ServerContext
    {
        public static ServerListReport ServerListReport { get; private set; }
        public static ServerListRetrieve ServerListRetrieve { get; private set; }
        public static LoginServerRetranslator LoginMasterServer { get; private set; }
        public static ChatServerRetranslator ChatServer { get; private set; }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Start(IPAddress bind)
        {
            ServerListReport = new ServerListReport(bind, 27900);
            ServerListRetrieve = new ServerListRetrieve(bind, 28910);
            LoginMasterServer = new LoginServerRetranslator(bind, 29900, 29901);
            ChatServer = new ChatServerRetranslator(bind, 6667);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Stop()
        {
            try
            {
                ServerListReport?.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                ServerListReport = null;
            }

            try
            {
                ServerListRetrieve?.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                ServerListRetrieve = null;
            }

            try
            {
                LoginMasterServer?.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                LoginMasterServer = null;
            }

            try
            {
                ChatServer?.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                ChatServer = null;
            }
        }
    }
}