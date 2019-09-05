using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ThunderHawk.HostsFixer
{
    public static class HostsFixer
    {
        static string IP;

        static int Main(string[] args)
        {
            try
            {
                IP = args[0];

                string entries = $@"
{IP} ocs.thq.com
{IP} www.dawnofwargame.com
{IP} gmtest.master.gamespy.com

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
{IP} whamdowfr.gamestats.gamespy.com

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
{IP} whamdowfram.gamestats.gamespy.com

{IP} gamespy.net
{IP} gamespygp
{IP} motd.gamespy.com
127.0.0.1 peerchat.gamespy.com
{IP} gamestats.gamespy.com
127.0.0.1 gpcm.gamespy.com
127.0.0.1 gpsp.gamespy.com
{IP} key.gamespy.com
{IP} master.gamespy.com
{IP} master0.gamespy.com
127.0.0.1 natneg.gamespy.com
127.0.0.1 natneg0.gamespy.com
127.0.0.1 natneg1.gamespy.com
127.0.0.1 natneg2.gamespy.com
127.0.0.1 natneg3.gamespy.com
127.0.0.1 chat.gamespynetwork.com
{IP} available.gamespy.com
{IP} gamespy.com
{IP} gamespyarcade.com
{IP} www.gamespy.com
{IP} www.gamespyarcade.com
127.0.0.1 chat.master.gamespy.com
{IP} thq.vo.llnwd.net
{IP} gamespyid.com
127.0.0.1 nat.gamespy.com
";

                if (string.IsNullOrWhiteSpace(IP))
                    return 1;

                ModifyHostsFile(entries.Split(new string[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries)
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
    }
}

