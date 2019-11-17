﻿using SharedServices;

namespace ThunderHawk.Core
{
    public class PlayerInfo
    {
        public string Name { get; set; }
        public int Team { get; set; }
        public Race Race { get; set; }
        public PlayerFinalState FinalState { get; set; }
        public long Rating { get; set; }
        public long RatingDelta { get; set; }
    }
}
