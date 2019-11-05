using GSMasterServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using ThunderHawk.Data;

namespace ThunderHawk.Utils
{
    public static class ParseHelper
    {
        public static Dictionary<string, string> ParseMessage(string message, out string query)
        {
            var parsedData = new Dictionary<string, string>();

            string[] responseData = message.Split(new string[] { @"\" }, StringSplitOptions.None);

            if (responseData.Length > 1)
            {
                query = responseData[1];
            }
            else
            {
                query = string.Empty;
                return null;
            }

            for (int i = 1; i < responseData.Length - 1; i += 2)
            {
                if (parsedData.ContainsKey(responseData[i]))
                {
                    parsedData[responseData[i].ToLowerInvariant()] = responseData[i + 1];
                }
                else
                {
                    parsedData.Add(responseData[i].ToLowerInvariant(), responseData[i + 1]);
                }
            }

            return parsedData;
        }

        internal static GameServerDetails ParseDetails(string serverVars)
        {
            var serverVarsSplit = serverVars.Split(new string[] { "\x00" }, StringSplitOptions.None);

            var details = new GameServerDetails();

            for (int i = 0; i < serverVarsSplit.Length - 1; i += 2)
            {
                if (serverVarsSplit[i] == "hostname")
                    details.Set(serverVarsSplit[i], Regex.Replace(serverVarsSplit[i + 1], @"\s+", " ").Trim());
                else
                    details.Set(serverVarsSplit[i], serverVarsSplit[i + 1]);
            }

            return details;
        }

        public static byte[] PackServerList(IPEndPoint remoteEndPoint, IEnumerable<GameServerDetails> servers, string[] fields, bool isAutomatch)
        {
            List<byte> data = new List<byte>();

            data.AddRange(remoteEndPoint.Address.GetAddressBytes());

            byte[] value2 = BitConverter.GetBytes((ushort)remoteEndPoint.Port);
            data.AddRange(BitConverter.IsLittleEndian ? value2.Reverse() : value2);

            if (fields.Length == 1 && fields[0] == "\u0004")
                fields = new string[0];

            data.Add((byte)fields.Length);
            data.Add(0);

            foreach (var field in fields)
            {
                data.AddRange(Encoding.UTF8.GetBytes(field));
                data.AddRange(new byte[] { 0, 0 });
            }

            PortBindingManager.ClearPortBindings();

            foreach (var server in servers)
            {
                if (server.Properties.TryGetValue("gamename", out string gamename))
                {
                    if (isAutomatch && gamename != "whamdowfram")
                        continue;

                    if (!isAutomatch && gamename != "whamdowfr")
                        continue;
                }

                var localip0 = server.GetOrDefault("localip0");
                var localport = ushort.Parse(server.GetOrDefault("localport") ?? "0");
                var queryPort = ushort.Parse(server.GetOrDefault("QueryPort"));
                var iPAddress = server.GetOrDefault("IPAddress");

                var retranslator = PortBindingManager.AddOrUpdatePortBinding(server.HostSteamId);

                retranslator.AttachedServer = server;

                ushort retranslationPort = retranslator.Port;

                var retranslationPortBytes = BitConverter.IsLittleEndian ? BitConverter.GetBytes(retranslationPort).Reverse() : BitConverter.GetBytes(retranslationPort);

                server["hostport"] = retranslationPort.ToString();
                server["localport"] = retranslationPort.ToString();

                var flags = ServerFlags.UNSOLICITED_UDP_FLAG |
                    ServerFlags.PRIVATE_IP_FLAG |
                    ServerFlags.NONSTANDARD_PORT_FLAG |
                    ServerFlags.NONSTANDARD_PRIVATE_PORT_FLAG |
                    ServerFlags.HAS_KEYS_FLAG;

                var loopbackIpBytes = IPAddress.Loopback.GetAddressBytes();

                data.Add((byte)flags);
                data.AddRange(loopbackIpBytes);
                data.AddRange(retranslationPortBytes);
                data.AddRange(loopbackIpBytes);
                data.AddRange(retranslationPortBytes);

                data.Add(255);

                for (int i = 0; i < fields.Length; i++)
                {
                    var name = fields[i];
                    var f = GetField(server, name);

                    data.AddRange(Encoding.UTF8.GetBytes(f));

                    if (i < fields.Length - 1)
                    {
                        data.Add(0);
                        data.Add(255);
                    }
                }

                data.Add(0);
            }

            data.Add(0);
            data.Add(255);
            data.Add(255);
            data.Add(255);
            data.Add(255);

            return data.ToArray();
        }

        private static string GetField(GameServerDetails server, string fieldName)
        {
            var value = server.GetOrDefault(fieldName);
            if (value == null)
                return string.Empty;
            return value;
        }
    }
}
