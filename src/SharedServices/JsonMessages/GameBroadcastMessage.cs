﻿namespace SharedServices
{
    public class GameBroadcastMessage
    {
        public bool Teamplay { get; set; }
        public bool Ranked { get; set; }
        public string GameVariant { get; set; }
        public int MaxPlayers { get; set; }
        public int Players { get; set; }
    }
}
