using Lidgren.Network;
using SharedServices;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ThunderHawk.Core;

namespace ThunderHawk
{
    public partial class SingleMasterServer
    {
        public bool IsInLobbyNow => _currentlobbyData != null;
        public bool HasHostedLobby => IsInLobbyNow && (_currentlobbyData.Host?.SteamId ?? 0ul) == CoreContext.SteamApi.SteamId;

        public event Action<EnteredInLobbyMessage> EnterInLobbyDataReceived;

        public event Action<ulong, string, long> LobbyMemberLeft;
        public event Action<ulong, string> LobbyChatMessage;

        public event Action<GameServerDetails[]> LobbiesReceived;

        LobbyData _currentlobbyData;

        public void UpdateCurrentLobby(GameServerDetails details, string indicator)
        {
            var mes = _clientPeer.CreateMessage();
            mes.WriteJsonMessage(new UpdateLobbyMessage()
            {
                Indicator = indicator,
                Properties = details.Properties
            });
            _clientPeer.SendMessage(mes, NetDeliveryMethod.ReliableOrdered);
        }

        public void LeaveFromCurrentLobby()
        {
            var mes = _clientPeer.CreateMessage();
            mes.WriteJsonMessage(new LeaveLobbyMessage());
            _clientPeer.SendMessage(mes, NetDeliveryMethod.ReliableOrdered);

            _currentlobbyData = null;
        }

        public void SendInLobbyChat(string line)
        {
            var mes = _clientPeer.CreateMessage();
            mes.WriteJsonMessage(new LobbyChatLineMessage()
            {
                Line = line
            });
            _clientPeer.SendMessage(mes, NetDeliveryMethod.ReliableOrdered);
        }

        public void SetLobbyKeyValue(string key, string value)
        {
            var mes = _clientPeer.CreateMessage();
            mes.WriteJsonMessage(new LobbyKeyValueMessage()
            {
                Key = key,
                Value = value
            });
            _clientPeer.SendMessage(mes, NetDeliveryMethod.ReliableOrdered);
        }

        public void SetLobbyTopic(string topic)
        {
            // nothing
        }

        public void SetLobbyGameStarted()
        {
            var mes = _clientPeer.CreateMessage();
            mes.WriteJsonMessage(new LobbyGameStartedMessage());
            _clientPeer.SendMessage(mes, NetDeliveryMethod.ReliableOrdered);
        }

        public string GetLocalLobbyMemberData(string key)
        {
            return _currentlobbyData?.LocalPlayer?.GetKeyValue(key);
        }

        public int GetLobbyMembersCount()
        {
            return _currentlobbyData?.Members?.Count ?? -1;
        }

        public string GetLobbyMemberName(int i)
        {
            return _currentlobbyData?.Members?.ElementAtOrDefault(i)?.Name;
        }

        public string GetLobbyMemberData(int i, string key)
        {
            return _currentlobbyData?.Members?.ElementAtOrDefault(i)?.GetKeyValue(key);
        }

        public int GetCurrentLobbyMaxPlayers()
        {
            return _currentlobbyData?.MaxMembers ?? -1;
        }

        public void SetCurrentLobbyMaxPlayers(int value)
        {
            var mes = _clientPeer.CreateMessage();
            mes.WriteJsonMessage(new UpdateLobbyMessage()
            {
                MaxPlayers = value
            });
            _clientPeer.SendMessage(mes, NetDeliveryMethod.ReliableOrdered);
        }

        public string GetLobbyTopic()
        {
            return "Topic";
        }

        public string[] GetCurrentLobbyMembers()
        {
            return _currentlobbyData?.Members?.Select(x => x.Name).ToArray() ?? new string[0];
        }

        public void EnterInLobby(ulong hostSteamId, string localRoomHash, string shortUser, string name, string profileId)
        {
            var mes = _clientPeer.CreateMessage();
            mes.WriteJsonMessage(new EnterLobbyMessage()
            {
                HostId = hostSteamId,
                LocalRoomHash = localRoomHash,
                ShortUser = shortUser,
                Name = name,
                ProfileId = profileId
            });
            _clientPeer.SendMessage(mes, NetDeliveryMethod.ReliableOrdered);
        }

        public Task<GameServerDetails[]> LoadLobbies(string gameVariant, string indicator, bool filterByMod = false)
        {
            var tcs = new TaskCompletionSource<GameServerDetails[]>();
            var cts = new CancellationTokenSource();

            Action<GameServerDetails[]> handler = null;

            handler = lobbies =>
            {
                LobbiesReceived -= handler;
                tcs.SetResult(lobbies);
                cts.Dispose();
            };

            cts.Token.Register(() =>
                {
                    LobbiesReceived -= handler;
                    tcs.TrySetResult(new GameServerDetails[0]);
                    cts.Dispose();
                });

            LobbiesReceived += handler;
            cts.CancelAfter(5000);

            var mes = _clientPeer.CreateMessage();
            mes.WriteJsonMessage(new RequestLobbiesMessage());
            _clientPeer.SendMessage(mes, NetDeliveryMethod.ReliableOrdered);

            return tcs.Task;
        }

        public void SetLobbyJoinable(bool joinable)
        {
            var mes = _clientPeer.CreateMessage();
            mes.WriteJsonMessage(new UpdateLobbyMessage()
            {
                Joinable = joinable
            });
            _clientPeer.SendMessage(mes, NetDeliveryMethod.ReliableOrdered);
        }

        public void CreatePublicLobby(string name, string shortUser, string flags, string indicator)
        {
            var mes = _clientPeer.CreateMessage();
            mes.WriteJsonMessage(new CreateLobbyMessage()
            {
                Name = name,
                ShortUser = shortUser, 
                Flags = flags, 
                Indicator = indicator
            });
            _clientPeer.SendMessage(mes, NetDeliveryMethod.ReliableOrdered);
        }
    }
}
