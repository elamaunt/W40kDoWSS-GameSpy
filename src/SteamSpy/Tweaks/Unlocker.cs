using Microsoft.Win32;
using System;
using System.IO;
using ThunderHawk.Core.Services;
using ThunderHawk.StaticClasses.Soulstorm;

namespace ThunderHawk.Tweaks
{
    public class Unlocker : ITweak
    {
        public string TweakTitle => Core.CoreContext.LangService.GetString("UnlockerTweakTitle");
        public string TweakDescription => Core.CoreContext.LangService.GetString("UnlockerTweakDescription");
        public bool IsRecommendedTweak { get; } = true;

        public bool CheckTweak()
        {
            var thqKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("Software\\THQ");
            if (thqKey == null)
                return false;

            var dowReg = thqKey.OpenSubKey("Dawn Of War");
            var dcReg = thqKey.OpenSubKey("Dawn Of War - Dark Crusade");
            var ssReg = thqKey.OpenSubKey("Dawn Of War - Soulstorm");

            if (dowReg == null || dcReg == null || ssReg == null)
                return false;

            string dowCdKey = (string)dowReg.GetValue("CDKEY");
            string waCdKey = (string)dowReg.GetValue("CDKEY_WXP");
            string dcCdKey = (string)dcReg.GetValue("CDKEY");
            string ssCdKey = (string)ssReg.GetValue("CDKEY");


            return dowCdKey?.Length == 19 && waCdKey?.Length == 24 && dcCdKey?.Length == 24 && ssCdKey?.Length == 24
                && (string)dcReg.GetValue("w40kcdkey") == dowCdKey && (string)dcReg.GetValue("wxpcdkey") == waCdKey
                && (string)ssReg.GetValue("w40kcdkey") == dowCdKey && (string)ssReg.GetValue("wxpcdkey") == waCdKey && (string)ssReg.GetValue("dxp2cdkey") == dcCdKey
                && File.Exists(dowReg.GetValue("installlocation") + "w40k.exe") && File.Exists(dowReg.GetValue("installlocation") + "w40kwa.exe")
                && (File.Exists(dcReg.GetValue("installlocation") + "DarkCrusade.exe") || File.Exists(dcReg.GetValue("installlocation") + "\\DarkCrusade.exe"));
        }

        public void EnableTweak()
        {
            var unlockerDir = Path.Combine("LauncherFiles", "Addons", "Unlocker");

            if (!Directory.Exists(unlockerDir))
                throw new Exception("Could not find unlocker in LauncherFiles!");

            //TODO: Сделать более качественную проверку на наличие файлов в LauncherFiles


            var gamePath = PathFinder.GamePath;
            var targetDir = Path.Combine(gamePath, "Unlocker");
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }
            ExplorerExtensions.ClearFlagsInDirFiles(targetDir);


            var unlockerFiles = Directory.GetFiles(unlockerDir);

            foreach(var unlockerFile in unlockerFiles)
            {
                File.Copy(unlockerFile, Path.Combine(targetDir, Path.GetFileName(unlockerFile)), true);
            }

            string dowCdKey = "3697-5fd2-5a76-0e44";
            string waCdKey = "57a4-fae0-7611-1504-fa90";
            string dcCdKey = "8c34-5670-91a4-c2f2-bfca";
            string ssCdKey = "BEEF-B00B-BABE-CAFE-80F1";

            var thqKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).CreateSubKey("Software\\THQ");

            var dowReg = thqKey.CreateSubKey("Dawn Of War");
            dowReg.SetValue("CDKEY", dowCdKey);
            dowReg.SetValue("CDKEY_WXP", waCdKey);
            dowReg.SetValue("dawnofwar_ver", "1.51");
            dowReg.SetValue("installlocation", gamePath + "\\Unlocker\\");

            var dcReg = thqKey.CreateSubKey("Dawn Of War - Dark Crusade");
            dcReg.SetValue("CDKEY", dcCdKey);
            dcReg.SetValue("w40kcdkey", dowCdKey);
            dcReg.SetValue("wxpcdkey", waCdKey);
            dcReg.SetValue("installlocation", gamePath + "\\Unlocker\\");

            var ssReg = thqKey.CreateSubKey("Dawn Of War - Soulstorm");
            ssReg.SetValue("CDKEY", ssCdKey);
            ssReg.SetValue("w40kcdkey", dowCdKey);
            ssReg.SetValue("wxpcdkey", waCdKey);
            ssReg.SetValue("dxp2cdkey", dcCdKey);
        }

        public void DisableTweak()
        {
            var gamePath = PathFinder.GamePath;

            var targetDir = Path.Combine(gamePath, "Unlocker");

            if (Directory.Exists(targetDir))
            {
                ExplorerExtensions.ClearFlagsInDirFiles(targetDir);
                Directory.Delete(targetDir, true);
            }

            var softwareKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("Software", true);

            if (softwareKey.OpenSubKey("THQ") != null)
            {
                softwareKey.DeleteSubKeyTree("THQ");
            }

        }
    }
}
