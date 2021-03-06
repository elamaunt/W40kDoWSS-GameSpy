﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ThunderHawk.HostsFixer
{
    public static class HostsFixer
    {
        [STAThread]
        static int Main(string[] args)
        {
            try
            {
                ModifyHostsFile(Entries.Split(new string[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Split(' ')).Where(x => x.Length == 2).ToList());
            }
            catch
            {
                return 1;
            }

            return 0;
        }

        static void ModifyHostsFile(List<string[]> entries)
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers", "etc", "hosts");

            if (!File.Exists(path))
                File.Create(path);

            var list = new List<string>();

            using (var reader = File.OpenText(path))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();

                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var commentStart = line.IndexOf("#");

                    string[] parts;

                    if (commentStart != -1)
                    {
                        parts = line.Substring(0, commentStart).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    else
                    {
                        parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    }

                    if (parts.Length != 2)
                    {
                        list.Add(line);
                        continue;
                    }

                    var hostName = parts[1];
                    var address = parts[0];

                    var entry = entries.FirstOrDefault(x => x[1] == hostName);

                    if (entry != null)
                    {
                        entries.Remove(entry);

                        if (entry[0] == address)
                        {
                            list.Add(line);
                        }
                        else
                        {
                            list.Add(line.Replace(address, entry[0]));
                        }
                    }
                }

                for (int i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];
                    list.Add(entry[0] + " " + entry[1]);
                }
            }

            using (var stream = File.Create(path))
            {
                using (var writer = new StreamWriter(stream))
                {
                    for (int i = 0; i < list.Count; i++)
                        writer.WriteLine(list[i]);
                }
            }
        }

        static string Entries = $@"
{GameConstants.SERVER_ADDRESS} ocs.thq.com
{GameConstants.SERVER_ADDRESS} www.dawnofwargame.com
{GameConstants.SERVER_ADDRESS} gmtest.master.gamespy.com

127.0.0.1 whamdowfr.master.gamespy.com
127.0.0.1 whamdowfr.gamespy.com
127.0.0.1 whamdowfr.ms9.gamespy.com
127.0.0.1 whamdowfr.ms11.gamespy.com
127.0.0.1 whamdowfr.available.gamespy.com
127.0.0.1 whamdowfr.available.gamespy.com
127.0.0.1 whamdowfr.natneg.gamespy.com
127.0.0.1 whamdowfr.natneg0.gamespy.com
127.0.0.1 whamdowfr.natneg1.gamespy.com
127.0.0.1 whamdowfr.natneg2.gamespy.com
127.0.0.1 whamdowfr.natneg3.gamespy.com
{GameConstants.SERVER_ADDRESS} whamdowfr.gamestats.gamespy.com

127.0.0.1 whamdowfram.master.gamespy.com
127.0.0.1 whamdowfram.gamespy.com
127.0.0.1 whamdowfram.ms9.gamespy.com
127.0.0.1 whamdowfram.ms11.gamespy.com
127.0.0.1 whamdowfram.available.gamespy.com
127.0.0.1 whamdowfram.available.gamespy.com
127.0.0.1 whamdowfram.natneg.gamespy.com
127.0.0.1 whamdowfram.natneg0.gamespy.com
127.0.0.1 whamdowfram.natneg1.gamespy.com
127.0.0.1 whamdowfram.natneg2.gamespy.com
127.0.0.1 whamdowfram.natneg3.gamespy.com
{GameConstants.SERVER_ADDRESS} whamdowfram.gamestats.gamespy.com

{GameConstants.SERVER_ADDRESS} gamespy.net
{GameConstants.SERVER_ADDRESS} gamespygp
{GameConstants.SERVER_ADDRESS} motd.gamespy.com
127.0.0.1 peerchat.gamespy.com
{GameConstants.SERVER_ADDRESS} gamestats.gamespy.com
127.0.0.1 gpcm.gamespy.com
127.0.0.1 gpsp.gamespy.com
{GameConstants.SERVER_ADDRESS} key.gamespy.com
{GameConstants.SERVER_ADDRESS} master.gamespy.com
{GameConstants.SERVER_ADDRESS} master0.gamespy.com
127.0.0.1 natneg.gamespy.com
127.0.0.1 natneg0.gamespy.com
127.0.0.1 natneg1.gamespy.com
127.0.0.1 natneg2.gamespy.com
127.0.0.1 natneg3.gamespy.com
127.0.0.1 chat.gamespynetwork.com
{GameConstants.SERVER_ADDRESS} available.gamespy.com
{GameConstants.SERVER_ADDRESS} gamespy.com
{GameConstants.SERVER_ADDRESS} gamespyarcade.com
{GameConstants.SERVER_ADDRESS} www.gamespy.com
{GameConstants.SERVER_ADDRESS} www.gamespyarcade.com
127.0.0.1 chat.master.gamespy.com
{GameConstants.SERVER_ADDRESS} thq.vo.llnwd.net
{GameConstants.SERVER_ADDRESS} gamespyid.com
127.0.0.1 nat.gamespy.com
";
    }
}

