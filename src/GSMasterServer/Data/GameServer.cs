using System;

namespace GSMasterServer.Data
{
	internal class NonFilterAttribute : Attribute
	{
	}

	internal class GameServer
	{
        [NonFilter]
		public bool Valid { get; set; }

		[NonFilter]
		public string IPAddress { get; set; }

		[NonFilter]
		public int QueryPort { get; set; }

		[NonFilter]
		public DateTime LastRefreshed { get; set; }

		[NonFilter]
		public DateTime LastPing { get; set; }


		[NonFilter]
		public string localip0 { get; set; }

		[NonFilter]
		public string localip1 { get; set; }

		[NonFilter]
		public int localport { get; set; }

		[NonFilter]
		public bool natneg { get; set; }

		[NonFilter]
		public int statechanged { get; set; }

		public string country { get; set; }
		public string hostname { get; set; }
		public string gamename { get; set; }
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
        public bool devmode { get; set; }
    }
}
