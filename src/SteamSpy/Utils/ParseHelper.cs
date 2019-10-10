using GSMasterServer.Data;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

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

        internal static GameServerDetails ParseDetails(byte[] data)
        {
            var details = new GameServerDetails();

            return details;
        }
    }
}
