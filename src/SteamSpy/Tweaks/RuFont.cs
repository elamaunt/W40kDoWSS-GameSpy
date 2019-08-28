using System;
using System.IO;
using ThunderHawk.Core.Services;
using ThunderHawk.StaticClasses.Soulstorm;

namespace ThunderHawk.Tweaks
{
    public class RuFont : ITweak
    {
        public string TweakTitle => Core.CoreContext.LangService.GetString("RuFontTweakTitle");

        public string TweakDescription => Core.CoreContext.LangService.GetString("RuFontTweakDescription");

        public bool IsRecommendedTweak { get; } = true;

        public bool CheckTweak()
        {
            var gamePath = PathFinder.GamePath;
            var targetDir = Path.Combine(gamePath, "Engine", "Locale", "English", "data", "font");

            return Directory.Exists(targetDir) && Directory.GetFiles(targetDir).Length >= 12; // Very rude check but fast
        }

        public void EnableTweak()
        {

            var fontsDir = Path.Combine("GameFiles", "Tweaks", "RuFont");
            if (!Directory.Exists(fontsDir))
                throw new Exception("Could not find RuFont in GameFiles!");

            //TODO: Сделать более качественную проверку на наличие файлов в GameFiles


            var fontFiles = Directory.GetFiles(fontsDir);
            var gamePath = PathFinder.GamePath;
            var targetDir = Path.Combine(gamePath, "Engine", "Locale", "English", "data", "font");
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }
            foreach(var font in fontFiles)
            {
                File.Copy(font, Path.Combine(targetDir, Path.GetFileName(font)), true);
            }
        }

        public void DisableTweak()
        {
            var gamePath = PathFinder.GamePath;
            var targetDir = Path.Combine(gamePath, "Engine", "Locale", "English", "data", "font");
            if (Directory.Exists(targetDir))
            {
                var files = Directory.GetFiles(targetDir);
                foreach(var file in files)
                {
                    File.Delete(file);
                }
            }
        }
    }
}
