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
        public string ModVersion => "1.5.5";
        
        public void DeployModAndModule(string gamePath)
        {
            try
            {
                File.Copy(Path.Combine(ModFolderName, "ThunderHawk.module"), Path.Combine(gamePath, "ThunderHawk.module"), true);

                var modPath = Path.Combine(gamePath, "ThunderHawk");

                if (Directory.Exists(modPath))
                    return;

                Directory.CreateDirectory(modPath);
                using (var archive = ZipFile.Open(Path.Combine(ModFolderName, "Mod.zip"), ZipArchiveMode.Read))
                    archive.ExtractToDirectory(gamePath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}
