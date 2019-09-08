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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using IrcD.Channel;
using IrcD.Commands.Arguments;
using IrcD.Core.Utils;
using IrcD.Modes;
using IrcD.Tools;
using Enumerable = System.Linq.Enumerable;

namespace IrcD.Core
{

    public class UserInfo : InfoBase
    {
        // Костыль: Замена отправки по сокету для шифрования
        readonly Func<object, string, int> _send;
        readonly object _state;

        public long ProfileId { get; }

        public UserInfo(IrcDaemon ircDaemon, Socket socket, long profileId, string host, bool isAcceptSocket, bool passAccepted, object state, Func<object, string, int> send)
            : base(ircDaemon)
        {
            _send = send;
            _state = state;
            ProfileId = profileId;
            IsService = false;
            Registered = false;
            PassAccepted = passAccepted;
            Host = host;
            Created = DateTime.Now;
            Capabilities = null;

            IsAcceptSocket = isAcceptSocket;
            Socket = socket;

            Modes = new UserModeList(ircDaemon);
        }

        internal Socket Socket { get; }

        public bool IsAcceptSocket { get; }
        public bool PassAccepted { get; internal set; }
        public bool Registered { get; internal set; }
        public bool IsService { get; set; }

        public string User { get; private set; }
        public string Nick { get; private set; }
        public string RealName { get; private set; }
        public string Host { get; private set; }
        public string AwayMessage { get; set; }

        public Game Game { get; set; }

        public List<string> Capabilities { get; private set; }

        private IEnumerable<string> _languages = new List<string> { "en" };

        public IEnumerable<string> Languages
        {
            get
            {
                return _languages.Any() ? _languages : Enumerable.Repeat("en", 1);
            }
            set
            {
                _languages = value.Where(l => Tools.Languages.All.ContainsKey(l));
            }
        }


        public void InitNick(string nick)
        {
            if (NickExists)
            {
                throw new AlreadyCalledException();
            }

            Nick = nick;

            if (!UserExists) return;

            RegisterComplete();
        }

        public void InitUser(string user, string realname)
        {
            if (UserExists)
            {
                throw new AlreadyCalledException();
            }

            User = user;
            RealName = realname;

            if (!NickExists) return;

            RegisterComplete();
        }

        private void RegisterComplete()
        {
            Logger.Log($"New User: {Usermask}");

            //IrcDaemon.Replies.RegisterComplete(this);
            Registered = true;
            if (IsService)
            {
                IrcDaemon.Replies.SendYouAreService(this);
                IrcDaemon.Replies.SendYourHost(this);
                IrcDaemon.Replies.SendMyInfo(this);
            }
            else
            {
                IrcDaemon.Replies.RegisterComplete(this);
            }
        }


        internal bool UserExists => User != null;

        public bool NickExists => Nick != null;

        public void Rename(string newNick)
        {
            // Update Global Nick-Dictionary
            IrcDaemon.Nicks.TryRemove(Nick, out UserInfo oldUser);
            IrcDaemon.Nicks[newNick] = this;

            // Update Channel Nicklists
            foreach (var channel in Channels)
            {
                var channelInfo = channel.UserPerChannelInfos[Nick];
                channel.UserPerChannelInfos.TryRemove(Nick, out UserPerChannelInfo info);
                channel.UserPerChannelInfos[newNick] = channelInfo;
            }

            Nick = newNick;
        }

        public string Usermask => Nick + "!" + User + "@" + Host;

        public string Prefix => ":" + Usermask;

        public DateTime LastAction { get; set; } = DateTime.Now;

        public DateTime LastAlive { get; set; }

        public DateTime LastPing { get; set; }


        public ConcurrentDictionary<string, UserPerChannelInfo> UserPerChannelInfos { get; } = new ConcurrentDictionary<string, UserPerChannelInfo>();

        public IEnumerable<ChannelInfo> Channels => UserPerChannelInfos.Select(pair => pair.Value.ChannelInfo);

        public List<ChannelInfo> Invited { get; } = new List<ChannelInfo>();

        public UserModeList Modes { get; }

        public string ModeString => Modes.ToUserModeString();

        public DateTime Created { get; private set; }
        public string UserFlags { get; set; }
        public string UserStats { get; set; }

        public override string ToString()
        {
            return Nick + "!" + User + "@" + Host;
        }

        public int WriteLine(string line)
        {
#if DEBUG
            //Logger.Log(line.ToString(), location: "OUT:" + Nick);
#endif
            // Костыль дла переопределения отправки
            return _send(_state, line + IrcDaemon.ServerCrLf);
            //return Socket.Send(Encoding.UTF8.GetBytes(line + IrcDaemon.ServerCrLf));
        }

        public override int WriteLine(StringBuilder line)
        {
#if DEBUG
            Logger.Log(line.ToString(), location: "OUT:" + Nick);
#endif
            // Костыль дла переопределения отправки
            return _send(_state, line + IrcDaemon.ServerCrLf);
            //return Socket.Send(Encoding.UTF8.GetBytes(line + IrcDaemon.ServerCrLf));
        }

        public override int WriteLine(StringBuilder line, UserInfo exception)
        {
            if (this != exception)
            {
                return WriteLine(line);
            }

            return 0;

        }

        // Cleanly Quit a user, in any case, Connection dropped, QuitMesssage, all traces of 'this' must be removed.
        public void Remove(string message)
        {
            // Clean up channels
            foreach (var upci in UserPerChannelInfos.Select(x => x.Value).Reverse<UserPerChannelInfo>())
            {
                // Important: remove nick first! or we end in a exception-catch endless loop
                upci.ChannelInfo.RemoveUser(this);
                IrcDaemon.Commands.Send(new QuitArgument(this, upci.ChannelInfo, message));
            }

            // Clean up server

            UserInfo info;

            if (Nick != null)
                IrcDaemon.Nicks.TryRemove(Nick, out info);

            IrcDaemon.Users.TryRemove(ProfileId, out info);

            // Close connection
            (info?._state as IDisposable)?.Dispose();

            // Ready for destruction
        }

        internal static string NormalizeHostmask(string parameter)
        {
            var hasAt = parameter.Contains('@');
            var hasEx = parameter.Contains('!');

            if (!hasAt && !hasEx)
            {
                if (parameter.Contains('.'))
                {
                    parameter = "*!*@" + parameter;

                }
                else
                {
                    parameter = parameter + "!*@*";
                }
            }

            if (hasEx && parameter.First() == '!')
            {
                parameter = "*" + parameter;
            }

            if (hasAt && parameter.Last() == '@')
            {
                parameter = parameter + "*";
            }

            return parameter;
        }

        public void WriteServerPrivateMessage(string message)
        {
            WriteLine($@":SERVER!XaaaaaaaaX|10008@127.0.0.1 PRIVMSG {Nick} :{message}");
        }
    }
}
