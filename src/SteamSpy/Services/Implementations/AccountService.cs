using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ThunderHawk.Core;
using ThunderHawk.StaticClasses.Soulstorm;
using ThunderHawk.Utils;

namespace ThunderHawk
{
    public class AccountService : IAccountService
    {
        public AccountService()
        {
            // для проверки авторизации пользователя
        }

        public bool IsAuthorized { get; } = false;

        public string AuthInputFieldLogin { get; set; } = "";
        public string AuthInputFieldPassword { get; set; } = "";
        public string RegInputFieldLogin { get; set; } = "";
        public string RegInputFieldPassword { get; set; } = "";
        public string RegInputFieldConfirmPassword { get; set; } = "";

        public void SendCheckAuthorizedOnSsProfile()
        {
            var (login, pass) = readPlayerCfgLoginPass();
            CanAuthorizeRequest(login, pass);
        }

        public void RegisterRequest(string login, string password)
        {
            CoreContext.MasterServer.RequestRegistration(login, password);
        }

        public void CanAuthorizeRequest(string login, string password)
        {
            CoreContext.MasterServer.RequestCanAuthorize(login, password);
        }

        // Тут мы читаем файл с профилем, чтобы использовать запомненный пароль
        private (string, string) readPlayerCfgLoginPass()
        {
            try
            {
                string activeProfilePath = "";

                string playerName = null;
                string playerPassword = null;

                var profiles = Directory.GetDirectories(Path.Combine(PathFinder.GamePath, "Profiles"));
                DateTime timeStamp = DateTime.MinValue;
                foreach (var profile in profiles)
                {
                    DateTime profileLastActivity = File.GetLastWriteTimeUtc(profile + "\\playercfg.lua");
                    if (timeStamp < profileLastActivity)
                    {
                        timeStamp = profileLastActivity;
                        activeProfilePath = profile;
                    }
                }

                var playercfgPath = Path.Combine(activeProfilePath, "playercfg.lua");
                FileInfo playercfg = new FileInfo(playercfgPath);

                using (var streamReader =
                    new StreamReader(playercfg.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    while (streamReader.Peek() > -1)
                    {
                        var line = streamReader.ReadLine();

                        if (line == null)
                            continue;

                        var str = "nick_name = ";
                        var index = line.IndexOf(str, StringComparison.OrdinalIgnoreCase);
                        if (index != -1)
                        {
                            playerName = Regex.Replace(line, ".*?\"(.*?)\".*", "$1");
                        }

                        str = "password = ";
                        index = line.IndexOf(str, StringComparison.OrdinalIgnoreCase);
                        if (index != -1)
                        {
                            playerPassword = Regex.Replace(line, ".*?\"(.*?)\".*", "$1");
                        }
                    }
                }

                return (playerName, playerPassword);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return (null, null);
            }
        }

        public void WritePlayerCfgLoginPass(string login, string password)
        {
            string credentionalString = "player_info =  \r\n" +
                                        "{\r\n" +
                                        "    email_address = \"thunderhawk@dowonline.steam\",\r\n" +
                                        "    last_room = \"ThunderHawk\",\r\n" +
                                        "    nick_name = \""+ login +"\",\r\n" +
                                        "    password = \""+ password +"\",\r\n" +
                                        "}";

            string playerCfgToWrite;

            try
            {
                string activeProfilePath = "";

                string playerName = null;
                string playerPassword = null;

                var profiles = Directory.GetDirectories(Path.Combine(PathFinder.GamePath, "Profiles"));
                DateTime timeStamp = DateTime.MinValue;
                foreach (var profile in profiles)
                {
                    DateTime profileLastActivity = File.GetLastWriteTimeUtc(profile + "\\playercfg.lua");
                    if (timeStamp < profileLastActivity)
                    {
                        timeStamp = profileLastActivity;
                        activeProfilePath = profile;
                    }
                }

                var playercfgPath = Path.Combine(activeProfilePath, "playercfg.lua");

                string playercfg;

                using (StreamReader sr = new StreamReader(playercfgPath))
                {
                    playercfg = sr.ReadToEnd();
                    sr.Close();
                }

                // Модифицируем playerConfig
                if (playercfg.Contains("player_info ="))
                {
                    // если уже была авторизация на геймспае, заменяем её на новую
                    playerCfgToWrite = Regex.Replace(playercfg, "player_info = ((.|\r\n)*?)}", credentionalString);
                }
                else
                {
                    playerCfgToWrite = playercfg + credentionalString;
                }

                using (StreamWriter sw = new StreamWriter(playercfgPath, false, System.Text.Encoding.Default))
                {
                    sw.WriteLine(playerCfgToWrite);
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}