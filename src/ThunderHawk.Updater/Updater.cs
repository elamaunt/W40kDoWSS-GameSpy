using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Windows;

namespace ThunderHawk.Updater
{
    public static class Updater
    {
        static string ArchivePath;

        static void Main(string[] args)
        {
            Thread.Sleep(1000);

            //Debugger.Launch();

            // var fileName = $@"E:\Coding\Projects\W40kDoWSS-GameSpy\src\SteamSpy\bin\ThunderHawk-1.0-beta.zip";
            var fileName = args?.FirstOrDefault();
            
            if (fileName == null)
            {
                MessageBox.Show("Nothing to update. Launcher reinstallation may fix the problem." +
                    "Also you can install update manually from https://drive.google.com/drive/folders/1xi63t6lKE_EkldNWz9l-QM_8y99d_q9H");
                return;
            }

            ArchivePath = Path.GetFullPath(fileName);

            var archive = ZipFile.Open(fileName, ZipArchiveMode.Read);

            RemoveOldFiles();

            var entries = archive.Entries.ToArray();

            for (int i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];

                var path = entry.FullName;

                if (path.StartsWith("ThunderHawk/", System.StringComparison.OrdinalIgnoreCase))
                    path = path.Substring("ThunderHawk/".Length);

                if (string.IsNullOrWhiteSpace(path))
                    continue;

                try
                {
                    var folder = Path.GetDirectoryName(path);

                    if (!string.IsNullOrWhiteSpace(folder))
                        Directory.CreateDirectory(folder);

                    entry.ExtractToFile(path, true);
                }
                catch(Exception ex)
                {

                }
            }

            archive.Dispose();
            File.Delete(fileName);

            Process.Start(new ProcessStartInfo("ThunderHawk.exe")
            {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory
            });
        }

        static void RemoveOldFiles()
        {
            foreach (var directory in Directory.EnumerateDirectories(Environment.CurrentDirectory).ToArray())
            {
                if (ShouldSkipDirectory(directory))
                    continue;

                try
                {
                    Directory.Delete(directory, true);
                }
                catch(Exception ex)
                {

                }

            }

            foreach (var file in Directory.EnumerateFiles(Environment.CurrentDirectory).ToArray())
            {
                if (ShouldSkipFile(file))
                    continue;

                try
                {
                    File.Delete(file);
                }
                catch(Exception ex)
                {

                }
            }
        }

        static bool ShouldSkipDirectory(string directory)
        {
            //if (directory.EndsWith("mod\\", StringComparison.OrdinalIgnoreCase))
            //    return true;

            //if (directory.EndsWith("\\mod", StringComparison.OrdinalIgnoreCase))
            //    return true;

            return false;
        }

       static bool ShouldSkipFile(string file)
        {
            if (Path.GetFullPath(file) == ArchivePath)

            if (file.EndsWith(".config", StringComparison.OrdinalIgnoreCase))
                return true;

            if (file.EndsWith("ThunderHawk.Updater.exe", StringComparison.OrdinalIgnoreCase))
                return true;
            
            return false;
        }
    }
}
