using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSMasterServer.Data
{
    public class GameServer
	{
        public bool Valid { get; set; }

        public CSteamID HostSteamId { get; set; }
        public bool HasPlayers => Get<string>("numplayers ") != "0";
        
        public readonly Dictionary<string, string> Properties = new Dictionary<string, string>();

        public string this[string fieldName]
        {
            get => GetByName(fieldName);
            set => Properties[fieldName] = value;
        }

        internal string GetByName(string fieldName)
        {
            Properties.TryGetValue(fieldName, out string value);
            return value;
        }

        internal T Get<T>(string fieldName)
        {
            try
            {
                Properties.TryGetValue(fieldName, out string value);
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch(Exception ex)
            {
                return default(T);
            }
        }

        public void Set(string fieldName, string value)
        {
            Properties[fieldName] = value;
        }

        public string[] GetKeys()
        {
            return Properties.Keys.ToArray();
        }
    }
}
