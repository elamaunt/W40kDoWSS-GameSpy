using System;
using System.IO;
using System.IO.Compression;
using ThunderHawk.Core;

namespace ThunderHawk
{
    public class ThunderHawkModManager : IThunderHawkModManager
    {
        const string ModFolderName = @"Mod";

        public string ModName => "ThunderHawk";
        public string ModVersion => "1.5.7";
        
        public void DeployModAndModule(string gamePath)
        {
            try
            {
                if (Directory.Exists(Path.Combine("Mod", "ThunderHawk")))
                    Directory.Delete(Path.Combine("Mod", "ThunderHawk"), true);

                if (Directory.Exists(Path.Combine("Mod", ".git")))
                    Directory.Delete(Path.Combine("Mod", ".git"), true);

                var launcherModulePath = Path.Combine(ModFolderName, "ThunderHawk.module");
                var modulePath = Path.Combine(gamePath, "ThunderHawk.module");
                var modPath = Path.Combine(gamePath, "ThunderHawk");

                if (File.Exists(modulePath))
                {
                    var bytes = File.ReadAllBytes(modulePath);
                    var currentBytes = File.ReadAllBytes(launcherModulePath);

                    if (ArrayEquals(bytes, currentBytes) && Directory.Exists(modPath))
                        return;
                }

                File.Copy(launcherModulePath, modulePath, true);

                if (Directory.Exists(modPath))
                {
                    Directory.Delete(modPath, true);
                    return;
                }

                Directory.CreateDirectory(modPath);
                using (var archive = ZipFile.Open(Path.Combine(ModFolderName, "Mod.zip"), ZipArchiveMode.Read))
                    archive.ExtractToDirectory(gamePath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private bool ArrayEquals(byte[] array1, byte[] array2)
        {
            if (array1.Length != array2.Length)
                return false;

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                    return false;
            }

            return true;
        }
    }
}
