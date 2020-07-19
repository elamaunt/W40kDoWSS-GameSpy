using System;
using System.IO;
using System.Linq;
using ThunderHawk.Core;
using ThunderHawk.Core.Services;
using ThunderHawk.StaticClasses.Soulstorm;

namespace ThunderHawk.Tweaks
{
    public class GridHotkeys : ITweak
    {
        public string TweakTitle => CoreContext.LangService.GetString("HotKeysTweakTitle");

        public string TweakDescription => CoreContext.LangService.GetString("HotKeysTweakDescription");

        public bool IsRecommendedTweak { get; } = false;

        private string GetCurrentProfileName(string gamePath)
        {
            var localConfig = Path.Combine(gamePath, "Local.ini");
            if (!File.Exists(localConfig))
                return "";

            var profileLine = File.ReadAllLines(localConfig).FirstOrDefault(l => l.StartsWith("playerprofile"));
            if (profileLine == null)
                return "";
            var equPos = profileLine.IndexOf('=');
            var profileName = profileLine.Substring(equPos + 1);
            return profileName;
        }
        public bool CheckTweak()
        {
            var gridKeys = Path.Combine("GameFiles", "Tweaks", "GridKeys", "dxp2", "KEYDEFAULTS.LUA");

            if (!File.Exists(gridKeys))
                throw new Exception("Could not find GridKeys in GameFiles!");

            var gamePath = PathFinder.GamePath;
            var profileName = GetCurrentProfileName(gamePath);
            var profilePath = Path.Combine(gamePath, "Profiles", profileName, "dxp2");
            var keyDefPath = Path.Combine(profilePath, "KEYDEFAULTS.LUA");

            return File.Exists(keyDefPath) && File.ReadLines(gridKeys).SequenceEqual(File.ReadLines(keyDefPath));
        }

        public void EnableTweak()
        {
            var gridKeys = Path.Combine("GameFiles", "Tweaks", "GridKeys", "dxp2", "KEYDEFAULTS.LUA");

            if (!File.Exists(gridKeys))
                throw new Exception("Could not find GridKeys in GameFiles!");

            var gamePath = PathFinder.GamePath;
            var profileName = GetCurrentProfileName(gamePath);
            var profilePath = Path.Combine(gamePath, "Profiles", profileName, "dxp2");
            var keyDefPath = Path.Combine(profilePath, "KEYDEFAULTS.LUA");

            if (!Directory.Exists(profilePath))
                Directory.CreateDirectory(profilePath);
            File.Copy(gridKeys, keyDefPath, true);
        }

        public void DisableTweak()
        {
            var gamePath = PathFinder.GamePath;
            var profileName = GetCurrentProfileName(gamePath);
            var profilePath = Path.Combine(gamePath, "Profiles", profileName, "dxp2");
            var keyDefPath = Path.Combine(profilePath, "KEYDEFAULTS.LUA");

            if (File.Exists(keyDefPath))
                File.Delete(keyDefPath);

        }
    }
}
