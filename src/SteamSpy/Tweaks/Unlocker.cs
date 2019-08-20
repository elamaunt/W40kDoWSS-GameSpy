using Microsoft.Win32;
using System;
using System.IO;
using ThunderHawk.Core;
using ThunderHawk.Core.Services;
using ThunderHawk.StaticClasses.Soulstorm;

namespace ThunderHawk.Tweaks
{
    public class Unlocker : ITweak
    {
        public TweakLevel TweakLevel { get; } = TweakLevel.Important;
        public string Title => Core.CoreContext.LangService.GetString("UnlockerTweakTitle");
        public string Description => Core.CoreContext.LangService.GetString("UnlockerTweakDescription");

        public void ApplyTweak()
        {
            var gamePath = PathFinder.GamePath;
            var unlockerDir = Path.Combine(Directory.GetCurrentDirectory(), "LauncherFiles\\Unlocker");

            if (!Directory.Exists(unlockerDir))
                throw new Exception("Could not find unlocker in LauncherFiles!");

            var targetDir = Path.Combine(gamePath, "Unlocker");

            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            var files = Directory.GetFiles(unlockerDir);

            foreach(var filePath in files)
            {
                var fileName = filePath.Substring(unlockerDir.Length + 1);
                File.Copy(Path.Combine(unlockerDir, fileName),
                    Path.Combine(targetDir, fileName), true);
            }

            string dowCdKey = "3697-5fd2-5a76-0e44";
            string waCdKey = "57a4-fae0-7611-1504-fa90";
            string dcCdKey = "8c34-5670-91a4-c2f2-bfca";
            string ssCdKey = "BEEF-B00B-BABE-CAFE-80F1";

            var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).CreateSubKey("Software\\THQ");

            var dowReg = baseKey.CreateSubKey("Dawn Of War");
            dowReg.SetValue("CDKEY", dowCdKey);
            dowReg.SetValue("CDKEY_WXP", waCdKey);
            dowReg.SetValue("dawnofwar_ver", "1.51");
            dowReg.SetValue("installlocation", gamePath + "\\Unlocker\\");

            var dcReg = baseKey.CreateSubKey("Dawn Of War - Dark Crusade");
            dcReg.SetValue("CDKEY", dcCdKey);
            dcReg.SetValue("w40kcdkey", dowCdKey);
            dcReg.SetValue("wxpcdkey", waCdKey);
            dcReg.SetValue("installlocation", gamePath + "\\Unlocker\\");

            var ssReg = baseKey.CreateSubKey("Dawn Of War - Soulstorm");
            ssReg.SetValue("CDKEY", ssCdKey);
            ssReg.SetValue("w40kcdkey", dowCdKey);
            ssReg.SetValue("wxpcdkey", waCdKey);
            ssReg.SetValue("dxp2cdkey", dcCdKey);
        }

        public bool CheckTweak()
        {
            var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("Software\\THQ");
            if (baseKey == null)
                return false;

            var dowReg = baseKey.OpenSubKey("Dawn Of War");
            var dcReg = baseKey.OpenSubKey("Dawn Of War - Dark Crusade");
            var ssReg = baseKey.OpenSubKey("Dawn Of War - Soulstorm");

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
    }
}
