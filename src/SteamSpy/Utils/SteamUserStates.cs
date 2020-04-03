using Steamworks;
using System;
using ThunderHawk.Core;

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
            UserSessionChanged?.Invoke(param.m_steamIDRemote.m_SteamID, UserState.Disconnected);
        }

        public static void SendSuccededTestBuffer(ulong steamId, uint bufferSize = 1, int channel = 1)
        {
            var userId = new CSteamID(steamId);

            var buffer = new byte[bufferSize];

            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = 1;

            SteamNetworking.SendP2PPacket(userId, buffer, bufferSize, EP2PSend.k_EP2PSendReliable, channel);

            var state = GetUserState(steamId);

            UserSessionChanged?.Invoke(steamId, state);
        }

        public static void SendTestBuffer(ulong steamId, uint bufferSize = 1, int channel = 1)
        {
            var userId = new CSteamID(steamId);

            var buffer = new byte[bufferSize];

            SteamNetworking.SendP2PPacket(userId, buffer, bufferSize, EP2PSend.k_EP2PSendReliable, channel);

            var state = GetUserState(steamId);

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
