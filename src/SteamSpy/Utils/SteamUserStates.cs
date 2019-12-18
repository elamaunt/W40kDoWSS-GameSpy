using Steamworks;
using System;
using System.Threading.Tasks;
using ThunderHawk.Core;
using ThunderHawk.Utils;

namespace ThunderHawk
{
    public static class SteamUserStates
    {
        static Callback<P2PSessionRequest_t> _sessionRequestCallback = Callback<P2PSessionRequest_t>.Create(OnSessionCallbackReceived);
        static Callback<P2PSessionConnectFail_t> _sessionConnectFailedCallback = Callback<P2PSessionConnectFail_t>.Create(OnSessionConnectFailReceived);

        public static event Action<ulong, UserState> UserSessionChanged;

        private static void OnSessionCallbackReceived(P2PSessionRequest_t param)
        {
            Logger.Info($"AcceptP2PSessionWithUser {param.m_steamIDRemote}");
            SteamNetworking.AcceptP2PSessionWithUser(param.m_steamIDRemote);
            UserSessionChanged?.Invoke(param.m_steamIDRemote.m_SteamID, GetUserState(param.m_steamIDRemote.m_SteamID));
        }

        private static void OnSessionConnectFailReceived(P2PSessionConnectFail_t param)
        {
            var error = (EP2PSessionError)param.m_eP2PSessionError;
            Logger.Info($"OnSessionConnectFailReceived {param.m_steamIDRemote} {error}");
            UserSessionChanged?.Invoke(param.m_steamIDRemote.m_SteamID, GetUserState(param.m_steamIDRemote.m_SteamID));
        }

        public static void CheckConnection(ulong steamId)
        {
            var userId = new CSteamID(steamId);

            if (CoreContext.LaunchService.GameProcess == null)
            {
                SteamAPI.RunCallbacks();
                PortBindingManager.UpdateFrame();

                SteamNetworking.SendP2PPacket(userId, new byte[] { 0 }, 1, EP2PSend.k_EP2PSendReliable, 1);

                SteamAPI.RunCallbacks();
                PortBindingManager.UpdateFrame();
            }
            else
            {
                SteamNetworking.SendP2PPacket(userId, new byte[] { 0 }, 1, EP2PSend.k_EP2PSendReliable, 1);
            }

            var state = GetUserState(steamId);

            if (state == UserState.Connecting)
                UserSessionChanged?.Invoke(steamId, state);
        }

        public static UserState GetUserState(ulong steamId)
        {
            if (SteamNetworking.GetP2PSessionState(new CSteamID(steamId), out P2PSessionState_t state))
            {
                if (state.m_bConnectionActive == 1)
                    return UserState.Connected;
                if (state.m_bConnecting == 1)
                    return UserState.Connecting;

                return UserState.Disconnected;
            }

            return UserState.Unknown;
        }
    }
}
