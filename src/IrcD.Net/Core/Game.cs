/*
 *  The ircd.net project is an IRC deamon implementation for the .NET Plattform
 *  It should run on both .NET and Mono
 *  
 * Copyright (c) 2009-2017, Thomas Bruderer, apophis@apophis.ch All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * * Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 *   
 * * Redistributions in binary form must reproduce the above copyright notice,
 *   this list of conditions and the following disclaimer in the documentation
 *   and/or other materials provided with the distribution.
 *
 * * Neither the name of ArithmeticParser nor the names of its
 *   contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace IrcD.Core
{
    public class Game
    {
        internal readonly Dictionary<UserInfo, bool> Users = new Dictionary<UserInfo, bool>();
        
        public IrcDaemon IrcDaemon { get; }

        public Game(IrcDaemon daemon, UserInfo[] users)
        {
            IrcDaemon = daemon;

            for (int i = 0; i < users.Length; i++)
            {
                var user = users[i];
                user.Game = this;
                Users.Add(user, true);
            }
        }

        public bool Clean()
        {
            Users.Clear();
            return IrcDaemon.CleanGame(this);
        }

        internal void SetPlayerAsLeft(UserInfo info)
        {
            Users[info] = false;

            if (Users.All(x => !x.Value))
                IrcDaemon.CleanGameWithoutWinners(this);
        }
    }
}