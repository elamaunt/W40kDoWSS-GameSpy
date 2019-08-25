using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ThunderHawk.Core;
using ThunderHawk.StaticClasses.Soulstorm;

namespace ThunderHawk
{
    public class LaunchService : ILaunchService
    {
        public bool CanLaunchGame => !ProcessFinder.IsProcessFound() && PathFinder.IsPathFound();

        public string GamePath => PathFinder.GamePath;

        public Task LaunchGameAndWait()
        {
            if (!CanLaunchGame)
                throw new Exception();

            var tcs = new TaskCompletionSource<Process>();

            try
            {
                var patch1_2path = Path.Combine("LauncherFiles", "Addons", "Patch1.2");
                var exeFileName = Path.Combine(PathFinder.GamePath, patch1_2path, "soulstorm.exe");

                if (!File.Exists(exeFileName))
                {
                    var gamePatch1_2path = Path.Combine(PathFinder.GamePath, patch1_2path);
                    Directory.CreateDirectory(gamePatch1_2path);

                    foreach (var file in Directory.EnumerateFiles(patch1_2path))
                        File.Copy(file, Path.Combine(gamePatch1_2path, Path.GetFileName(file)));
                }

                var ssProc = Process.Start(new ProcessStartInfo(exeFileName, $"-nomovies -forcehighpoly")
                {
                    UseShellExecute = true,
                    WorkingDirectory = PathFinder.GamePath
                });

                if (Core.CoreContext.OptionsService.DisableFog)
                    FogRemover.DisableFog(ssProc);

                ssProc.Exited += (s, e) =>
                {
                    tcs.TrySetResult(ssProc);
                };
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                Logger.Error(ex);
                tcs.TrySetException(ex);
            }

            return tcs.Task;
        }

        public void SwitchGameToMod(string modName)
        {
            var localConfig = Path.Combine(GamePath, "Local.ini");

            if (!File.Exists(localConfig))
                return;

            var lines = File.ReadAllLines(localConfig);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("currentmoddc=", StringComparison.OrdinalIgnoreCase))
                    lines[i] = "currentmoddc=thunderhawk";
            }

            File.WriteAllLines(localConfig, lines);
        }
    }
}
