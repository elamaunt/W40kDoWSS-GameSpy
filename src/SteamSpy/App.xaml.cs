using GSMasterServer.Servers;
using SteamSpy.StaticClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;

namespace SteamSpy
{
    public partial class App : Application
    {
        public static bool Is64BitProcess
        {
            get { return IntPtr.Size == 8; }
        }


        protected override void OnStartup(StartupEventArgs e)
        {
            NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(Directory.GetCurrentDirectory() + "\\LauncherFiles\\NLog.config", true);

            CoreContext.Start(IPAddress.Any);

            SoulstormExtensions.Init();

            //ModifyHostsFile(Entries.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Where(x => !x.IsNullOrWhiteSpace()).Select(x => x.Split(' ')).Where(x => x.Length == 2).ToList());

            if (Is64BitProcess)
                File.Copy(Path.Combine(Environment.CurrentDirectory, "steam_api64.dll"), Path.Combine(Environment.CurrentDirectory, "steam_api.dll"), true);
            else
                File.Copy(Path.Combine(Environment.CurrentDirectory, "steam_api86.dll"), Path.Combine(Environment.CurrentDirectory, "steam_api.dll"), true);
            base.OnStartup(e);
        }

        public static void ModifyHostsFile(List<string[]> entries)
        {
            try
            {
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"drivers\etc\hosts");

                if (!File.Exists(path))
                    File.Create(path);

                var list = new List<string>();

                using (var reader = File.OpenText(path))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();

                        if (line.IsNullOrWhiteSpace())
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
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка, может не хватает прав Администратора?");
            }
        }
        
        string Entries = @"
134.209.198.2 ocs.thq.com
134.209.198.2 www.dawnofwargame.com
134.209.198.2 gmtest.master.gamespy.com

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
134.209.198.2 whamdowfr.gamestats.gamespy.com

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
134.209.198.2 whamdowfram.gamestats.gamespy.com

134.209.198.2 gamespy.net
134.209.198.2 gamespygp
134.209.198.2 motd.gamespy.com
127.0.0.1 peerchat.gamespy.com
134.209.198.2 gamestats.gamespy.com
127.0.0.1 gpcm.gamespy.com
127.0.0.1 gpsp.gamespy.com
134.209.198.2 key.gamespy.com
134.209.198.2 master.gamespy.com
134.209.198.2 master0.gamespy.com
127.0.0.1 natneg.gamespy.com
127.0.0.1 natneg0.gamespy.com
127.0.0.1 natneg1.gamespy.com
127.0.0.1 natneg2.gamespy.com
127.0.0.1 natneg3.gamespy.com
127.0.0.1 chat.gamespynetwork.com
134.209.198.2 available.gamespy.com
134.209.198.2 gamespy.com
134.209.198.2 gamespyarcade.com
134.209.198.2 www.gamespy.com
134.209.198.2 www.gamespyarcade.com
127.0.0.1 chat.master.gamespy.com
134.209.198.2 thq.vo.llnwd.net
134.209.198.2 gamespyid.com
127.0.0.1 nat.gamespy.com
";

        string LocalEntries = @"
192.168.159.1 ocs.thq.com
192.168.159.1 www.dawnofwargame.com
192.168.159.1 gmtest.master.gamespy.com

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
192.168.159.1 whamdowfr.gamestats.gamespy.com

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
192.168.159.1 whamdowfram.gamestats.gamespy.com

192.168.159.1 gamespy.net
192.168.159.1 gamespygp
192.168.159.1 motd.gamespy.com
127.0.0.1 peerchat.gamespy.com
192.168.159.1 gamestats.gamespy.com
127.0.0.1 gpcm.gamespy.com
127.0.0.1 gpsp.gamespy.com
192.168.159.1 key.gamespy.com
192.168.159.1 master.gamespy.com
192.168.159.1 master0.gamespy.com
127.0.0.1 natneg.gamespy.com
127.0.0.1 natneg0.gamespy.com
127.0.0.1 natneg1.gamespy.com
127.0.0.1 natneg2.gamespy.com
127.0.0.1 natneg3.gamespy.com
127.0.0.1 chat.gamespynetwork.com
192.168.159.1 available.gamespy.com
192.168.159.1 gamespy.com
192.168.159.1 gamespyarcade.com
192.168.159.1 www.gamespy.com
192.168.159.1 www.gamespyarcade.com
127.0.0.1 chat.master.gamespy.com
192.168.159.1 thq.vo.llnwd.net
192.168.159.1 gamespyid.com
127.0.0.1 nat.gamespy.com
";
    }
}
