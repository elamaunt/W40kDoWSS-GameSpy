﻿namespace SharedServices
{
    public class UserNameChangedMessage
    {
        public ulong SteamId { get; set; }
        public long ActiveProfileId { get; set; }
        public string NewName { get; set; }
    }
}
