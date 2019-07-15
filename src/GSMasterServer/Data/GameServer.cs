using System;
using System.Collections.Generic;
using System.Linq;

namespace GSMasterServer.Data
{
    internal class GameServer
	{
        //static readonly Dictionary<string, Func<GameServer, object>> _gettesCache = new Dictionary<string, Func<GameServer, object>>();
        public bool Valid { get; set; }


        /*

         public string IPAddress { get; set; }

         public int QueryPort { get; set; }

         public DateTime LastRefreshed { get; set; }

         public DateTime LastPing { get; set; }

         public string localip0 { get; set; }

         public string localip1 { get; set; }

         public int localport { get; set; }

         public bool natneg { get; set; }

         public int statechanged { get; set; }

         public string country { get; set; }
         public string hostname { get; set; }
         public string gamename
         {
             get => "whamdofr";
             set { }
         }
         public string gamever { get; set; }
         public string mapname { get; set; }
         public string gametype { get; set; }
         public string gamevariant { get; set; }
         public int numplayers { get; set; }
         public int maxplayers { get; set; }
         public string gamemode { get; set; }
         public bool password { get; set; }
         public int hostport { get; set; }
         public int numwaiting { get; set; }
         public int maxwaiting { get; set; }
         public int numservers { get; set; }
         public int numplayersname { get; set; }
         public int score_ { get; set; }

         public bool teamplay  { get; set; }
         public int groupid { get; set; }
         public int numobservers { get; set; }
         public int maxobservers { get; set; }
         public string modname { get; set; }
         public string moddisplayname { get; set; }
         public string modversion { get; set; }
         public bool devmode { get; set; }*/
         
        public readonly Dictionary<string, object> Properties = new Dictionary<string, object>();

        public object this[string fieldName]
        {
            get => GetByName(fieldName);
            set => Properties[fieldName] = value;
        }

        internal object GetByName(string fieldName)
        {
            Properties.TryGetValue(fieldName, out object value);
            return value;

            /*if (_gettesCache.TryGetValue(fieldName, out Func<GameServer, object> getter))
                return getter(this);
            else
                return (_gettesCache[fieldName] = typeof(GameServer).GetProperty(fieldName).GetValue)(this);*/
        }

        internal T Get<T>(string fieldName)
        {
            try
            {
                Properties.TryGetValue(fieldName, out object value);
                return (T)value;
            }
            catch(Exception ex)
            {
                return default(T);
            }
        }

        public void Set(string fieldName, object value)
        {
            Properties[fieldName] = value;
        }

        public string[] GetKeys()
        {
            return Properties.Keys.ToArray();
        }
    }
}
